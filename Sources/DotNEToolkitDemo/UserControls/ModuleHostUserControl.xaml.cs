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
            ModuleFactoryOptions moduleFactoryOptions = new ModuleFactoryOptions() 
            {
                AsyncInitializing = false,
                ModuleList = new List<ModuleDefinition>()
            };

            this.moduleFactory = ModuleFactory.CreateFactory(moduleFactoryOptions);
            this.moduleFactory.Initialized += ModuleFactory_Initialized;
        }

        private void ModuleFactory_ModuleEvent(ModuleFactory arg1, IModuleInstance arg2, int arg3, object arg4)
        {
        }

        private void ModuleFactory_Initialized(ModuleFactory factory)
        {
        }
    }
}
