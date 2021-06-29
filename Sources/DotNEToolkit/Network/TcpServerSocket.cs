using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DotNEToolkit.Network
{
    public class TcpServerSocket
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TcpServerSocket");

        #endregion

        #region 实例变量

        private TcpListener socket;

        private bool isRunning;

        #endregion

        #region 公开接口

        public int Start(int port)
        {
            if (port <= 0 || port > 65535)
            {
                logger.ErrorFormat("端口不正确, {0}", port);
                return DotNETCode.INVALID_PARAMS;
            }

            try
            {
                this.socket = new TcpListener(IPAddress.Any, port);
                this.socket.Start();

                System.Threading.Tasks.Task.Factory.StartNew(this.AcceptClientThreadProc);

                logger.DebugFormat("TCP服务启动成功，监听端口 = {0}", port);

                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error("启动TCP服务异常", ex);
                return DotNETCode.UNKNOWN_EXCEPTION;
            }
        }

        public void Stop()
        {
            
        }

        #endregion

        private void AcceptClientThreadProc()
        {
            while (this.isRunning)
            {
                TcpClient client = this.socket.AcceptTcpClient();

                logger.InfoFormat("客户端已连接, {0}", client);
            }
        }
    }
}
