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
    public class TcpServiceIODriver : AbstractIODriver
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TcpServiceIODriver");

        private const int DefaultServerPort = 8000;

        private const int MaxReceiveTimes = 3;
        
        #endregion

        #region 实例变量

        private bool connected;
        private TcpClient tcpClient;
        private TcpListener tcpService;
        protected NetworkStream stream;
        private int port;
        private StreamReader sr;
        private StreamWriter sw;

        #endregion

        #region IVServiceBase

        public override int Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            this.port = this.InputParameters.GetValue<int>("Port", DefaultServerPort);

            if (this.port <= 0 || this.port >= 65535)
            {
                logger.ErrorFormat("无效的端口号:{0}", this.port);
                return ResponseCode.INVALID_PARAMETER;
            }

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, this.port);
            this.tcpService = new TcpListener(ep);
            try
            {
                this.tcpService.Start();
            }
            catch (Exception ex)
            {
                this.tcpService.Stop();
                this.tcpService = null;
                logger.ErrorFormat("TcpServiceIODriver启动失败, port = {0}, Message = {1}", port, ex);
                return ResponseCode.UNKOWN_EXCEPTION;
            }

            System.Threading.Tasks.Task.Factory.StartNew(this.AcceptClientThreadProc);

            logger.DebugFormat("TcpServiceIODriver - {0}:{1}监听成功", IPAddress.Any, port);

            return ResponseCode.SUCCESS;
        }

        public override void Release()
        {
            base.Release();
        }

        #endregion

        #region AbstractIODriver

        public override IODriverTypes Type { get { return IODriverTypes.TcpService; } }

        public override bool IsConnected()
        {
            return this.connected;
        }

        public override int WriteBytes(byte[] buffer)
        {
            try
            {
                this.stream.Write(buffer, 0, buffer.Length);
                return ResponseCode.SUCCESS;
            }
            catch (IOException ex)
            {
                logger.Error("WriteBuffer异常", ex);
                return ResponseCode.IODRV_WRITE_FAILED;
            }
            catch (ObjectDisposedException ex)
            {
                logger.Error("WriteBuffer异常", ex);
                return ResponseCode.IODRV_WRITE_FAILED;
            }
        }

        public override int ReadBytes(byte[] buffer)
        {
            try
            {
                this.stream.Read(buffer, 0, buffer.Length);
                return ResponseCode.SUCCESS;
            }
            catch (IOException ex)
            {
                logger.Error("ReadBuffer异常", ex);
                return ResponseCode.IODRV_READ_FAILED;
            }
            catch (ObjectDisposedException ex)
            {
                logger.Error("ReadBuffer异常", ex);
                return ResponseCode.IODRV_READ_FAILED;
            }
        }

        public override int WriteLine(string cmd)
        {
            try
            {
                cmd = string.Format("{0}{1}", cmd, base.newLine);
                byte[] data = Encoding.ASCII.GetBytes(cmd);
                this.stream = this.tcpClient.GetStream();
                this.stream.Write(data, 0, data.Length);
                return ResponseCode.SUCCESS;
            }
            catch (IOException ex)
            {
                logger.Error("WriteLine异常", ex);
                return ResponseCode.IODRV_WRITE_FAILED;
            }
            catch (ObjectDisposedException ex)
            {
                logger.Error("WriteLine异常", ex);
                return ResponseCode.IODRV_WRITE_FAILED;
            }
        }

        public override int ReadLine(out string data)
        {
            data = null;

            try
            {
                data = this.sr.ReadLine();
                return ResponseCode.SUCCESS;
            }
            catch (IOException ex)
            {
                logger.Error("ReadLine异常", ex);
                return ResponseCode.IODRV_READ_FAILED;
            }
            catch (ObjectDisposedException ex)
            {
                logger.Error("ReadLine异常", ex);
                return ResponseCode.IODRV_READ_FAILED;
            }
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
                logger.Error("ClearReceivingBuffer异常");
            }
        }

        #endregion

        public void CloseClient()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Close();
                this.tcpClient = null;
            }

            if (this.sr != null)
            {
                this.sr.Dispose();
                this.sr = null;
            }

            if (this.sw != null)
            {
                this.sw.Dispose();
                this.sw = null;
            }

            this.connected = false;
        }

        private void AcceptClientThreadProc()
        {
            while (true)
            {
                try
                {
                    this.tcpClient = this.tcpService.AcceptTcpClient();

                    this.tcpClient.ReceiveTimeout = this.readTimeout;
                    this.tcpClient.SendTimeout = this.writeTimeout;
                    this.tcpClient.ReceiveBufferSize = this.readBufferSize;
                    this.tcpClient.SendBufferSize = this.writeBufferSize;
                    this.stream = this.tcpClient.GetStream();
                    this.sr = new StreamReader(stream);
                    this.sw = new StreamWriter(stream);

                    this.connected = true;
                }
                catch(Exception ex)
                {
                    logger.Error("接收客户端连接异常", ex);
                }
            }
        }
    }
}
