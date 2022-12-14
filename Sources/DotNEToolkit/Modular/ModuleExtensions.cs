using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 模块的附加功能
    /// </summary>
    public static class ModuleExtensions
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleExtensions");

        private const int DefaultMaxArrayLength = 4194304;

        /// <summary>
        /// 快速初始化一个WCF WebService
        /// </summary>
        /// <typeparam name="TServiceContract"></typeparam>
        /// <param name="webModule"></param>
        /// <param name="openedDlg"></param>
        /// <param name="closedDlg"></param>
        /// <param name="svchost"></param>
        /// <returns></returns>
        public static int SetupWCFHttpService<TServiceContract>(this ModuleBase webModule, EventHandler openedDlg, EventHandler closedDlg, out WebServiceHost svchost)
        {
            svchost = null;

            // demo：http://127.0.0.1:8900/demo
            string svcUrl = webModule.GetParameter<string>("url", string.Empty);
            if (string.IsNullOrEmpty(svcUrl))
            {
                logger.ErrorFormat("SetupWCFHttpService失败, url为空");
                return DotNETCode.FAILED;
            }

            // 建立启动WCF服务
            WebHttpBinding binding = new WebHttpBinding();
            binding.OpenTimeout = TimeSpan.FromSeconds(10);
            binding.CloseTimeout = TimeSpan.FromSeconds(60);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 655350000;

            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxStringContentLength = 20971520;
            quotas.MaxArrayLength = DefaultMaxArrayLength;
            quotas.MaxBytesPerRead = 20971520;
            quotas.MaxDepth = 20971520;
            quotas.MaxNameTableCharCount = 20971520;
            binding.ReaderQuotas = quotas;

            try
            {
                Uri svcUri = new Uri(svcUrl);

                logger.InfoFormat("启动WCF WebService: {0}, {1}", webModule.Name, svcUri);

                WebServiceHost serviceHost = new WebServiceHost(webModule, svcUri);
                serviceHost.Opened += openedDlg;
                serviceHost.Closed += closedDlg;

                ServiceMetadataBehavior metadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadataBehavior == null)
                {
                    metadataBehavior = new ServiceMetadataBehavior();
                    serviceHost.Description.Behaviors.Add(metadataBehavior);
                }

                ServiceThrottlingBehavior throttle = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (throttle == null)
                {
                    throttle = new ServiceThrottlingBehavior();
                    throttle.MaxConcurrentCalls = 512;
                    throttle.MaxConcurrentSessions = 256;
                    throttle.MaxConcurrentInstances = 256;
                    serviceHost.Description.Behaviors.Add(throttle);
                }

                serviceHost.AddServiceEndpoint(typeof(TServiceContract), binding, svcUri);
                serviceHost.Open();

                svchost = serviceHost;

                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error("启动WCF WebService异常", ex);
                return DotNETCode.FAILED;
            }
        }
    }
}
