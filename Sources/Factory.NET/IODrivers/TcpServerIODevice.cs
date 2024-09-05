using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Factory.NET.IODrivers
{
    public class TcpServerIODevice : AbstractIODriver
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TcpServerIODevice");

        private static byte[] HeartbeatBytes = new byte[1] { (byte)'\0' };

        #endregion

        #region 实例变量

        private int port;
        private TcpListener tcpServer;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private StreamReader sr;
        private StreamWriter sw;
        private bool isRunning = false;

        #endregion

        #region TcpServerIODevice

        public override IODriverTypes Type { get { return IODriverTypes.TcpServer; } }

        public override bool Initialize(IDictionary settings)
        {
            base.Initialize(settings);
            this.port = settings.GetValue<int>("Port");
            string address = settings.GetValue<string>("IPAddress");
            try
            {
                IPAddress ipAddr = string.IsNullOrEmpty(address) ? IPAddress.Any : IPAddress.Parse(address);
                this.tcpServer = new TcpListener(ipAddr, this.port);
                this.tcpServer.Start();
            }
            catch (Exception ex)
            {
                logger.Error("初始化TCP服务异常", ex);
                return false;
            }
            return true;
        }

        public override void ClearReceivingBuffer()
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

        public override bool Connect()
        {
            try
            {
                this.tcpClient = this.tcpServer.AcceptTcpClient();
                logger.InfoFormat("TcpClient Connected, {0}", this.tcpClient.Client.RemoteEndPoint);
                this.tcpClient.ReceiveTimeout = base.readTimeout;
                this.stream = this.tcpClient.GetStream();
                this.sr = new StreamReader(this.stream);
                this.sw = new StreamWriter(this.stream);
            }
            catch (Exception ex)
            {
                logger.Error("接收客户端连接异常", ex);
                return false;
            }
            return true;
        }

        public override bool Disconnect()
        {
            try
            {
                this.CloseClient();
                this.isRunning = false;
                this.tcpServer.Stop();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("断开与设备的Tcp连接异常", ex);
            }
            return true;
        }

        public override bool ReadBuffer(byte[] buffer)
        {
            try
            {
                this.stream.Read(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("ReadBuffer异常", ex);
                return false;
            }
        }

        public override bool ReadLine(out string line)
        {
            line = null;
            try
            {
                line = this.sr.ReadLine();
                logger.DebugFormat("read:{0}", line);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("ReadLine异常", ex);
                return false;
            }
        }

        public override bool TestConnection()
        {
            if (this.stream == null)
            {
                return false;
            }

            try
            {
                this.stream.Write(HeartbeatBytes, 0, HeartbeatBytes.Length);
            }
            catch (Exception ex)
            {
                this.CloseClient();
                logger.ErrorFormat("TestConnection失败", ex.Message);
                return false;
            }

            return true;
        }

        public override bool WriteBuffer(byte[] buffer)
        {
            try
            {
                this.stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                logger.Error("WriteBuffer异常", ex);
                return false;
            }

            return true;
        }

        public override bool WriteLine(string line)
        {
            line = string.Format("{0}\n", line);

            try
            {
                byte[] data = Encoding.ASCII.GetBytes(line);
                this.stream.Write(data, 0, data.Length);
                logger.DebugFormat("write:{0}", line);
            }
            catch (Exception ex)
            {
                logger.Error("WriteLine异常", ex);
                return false;
            }

            return true;
        }

        #endregion

        private void CloseClient()
        {
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

            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null;
            }

            if (this.tcpClient != null)
            {
                this.tcpClient.Close();
                this.tcpClient = null;
            }
        }
    }
}
