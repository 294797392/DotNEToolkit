using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    public static class ProcessCommFactory
    {
        public static ProcessCommClient CreateClient(ProcessCommTypes ipcType)
        {
            switch (ipcType)
            {
                case ProcessCommTypes.WCFNamedPipe: return new NamedPipeCommClient();

                default:
                    throw new NotImplementedException(string.Format("未实现{0}方式的IPC通信", ipcType));
            }
        }

        public static ProcessCommSvc CreateSvc(ProcessCommTypes ipcType)
        {
            switch (ipcType)
            {
                case ProcessCommTypes.WCFNamedPipe: return new NamedPipeCommSvc();

                default:
                    throw new NotImplementedException(string.Format("未实现{0}方式的IPC通信", ipcType));
            }
        }

        internal static string GetClientServiceHostURI(string baseURI)
        {
            return string.Format("{0}_clientHost", baseURI);
        }

        /// <summary>
        /// 创建一个WCF NamedPipe服务主机
        /// </summary>
        /// <returns></returns>
        internal static ServiceHost CreateNamedPipeServiceHost<TChannel>(string namedPipeUri, object channelImpl)
        {
            Uri uri = new Uri(namedPipeUri);
            ServiceHost namedPipeHost = new ServiceHost(channelImpl, uri);
            NetNamedPipeBinding namedPipeBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
            };
            namedPipeHost.AddServiceEndpoint(typeof(TChannel), namedPipeBinding, uri);
            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior()
            {
                HttpGetEnabled = false,
                HttpsGetEnabled = false
            };
            namedPipeHost.Description.Behaviors.Add(behavior);
            return namedPipeHost;
        }

        internal static ChannelFactory<TChannel> CreateNamedPipeChannelFactory<TChannel>(string namedPipeUri) 
            where TChannel : INamedPipeChannel
        {
            NetNamedPipeBinding namedPipeBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
            };
            EndpointAddress address = new EndpointAddress(namedPipeUri);
            ChannelFactory<TChannel> channelFactory = new ChannelFactory<TChannel>(namedPipeBinding, address);
            return channelFactory;
        }
    }
}
