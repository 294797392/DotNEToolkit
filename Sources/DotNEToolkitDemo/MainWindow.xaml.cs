using DotNEToolkit;
using DotNEToolkit.linux;
using DotNEToolkit.Modular;
using DotNEToolkitDemo;
using DotNEToolkitDemo.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        private void T_callback1(tail arg1, tail.tail_event_type arg2, object arg3, object arg4)
        {
            Console.WriteLine(arg3.ToString());
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
        }
    }
}
