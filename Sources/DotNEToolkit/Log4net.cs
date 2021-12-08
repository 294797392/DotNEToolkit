using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    public static class Log4net
    {
        private static string ExternalLog4netConfig = "log4net.xml";

        public static void InitializeLog4net()
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
    }
}
