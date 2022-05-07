using DotNEToolkit;
using DotNEToolkit.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    /// <summary>
    /// 封装Tcp客户端通信对象
    /// </summary>
    public class TcpClientCommObject : CommObject
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("TcpClientCommObject");

        private const string KEY_IPADDRESS = "IPAddress";
        private const string KEY_PORT = "Port";

        #region 实例变量

        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;

        private NetworkStream stream;

        #endregion

        #region ModuleBase

        #endregion

        #region CommunicationObject

        public override int Open()
        {
            string ip = base.InputParameters.GetValue<string>(KEY_IPADDRESS);
            int port = base.InputParameters.GetValue<int>(KEY_PORT);

            IPAddress ipaddr;
            if (!IPAddress.TryParse(ip, out ipaddr))
            {
                logger.ErrorFormat("IP地址格式错误, {0}", ip);
                return DotNETCode.INVALID_PARAMS;
            }

            if (port <= 0 || port >= 65535)
            {
                logger.ErrorFormat("非法的端口号, {0}", port);
                return DotNETCode.INVALID_PARAMS;
            }

            IPEndPoint remoteEP = new IPEndPoint(ipaddr, port);

            this.tcpClient = new TcpClient();
            try
            {
                this.tcpClient.Connect(remoteEP);
            }
            catch (Exception ex)
            {
                logger.Error("连接服务器异常", ex);
                return DotNETCode.FAILED;
            }

            this.stream = this.tcpClient.GetStream();
            this.reader = new StreamReader(this.stream);
            this.writer = new StreamWriter(this.stream);
            this.writer.AutoFlush = true;           // 注意必须加上这行代码，不然发送的时候会有延迟

            logger.InfoFormat("连接服务器成功, {0}", remoteEP);

            return DotNETCode.SUCCESS;
        }

        public override void Close()
        {
            this.reader.Close();
            this.reader.Dispose();

            this.writer.Close();
            this.writer.Dispose();

            this.stream.Close();

            this.tcpClient.Close();
        }

        public override bool IsOpened()
        {
            return this.tcpClient != null && this.tcpClient.Connected;
        }

        public override string ReadLine()
        {
            return this.reader.ReadLine();
        }

        public override void WriteLine(string line)
        {
            this.writer.WriteLine(line);
        }

        public override byte[] ReadBytes(int size)
        {
            return this.stream.ReadFull(size);
        }

        public override void WriteBytes(byte[] bytes)
        {
            this.stream.Write(bytes, 0, bytes.Length);
        }

        #endregion
    }
}
