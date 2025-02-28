using DotNEToolkit;
using DotNEToolkit.Modular;
using DotNEToolkit.Utility;
using Factory.NET.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private ITECH85XXLoader electronicLoad;
        private ZDotCH2221HOutputModule ch2221Module;
        private PK9015M pk9015m;

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

            this.electronicLoad = this.moduleFactory.LookupModule<ITECH85XXLoader>();
            if (this.electronicLoad != null)
            {
                this.electronicLoad.SetControlMode(ITECH85XXLoader.ControlMode.Remote);
            }

            this.ch2221Module = this.moduleFactory.LookupModule<ZDotCH2221HOutputModule>();

            this.pk9015m = this.moduleFactory.LookupModule<PK9015M>();

            List<ITECH85XXLoader.ElectronicLoadMode> electronicLoadModes = new List<ITECH85XXLoader.ElectronicLoadMode>()
            {
                ITECH85XXLoader.ElectronicLoadMode.CW,
                ITECH85XXLoader.ElectronicLoadMode.CV,
                ITECH85XXLoader.ElectronicLoadMode.CR,
                ITECH85XXLoader.ElectronicLoadMode.CC
            };
            ComboBoxModes.ItemsSource = electronicLoadModes;
            ComboBoxModes.SelectedIndex = 0;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            this.electronicLoad.SetInputMode(ITECH85XXLoader.InputMode.ON);
        }

        private void ButtonSetMode_Click(object sender, RoutedEventArgs e)
        {
            ITECH85XXLoader.ElectronicLoadMode electronicLoad = (ITECH85XXLoader.ElectronicLoadMode)ComboBoxModes.SelectedItem;

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

            this.ch2221Module.WriteCoils(0, 31, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

            //this.ch2221Module.WriteCoils(0, 32, new byte[] { 0x00, 0x00, 0x00, 0x00 });
        }

        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            byte[] values = this.pk9015m.ReadHoldingRegister(1, 0x02, 1);

            values = values.Reverse().ToArray();

            double v = (double)BitConverter.ToUInt16(values, 0) / 100;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    byte[] bytes = this.pk9015m.ReadHoldingRegister(1, 0x03, 1);

                    bytes = bytes.Reverse().ToArray();

                    ushort value = BitConverter.ToUInt16(bytes, 0);
                    double d = (double)value / 10000 * v;

                    Console.WriteLine(ByteUtils.ToString(bytes));

                    Thread.Sleep(1000);
                }
            });
        }
    }
}
