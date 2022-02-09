using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供Log4Net的帮助函数
    /// </summary>
    public static class Log4net
    {
        private const string ExternalLog4netConfig = "log4net.xml";

        /// <summary>
        /// 初始化Log4net库
        /// 默认使用程序根目录下的log4net.xml配置文件初始化log4net
        /// </summary>
        public static void InitializeLog4net(string configPath = ExternalLog4netConfig)
        {
            try
            {
                FileInfo configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath));
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
    }
}
