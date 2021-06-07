using DotNEToolkit;
using DotNEToolkit.Excels;
using DotNEToolkit.linux;
using DotNEToolkit.Modular;
using DotNEToolkitDemo;
using DotNEToolkitDemo.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DotNETClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModuleFactory factory;

        private void Foreach(int v, object userData)
        {
            Console.WriteLine("开始运行:{0}", v);
            //if (v == 99)
            if (v == 98)
            {
                System.Threading.Thread.Sleep(10000);
                Console.WriteLine("99个数据处理完毕");
            }
            else
            {
                System.Threading.Thread.Sleep(new Random().Next(100, 900));
            }
        }

        private void Callback(object userData)
        {
            Console.WriteLine("全部运行完了");
        }

        public MainWindow()
        {
            InitializeComponent();

            //List<int> source = new List<int>();
            //for (int i = 0; i < 100; i++)
            //{
            //    source.Add(i);
            //}

            //DotNEToolkit.Parallel.Foreach<int>(source, 5, Foreach, null, Callback);

            //ExcelSheet sheet;
            //Excel.QuickRead("636356149796417536_CMSXJ48H_200_20210511.xlsx", ReadOptions.KeepEmptyCell, out sheet);

            //Excel.QuickWrite(sheet, "1.xls", ExcelVersions.Xls);

            Console.ReadLine();

            //List<ModuleDefinition> modules = JSONHelper.ParseFile<List<ModuleDefinition>>("Modules/Modules.json");

            //List<string> metaFiles = new List<string>()
            //{
            //    "Modules/ModuleMetadatas.json"
            //};

            //this.factory = ModuleFactory.CreateFactory();
            //this.factory.RegisterModuleMetadata(metaFiles);
            //this.factory.SetupModulesAsync(modules, 1000);

            //Task.Factory.StartNew(() =>
            //{
            //    System.Threading.Thread.Sleep(5000);

            //    DemoModule module = this.factory.LookupModule<DemoModule>();

            //    Console.WriteLine("123");
            //});

            //tail t = new tail();
            //t.callback += T_callback;
            //t.addopt(tail.tail_options.follow);
            //t.addopt(tail.tail_options.readline);
            //t.addopt(tail.tail_options.retry);
            //t.period = 50;
            //t.bufsize = 16384;
            //t.start(@"E:\chuangmi\factoryIV\FactoryIV.Build\Rolling.log");
        }

        private void T_callback(tail arg1, tail.tail_event_type arg2, object data)
        {
            Console.WriteLine(data);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IntPtr handle = Win32API.GetDesktopWindow();

            Win32API.SendMessage(handle, Win32API.WM_APPCOMMAND, handle, 10 * 65536);
        }
    }
}
