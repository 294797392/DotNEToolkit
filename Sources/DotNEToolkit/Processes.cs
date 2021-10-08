using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供进程相关的帮助函数
    /// </summary>
    public static class Processes
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Processes");

        /// <summary>
        /// 清空某个进程里的输出缓冲区里的数据
        /// </summary>
        /// <param name="proc">要清空的进程</param>
        public static void ClearExisting(Process proc)
        {
            if (!proc.StartInfo.RedirectStandardOutput)
            {
                return;
            }

            int peeked = -1;
            while ((peeked = proc.StandardOutput.Peek()) != -1)
            {
                proc.StandardOutput.Read();
            }
        }

        /// <summary>
        /// 根据进程名字关闭一个进程
        /// </summary>
        /// <param name="procName">要关闭的进程名字，注意该进程名字不带后缀名（.exe）</param>
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
