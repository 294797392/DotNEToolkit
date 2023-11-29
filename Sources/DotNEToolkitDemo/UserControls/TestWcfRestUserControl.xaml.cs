using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace DotNEToolkitDemo.UserControls
{
    [ServiceContract]
    public interface DemoServiceContract
    {
        [OperationContract, WebGet]
        string Smoke();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServiceContractImpl : DemoServiceContract
    {
        public string Smoke()
        {
            return "ok";
        }
    }

    /// <summary>
    /// TestWcfRestUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TestWcfRestUserControl : System.Windows.Controls.UserControl
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("TestWcfRestUserControl");

        private const int DefaultMaxArrayLength = 4194304;

        public TestWcfRestUserControl()
        {
            InitializeComponent();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            // demo：http://127.0.0.1:8900/demo
            string svcUrl = TextBoxUrl.Text;
            if (string.IsNullOrEmpty(svcUrl))
            {
                logger.ErrorFormat("SetupWCFHttpService失败, url为空");
                return;
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

                logger.InfoFormat("启动WCF WebService: {0}", svcUri);

                ServiceContractImpl serviceContractImpl = new ServiceContractImpl();

                WebServiceHost serviceHost = new WebServiceHost(serviceContractImpl, svcUri);
                serviceHost.Opened += ServiceHost_Opened; ;
                serviceHost.Closed += ServiceHost_Closed;

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

                serviceHost.AddServiceEndpoint(typeof(DemoServiceContract), binding, svcUri);
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                logger.Error("启动WCF WebService异常", ex);
            }
        }

        private void ServiceHost_Closed(object sender, EventArgs e)
        {
            logger.InfoFormat("Service Closed");
        }

        private void ServiceHost_Opened(object sender, EventArgs e)
        {
            logger.InfoFormat("Service Opened");
        }
    }
}
