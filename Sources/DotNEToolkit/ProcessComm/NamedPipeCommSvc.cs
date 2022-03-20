using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    /// <summary>
    /// 使用WCF NamedPipe实现的IPC服务端
    /// 
    /// 参考：https://www.cnblogs.com/zhili/p/WCFCallbackOperacation.html
    /// 
    /// WCF支持服务将调用返回给它的客户端。
    /// 在回调期间，服务成为了客户端，而客户端成为了服务。
    /// 在WCF中，并不是所有的绑定都支持回调操作，只有具有双向能力的绑定才能够用于回调。
    /// 例如，HTTP本质上是与连接无关的，所以它不能用于回调，因此我们不能基于basicHttpBinding和wsHttpBinding绑定使用回调。
    /// WCF为NetTcpBinding和NetNamedPipeBinding提供了对回调的支持，因为TCP和IPC协议都支持双向通信。
    /// 为了让Http支持回调，WCF提供了WsDualHttpBinding绑定，它实际上设置了两个Http通道：一个用于从客户端到服务的调用，另一个用于服务到客户端的调用。
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NamedPipeCommSvc : ProcessCommSvc, INamedPipeServiceChannel
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("NamedPipeCommSvc");

        #endregion

        #region 实例变量

        private ServiceHost namedPipeHost;

        // 连接客户端使用的ChannelFactory
        private ChannelFactory<INamedPipeClientChannel> channelFactory;

        private INamedPipeClientChannel clientChannel;

        #endregion

        #region ProcessCommSvc

        protected override int OnInitialize()
        {
            this.namedPipeHost = ProcessCommFactory.CreateNamedPipeServiceHost<INamedPipeServiceChannel>(this.URI, this);
            this.namedPipeHost.Opened += this.NamedPipeHost_Opened;
            this.namedPipeHost.Closed += this.NamedPipeHost_Closed;

            this.channelFactory = ProcessCommFactory.CreateNamedPipeChannelFactory<INamedPipeClientChannel>(ProcessCommFactory.GetClientServiceHostURI(this.URI));
            this.channelFactory.Opened += this.ChannelFactory_Opened;
            this.channelFactory.Closed += this.ChannelFactory_Closed;

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            try
            {
                this.channelFactory.Close();
            }
            catch (Exception ex)
            {
                logger.Error("关闭ChannelFactory异常", ex);
            }
        }

        public override int Start()
        {
            try
            {
                this.namedPipeHost.Open();
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("启动NamedPipe服务异常, uri = {0}", this.URI), ex);
                return DotNETCode.IPC_START_FAILED;
            }
        }

        public override void Stop()
        {
            try
            {
                this.namedPipeHost.Close();
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("关闭NamedPipe服务异常, uri = {0}", this.URI), ex);
            }
        }

        /// <summary>
        /// 向客户端发消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override int Send(int cmdType, byte[] cmdParam)
        {
            try
            {
                this.clientChannel.SendBytes(cmdType, cmdParam);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe向客户端发送消息异常, uri = {0}", this.URI), ex);
                return DotNETCode.IPC_SEND_FAILED;
            }
        }

        public override int Send(int cmdType, object cmdParam)
        {
            try
            {
                this.clientChannel.SendObject(cmdType, cmdParam);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe向客户端发送消息异常, uri = {0}", this.URI), ex);
                return DotNETCode.IPC_SEND_FAILED;
            }
        }

        public override int Send(int cmdType, string cmdParam)
        {
            try
            {
                this.clientChannel.SendString(cmdType, cmdParam);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("WCF NamedPipe向客户端发送消息异常, uri = {0}", this.URI), ex);
                return DotNETCode.IPC_SEND_FAILED;
            }
        }

        #endregion

        #region INamedPipeServiceChannel

        /// <summary>
        /// 客户端连接的时候触发
        /// 如果客户端连接，那么说明客户端此时也启动了一个NamedPipeServiceHost
        /// 那么此时服务端就可以连接客户端的ServiceHost，实现双向通信的功能
        /// </summary>
        /// <returns></returns>
        public int Connect()
        {
            // 运行到这里说明客户端在连接服务
            // 那么服务也需要去连接客户端
            try
            {
                this.clientChannel = this.channelFactory.CreateChannel();
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error("服务器连接客户端异常", ex);
                return DotNETCode.IPC_CONNECT_FAILED;
            }
        }

        /// <summary>
        /// 客户端断开连接的时候触发
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // 关闭与客户端的ServiceHost的连接
                ICommunicationObject clientComm = this.clientChannel as ICommunicationObject;
                clientComm.Close();
            }
            catch (Exception ex)
            {
                logger.Error("服务器断开客户端连接异常", ex);
            }
        }

        /// <summary>
        /// 客户端有数据发送过来的时候触发
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

        private void NamedPipeHost_Closed(object sender, EventArgs e)
        {
            logger.InfoFormat("NamedPipe服务已关闭, uri = {0}", this.URI);
        }

        private void NamedPipeHost_Opened(object sender, EventArgs e)
        {
            logger.InfoFormat("NamedPipe服务已启动, uri = {0}", this.URI);
        }

        private void ChannelFactory_Closed(object sender, EventArgs e)
        {
            logger.InfoFormat("NamedPipe 服务器 -> 客户端连接已关闭, uri = {0}", this.URI);
        }

        private void ChannelFactory_Opened(object sender, EventArgs e)
        {
            logger.InfoFormat("NamedPipe 服务器 -> 客户端连接已连接, uri = {0}", this.URI);
        }

        #endregion
    }
}
