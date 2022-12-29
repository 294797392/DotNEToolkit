using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供进程相关的帮助函数
    /// </summary>
    public static class ProcessUtils
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ProcessUtils");

        /// <summary>
        /// 等待某个进程的主窗口句柄创建成功
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool WaitForMainWindowHandleCreated(this Process proc, int timeout)
        {
            int elapsed = 0;
            while (proc.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(10);
                elapsed += 10;
                if (elapsed >= 5000)
                {
                    logger.ErrorFormat("等待进程主窗口句柄创建超时");
                    return false;
                }
            }

            return true;
        }

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

        /// <summary>
        /// 创建一个标准输入输出都重定向了的进程
        /// </summary>
        /// <param name="exe"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Process CreateProcess(string exe, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = exe,
                Arguments = arguments
            };

            return Process.Start(psi);
        }

        public static void WaitProcessClosed(int pid)
        {
            while (true)
            {
                Process[] procList = Process.GetProcesses();
                Process proc = procList.FirstOrDefault(v => v.Id == pid);
                if (proc == null)
                {
                    logger.InfoFormat("进程{0}已退出", pid);
                    return;
                }
                else
                {
                    logger.InfoFormat("进程{0}还未退出..", pid);
                    Thread.Sleep(2000);
                }
            }
        }

        public static void WaitProcessClosed(string procName)
        {
            Process[] processes = Process.GetProcessesByName(procName);
            if (processes == null || processes.Length == 0)
            {
                return;
            }

            WaitProcessClosed(processes[0].Id);
        }
    }
}
