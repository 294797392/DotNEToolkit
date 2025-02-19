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
        private ITECH85XX electronicLoad;
        private ZDotCH2221H ch2221Module;

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
            this.electronicLoad = this.moduleFactory.LookupModule<ITECH85XX>();
            if (this.electronicLoad != null)
            {
                this.electronicLoad.SetControlMode(ITECH85XX.ControlMode.Remote);
            }

            this.ch2221Module = this.moduleFactory.LookupModule<ZDotCH2221H>();


            List<ITECH85XX.ElectronicLoadMode> electronicLoadModes = new List<ITECH85XX.ElectronicLoadMode>()
            {
                ITECH85XX.ElectronicLoadMode.CW,
                ITECH85XX.ElectronicLoadMode.CV,
                ITECH85XX.ElectronicLoadMode.CR,
                ITECH85XX.ElectronicLoadMode.CC
            };
            ComboBoxModes.ItemsSource = electronicLoadModes;
            ComboBoxModes.SelectedIndex = 0;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetInputMode(ITECH85XX.InputMode.ON);
        }

        private void ButtonSetMode_Click(object sender, RoutedEventArgs e)
        {
            ITECH85XX.ElectronicLoadMode electronicLoad = (ITECH85XX.ElectronicLoadMode)ComboBoxModes.SelectedItem;

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

        private void ButtonHigh_Click(object sender, RoutedEventArgs e)
        {
            //byte[] v = this.ch2221Module.ReadCoils(0x00, 32);

            //this.ch2221Module.WriteCoils(0x00, 32, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

            //this.ch2221Module.WriteCoils(0x00, 32, new byte[] { 0x00, 0x00, 0x00, 0x00 });

            this.ch2221Module.WriteCoils(0, 32, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            //this.ch2221Module.WriteCoils(0, 32, new byte[] { 0x00, 0x00, 0x00, 0x00 });
        }
    }
}
