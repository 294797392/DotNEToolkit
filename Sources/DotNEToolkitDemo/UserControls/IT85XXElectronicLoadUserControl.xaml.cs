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
        private ITECH85XXElectronicLoad electronicLoad;

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
            this.electronicLoad = this.moduleFactory.LookupModule<ITECH85XXElectronicLoad>();

            List<ITECH85XXElectronicLoad.ElectronicLoadMode> electronicLoadModes = new List<ITECH85XXElectronicLoad.ElectronicLoadMode>() 
            {
                ITECH85XXElectronicLoad.ElectronicLoadMode.CW,
                ITECH85XXElectronicLoad.ElectronicLoadMode.CV,
                ITECH85XXElectronicLoad.ElectronicLoadMode.CR,
                ITECH85XXElectronicLoad.ElectronicLoadMode.CC
            };
            ComboBoxModes.ItemsSource = electronicLoadModes;
            ComboBoxModes.SelectedIndex = 0;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetInputMode(ITECH85XXElectronicLoad.InputMode.ON);
        }

        private void ButtonSetMode_Click(object sender, RoutedEventArgs e)
        {
            ITECH85XXElectronicLoad.ElectronicLoadMode electronicLoad = (ITECH85XXElectronicLoad.ElectronicLoadMode)ComboBoxModes.SelectedItem;

            this.electronicLoad.SetMode(electronicLoad);
        }

        private void ButtonSetVoltage_Click(object sender, RoutedEventArgs e)
        {
            int vol;
            if (!int.TryParse(TextBoxVol.Text, out vol))
            {
                MessageBox.Show("请输入正确的电压");
                return;
            }

            this.electronicLoad.SetVoltage(vol);
        }

        private void ButtonSetPower_Click(object sender, RoutedEventArgs e)
        {
            int power;
            if (!int.TryParse(TextBoxPow.Text, out power))
            {
                MessageBox.Show("请输入正确的功率");
                return;
            }

            this.electronicLoad.SetPower(power);
        }

        private void ButtonSetCurrent_Click(object sender, RoutedEventArgs e)
        {
            int cur;
            if (!int.TryParse(TextBoxCur.Text, out cur))
            {
                MessageBox.Show("请输入正确的电流");
                return;
            }

            this.electronicLoad.SetCurrent(cur);
        }
    }
}
