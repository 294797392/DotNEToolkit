using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public static class FactoryUtils
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("FactoryUtils");

        /// <summary>
        /// 检查是否有ADB设备连接
        /// </summary>
        /// <returns>如果有链接则返回true，没有设备链接则返回false</returns>
        public static bool CheckAdbDevice()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "adb.exe",
                Arguments = "devices",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            try
            {
                Process process = Process.Start(processStartInfo);
                string message = process.StandardOutput.ReadToEnd();

                return message.Contains("\tdevices");
            }
            catch (Exception ex)
            {
                logger.Error("检查ADB设备状态异常", ex);
                return false;
            }
        }

    }
}
