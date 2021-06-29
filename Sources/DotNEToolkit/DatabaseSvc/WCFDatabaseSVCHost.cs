using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotNEToolkit.DatabaseSvc
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IWCFDatabaseService
    {
        /// <summary>
        /// 冒烟测试
        /// </summary>
        /// <returns></returns>
        [OperationContract, WebGet(UriTemplate = "smoke")]
        string Smoke();

        [OperationContract, WebInvoke(UriTemplate = "db")]
        string HandleRequest();
    }

    /// <summary>
    /// WCFDatabaseSVCHost类
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WCFDatabaseSVCHost : DatabaseSVCHost, IWCFDatabaseService
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("WCFDatabaseSVCHost");

        #region 实例变量

        private WebServiceHost serviceHost;

        #endregion

        public override DatabaseSVCType Type => DatabaseSVCType.WCF;

        #region 构造方法

        internal WCFDatabaseSVCHost()
        { }

        #endregion

        #region DatabaseSVCHost

        public override int Start()
        {
            return this.StartService();
        }

        #endregion

        #region 实例方法

        private int StartService()
        {
            // 建立启动WCF服务
            WebHttpBinding binding = new WebHttpBinding();
            binding.OpenTimeout = TimeSpan.FromSeconds(10);
            binding.CloseTimeout = TimeSpan.FromSeconds(60);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 655350000;

            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxStringContentLength = 20971520;
            quotas.MaxArrayLength = 16384; // 16K
            quotas.MaxBytesPerRead = 20971520;
            quotas.MaxDepth = 20971520;
            quotas.MaxNameTableCharCount = 20971520;
            binding.ReaderQuotas = quotas;

            try
            {
                Uri svcUri = new Uri(string.Format("http://127.0.0.1:{0}/{1}", this.port, this.rootPath));

                logger.InfoFormat("启动WCF WebService: {0}", svcUri);

                this.serviceHost = new WebServiceHost(this, svcUri);
                this.serviceHost.Opened += this.ServiceHost_Opened; ;

                ServiceMetadataBehavior metadataBehavior = this.serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadataBehavior == null)
                {
                    metadataBehavior = new ServiceMetadataBehavior();
                    this.serviceHost.Description.Behaviors.Add(metadataBehavior);
                }

                ServiceThrottlingBehavior throttle = this.serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (throttle == null)
                {
                    throttle = new ServiceThrottlingBehavior();
                    throttle.MaxConcurrentCalls = 512;
                    throttle.MaxConcurrentSessions = 256;
                    throttle.MaxConcurrentInstances = 256;
                    this.serviceHost.Description.Behaviors.Add(throttle);
                }

                this.serviceHost.AddServiceEndpoint(typeof(IWCFDatabaseService), binding, svcUri);
                this.serviceHost.Open();
            }
            catch (Exception ex)
            {
                logger.Fatal("启动WCF WebService异常", ex);
                return DotNETCode.SUCCESS;
            }

            return DotNETCode.UNKNOWN_EXCEPTION;
        }

        #endregion

        #region 事件处理器

        private void ServiceHost_Opened(object sender, EventArgs e)
        {
            logger.Info("WCF ServiceHost启动成功");
        }

        #endregion

        #region IWCFDatabaseService

        public string Smoke()
        {
            return "svc is running...";
        }

        public string HandleRequest()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}