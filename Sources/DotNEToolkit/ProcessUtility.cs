using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    public static class ProcessUtility
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ProcessUtility");

        public static void StartProcessAsUser()
        {
            
        }

        /// <summary>
        /// 根据进程名字关闭一个进程
        /// </summary>
        /// <param name="procName"></param>
        public static void KillProcess(string procName)
        {
            Process[] processes = Process.GetProcessesByName(procName);

            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("关闭进程异常, {0}, {1}", process.ProcessName, ex);
                }
            }
        }
    }
}
