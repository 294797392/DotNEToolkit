using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    /// <summary>
    /// 使用WCF NamedPipe进行进程间通信的客户端
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NamedPipeCommClient : ProcessCommClient, INamedPipeClientChannel
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("NamedPipeCommClient");

        #endregion

        #region 实例变量

        private ChannelFactory<INamedPipeServiceChannel> channelFactory;

        /// <summary>
        /// 与服务端进程进行通信的通道
        /// </summary>
        private INamedPipeServiceChannel serviceChannel;

        /// <summary>
        /// 用来接受服务端连接的ServiceHost
        /// </summary>
        private ServiceHost namedPipeHost;

        #endregion

        #region ProcessCommClient

        protected override int OnInitialize()
        {
            this.channelFactory = ProcessCommFactory.CreateNamedPipeChannelFactory<INamedPipeServiceChannel>(this.ServiceURI);
            this.channelFactory.Opened += this.ChannelFactory_Opened;
            this.channelFactory.Closed += this.ChannelFactory_Closed;

            this.namedPipeHost = ProcessCommFactory.CreateNamedPipeServiceHost<INamedPipeClientChannel>(ProcessCommFactory.GetClientServiceHostURI(this.ServiceURI), this);
            this.namedPipeHost.Opened += this.NamedPipeHost_Opened;
            this.namedPipeHost.Closed += this.NamedPipeHost_Closed;
            this.namedPipeHost.Open();

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            try
            {
                this.channelFactory.Close();
                this.namedPipeHost.Close();
            }
            catch (Exception ex)
            {
                logger.Error("关闭ChannelFactory异常", ex);
            }
        }

        public override int Connect()
        {
            try
            {
                this.NotifyStatusChanged(CommClientStates.Connecting);

                // 创建一个指向服务端ServiceHost的Channel并且连接
                this.serviceChannel = this.channelFactory.CreateChannel();

                // 调用Connect，此时服务端会连接客户端的ServiceHost
                this.serviceChannel.Connect();

                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                this.NotifyStatusChanged(CommClientStates.ConnectFailed);
                logger.Error(string.Format("WCF NamedPipe连接失败, uri = {0}", this.ServiceURI), ex);
                return DotNETCode.IPC_CONNECT_FAILED;
            }
        }

        public override void Disconnect()
        {
            try
            {
                // 调用Disconnect，此时服务端会断开与客户端ServiceHost的连接
                this.serviceChannel.Disconnect();

                // 断开与服务端ServiceHost连接的Channel
                ICommunicationObject commObject = this.serviceChannel as ICommunicationObject;
                commObject.Close();
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe断开连接失败, uri = {0}", this.ServiceURI), ex);
            }
        }

        public override int Send(int cmdType, byte[] data)
        {
            try
            {
                this.serviceChannel.SendBytes(cmdType, data);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe向服务器发送消息异常, uri = {0}", this.ServiceURI), ex);
                return DotNETCode.IPC_SEND_FAILED;
            }
        }

        public override int Send(int cmdType, string cmdParam)
        {
            try
            {
                this.serviceChannel.SendString(cmdType, cmdParam);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe向服务器发送消息异常, uri = {0}", this.ServiceURI), ex);
                return DotNETCode.IPC_SEND_FAILED;
            }
        }

        public override int Send(int cmdType, object param)
        {
            try
            {
                this.serviceChannel.SendObject(cmdType, param);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe向服务器发送消息异常, uri = {0}", this.ServiceURI), ex);
                return DotNETCode.IPC_SEND_FAILED;
            }
        }

        #endregion

        #region INamedPipeClientChannel

        /// <summary>
        /// 当服务端发送数据过来的时候触发
        /// </summary>
        /// <param name="data"></param>
        public void SendBytes(int cmdType, byte[] cmdParam)
        {
            this.NotifyDataReceived(cmdType, cmdParam);
        }

        public void SendString(int cmdType, string cmdParam)
        {
            this.NotifyDataReceived(cmdType, cmdParam);
        }

        public void SendObject(int cmdType, object cmdParam)
        {
            this.NotifyDataReceived(cmdType, cmdParam);
        }

        #endregion

        #region 事件处理器

        private void ChannelFactory_Closed(object sender, EventArgs e)
        {
            logger.InfoFormat("NamedPipe 客户端 -> 服务器连接已关闭", this.ServiceURI);
            this.NotifyStatusChanged(CommClientStates.Disconnected);
        }

        private void ChannelFactory_Opened(object sender, EventArgs e)
        {
            logger.InfoFormat("NamedPipe 客户端 -> 服务器连接已连接", this.ServiceURI);
            this.NotifyStatusChanged(CommClientStates.Connected);
        }

        private void NamedPipeHost_Closed(object sender, EventArgs e)
        {
            logger.InfoFormat("客户端NamedPipeHost已启动, {0}", this.ServiceURI);
        }

        private void NamedPipeHost_Opened(object sender, EventArgs e)
        {
            logger.InfoFormat("客户端NamedPipeHost已关闭, {0}", this.ServiceURI);
        }


        #endregion
    }
}
