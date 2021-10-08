using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DotNEToolkit.Extentions;
using System.IO;

namespace DotNETClient
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static string ExternalLog4netConfig = "log4net.xml";

        private static void InitializeLog4net()
        {
            try
            {
                FileInfo configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExternalLog4netConfig));
                if (configFile.Exists)
                {
                    log4net.Config.XmlConfigurator.ConfigureAndWatch(configFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("初始化日志异常, {0}", ex);
            }
        }

        static App()
        {
            InitializeLog4net();
        }
    }
}
