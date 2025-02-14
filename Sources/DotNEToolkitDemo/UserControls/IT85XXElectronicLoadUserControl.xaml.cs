using DotNEToolkit;
using DotNEToolkit.Modular;
using Factory.NET.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// IT85XXElectronicLoadUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class IT85XXElectronicLoadUserControl : UserControl
    {
        private ModuleFactory moduleFactory;
        private IT85XXElectronicLoad electronicLoad;

        public IT85XXElectronicLoadUserControl()
        {
            InitializeComponent();

            this.InitializeUserControl();
        }

        private void InitializeUserControl() 
        {
            ModuleFactoryOptions moduleFactoryOptions = new ModuleFactoryOptions() 
            {
                ModuleList = JSONHelper.File2Object<List<ModuleDefinition>>("demo.json")
            };

            this.moduleFactory = ModuleFactory.CreateFactory(moduleFactoryOptions);
            this.moduleFactory.Initialize();
            this.electronicLoad = this.moduleFactory.LookupModule<IT85XXElectronicLoad>();

            List<IT85XXElectronicLoad.ElectronicLoadMode> electronicLoadModes = new List<IT85XXElectronicLoad.ElectronicLoadMode>() 
            {
                IT85XXElectronicLoad.ElectronicLoadMode.CW,
                IT85XXElectronicLoad.ElectronicLoadMode.CV,
                IT85XXElectronicLoad.ElectronicLoadMode.CR,
                IT85XXElectronicLoad.ElectronicLoadMode.CC
            };
            ComboBoxModes.ItemsSource = electronicLoadModes;
            ComboBoxModes.SelectedIndex = 0;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetInputMode(IT85XXElectronicLoad.InputMode.ON);
        }

        private void ButtonSetMode_Click(object sender, RoutedEventArgs e)
        {
            IT85XXElectronicLoad.ElectronicLoadMode electronicLoad = (IT85XXElectronicLoad.ElectronicLoadMode)ComboBoxModes.SelectedItem;

            this.electronicLoad.SetMode(electronicLoad);
        }

        private void ButtonSetVoltage_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetVoltage(int.Parse(TextBoxVol.Text));
        }

        private void ButtonSetPower_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetPower(int.Parse(TextBoxPow.Text));
        }

        private void ButtonSetCurrent_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetCurrent(int.Parse(TextBoxCur.Text));
        }
    }
}
