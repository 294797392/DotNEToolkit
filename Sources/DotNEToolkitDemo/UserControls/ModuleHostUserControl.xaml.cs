using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DotNEToolkitDemo.UserControls
{
    public class HostedImageQueueProxy : ModuleHostProxy
    {
        protected override void OnDataReceived(int cmdType, object cmdParam)
        {
        }
    }

    /// <summary>
    /// ModuleHostUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleHostUserControl : UserControl
    {
        private ModuleFactory moduleFactory;

        public ModuleHostUserControl()
        {
            InitializeComponent();

            this.InitializeUserControl();
        }

        private void InitializeUserControl()
        {
            this.moduleFactory = ModuleFactory.CreateFactory();
            this.moduleFactory.Initialized += ModuleFactory_Initialized;
            this.moduleFactory.ModuleEvent += ModuleFactory_ModuleEvent;
            this.moduleFactory.SetupAsync("ModuleHost.json");
        }

        private void ModuleFactory_ModuleEvent(ModuleFactory arg1, IModuleInstance arg2, int arg3, object arg4)
        {
        }

        private void ModuleFactory_Initialized(ModuleFactory factory)
        {
            ModuleHostProxy proxy = factory.LookupModule<ModuleHostProxy>();

            byte[] data = System.IO.File.ReadAllBytes("FlyPRO.exe");

            for (int i = 0; i < 50; i++)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => 
                {
                    while (true)
                    {
                        proxy.Send(1, Guid.NewGuid().ToString());
                    }
                });
            }
        }
    }
}
