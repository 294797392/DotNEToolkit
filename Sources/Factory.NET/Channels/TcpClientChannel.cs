﻿using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime;
using DotNEToolkit;

namespace Factory.NET.Channels
{
    public class TcpClientChannel : ChannelBase
    {
        private const string DefaultServerIPAddress = "192.168.1.1";
        private const int DefaultServerPort = 1018;

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TcpClientIODriver");

        #endregion

        #region 实例变量

        private TcpClient tcpClient;
        protected NetworkStream stream;
        private string ipAddress;
        private int port;
        private StreamReader sr;
        private StreamWriter sw;
        private bool isConnected;

        #endregion

        #region AbstractIODriver

        public override ChannelTypes Type { get { return ChannelTypes.TcpClient; } }

        protected override int OnInitialize()
        {
            this.ipAddress = this.GetParameter<string>("IPAddress", DefaultServerIPAddress);
            this.port = this.GetParameter<int>("Port", DefaultServerPort);

            IPAddress ip;
            if (!IPAddress.TryParse(this.ipAddress, out ip))
            {
                logger.ErrorFormat("无效的IP地址格式:{0}", this.ipAddress);
                return ResponseCode.INVALID_PARAMETER;
            }

            if (this.port <= 0 || this.port >= 65535)
            {
                logger.ErrorFormat("无效的端口号:{0}", this.port);
                return ResponseCode.INVALID_PARAMETER;
            }

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(this.ipAddress), this.port);
            this.tcpClient = new TcpClient();
            this.tcpClient.ReceiveBufferSize = base.readBufferSize;
            this.tcpClient.ReceiveTimeout = base.readTimeout;
            this.tcpClient.SendBufferSize = base.writeBufferSize;
            this.tcpClient.SendTimeout = base.writeTimeout;
            this.tcpClient.Connect(ep);

            logger.DebugFormat("TcpClientChannel - {0}:{1}连接成功", ipAddress, port);

            this.stream = this.tcpClient.GetStream();
            this.sr = new StreamReader(stream);
            this.sw = new StreamWriter(stream);

            this.isConnected = true;

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            try
            {
                this.isConnected = false;

                if (this.sr != null)
                {
                    this.sr.Close();
                    this.sr = null;
                }

                if (this.sw != null)
                {
                    this.sw.Close();
                    this.sw = null;
                }

                if (this.tcpClient != null)
                {
                    this.tcpClient.Close();
                    this.tcpClient = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("断开与设备的Tcp连接异常", ex);
            }
        }

        public override int ReadBytes(byte[] bytes, int offset, int len)
        {
            return this.stream.Read(bytes, offset, len);
        }

        public override byte[] ReadBytesFull(int size)
        {
            return StreamUtils.ReadFull(this.stream, size);
        }

        public override void WriteBytes(byte[] bytes)
        {
            this.stream.Write(bytes, 0, bytes.Length);
            this.stream.Flush();
        }

        public override void WriteLine(string cmd)
        {
            cmd = string.Format("{0}{1}", cmd, base.newLine);
            byte[] data = Encoding.ASCII.GetBytes(cmd);
            this.stream = this.tcpClient.GetStream();
            this.stream.Write(data, 0, data.Length);
        }

        public override string ReadLine()
        {
            return this.sr.ReadLine();
        }

        public override void ClearExisting()
        {
            try
            {
                int size = this.tcpClient.Available;
                if (size > 0)
                {
                    byte[] buff = new byte[size];
                    size = this.stream.Read(buff, 0, size);
                    //string msg = new string(buff);
                    //logger.InfoFormat("缓冲区中剩余数据:{0}", msg);
                }
            }
            catch (Exception ex)
            {
                logger.Error("清除接收缓冲区异常", ex);
            }
        }

        #endregion
    }
}