using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DotNEToolkit.Network
{
    /// <summary>
    /// 指定数据包的格式
    /// </summary>
    public enum PacketFormatOptions
    {
    }

    /// <summary>
    /// 封装TCP客户端收包，发包逻辑
    /// </summary>
    public class TcpClientSocket
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TcpClientSocket");

        #endregion

        #region 公开事件

        /// <summary>
        /// 当收到了一个完整的数据包的时候触发
        /// </summary>
        public event Action<TcpClientSocket, TcpPacket> PacketReceived;

        /// <summary>
        /// 网络状态改变的时候触发
        /// </summary>
        public event Action<TcpClientSocket, NetworkStates> StatusChanged;

        #endregion

        #region 实例变量

        private TcpClient socket;

        #endregion

        #region 属性

        /// <summary>
        /// 是否自动重连
        /// </summary>
        public bool AutoReconnect { get; set; }

        #endregion

        #region 构造方法

        public TcpClientSocket()
        {
            this.AutoReconnect = true;
            this.socket = new TcpClient();
        }

        #endregion

        #region 公开接口

        public int ConnectAsync(string ip, int port)
        {
            IPAddress serverAddress;
            if (!IPAddress.TryParse(ip, out serverAddress))
            {
                return DotNETCode.INVALID_PARAMS;
            }

            if (port <= 0 || port > 65535)
            {
                return DotNETCode.INVALID_PARAMS;
            }

            IPEndPoint remoteEP = new IPEndPoint(serverAddress, port);
            this.socket.BeginConnect(ip, port, this.ConnectedCallback, this.socket);

            return DotNETCode.SUCCESS;
        }

        public void Disconnect()
        {
            if (this.socket == null)
            {
                return;
            }

            try
            {
                this.socket.Close();
            }
            catch (Exception ex)
            {
                logger.Error("断开客户端连接异常", ex);
            }
        }

        #endregion

        #region 实例方法

        private void ConnectedCallback(IAsyncResult iar)
        {
        }

        internal void NotifyPacketReceived(TcpPacket packet)
        {
            if (this.PacketReceived != null)
            {
                this.PacketReceived(this, packet);
            }
        }

        internal void NotifyStatusChanged(NetworkStates state)
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, state);
            }
        }

        #endregion
    }
}

