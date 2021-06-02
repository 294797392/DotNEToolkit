using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供对调用函数的工具函数
    /// </summary>
    public static class DelegateUtility
    {
        public static bool ContinuousInvoke<T1>(Func<T1, bool> action, T1 t1, int max_times)
        {
            if (max_times == 0)
            {
                return action(t1);
            }
            else
            {
                for (int i = 0; i <= max_times; i++)
                {
                    if (!action(t1))
                    {
                        continue;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public delegate bool Delegate1(string p1, string p2, out string p3);
        public delegate bool Delegate6(string p1);

        /// <summary>
        /// 用重试机制去调用一个函数
        /// </summary>
        /// <param name="dlg"></param>
        /// <param name="max_times"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static bool Invoke(Delegate1 dlg, int max_times, string p1, string p2, out string p3)
        {
            p3 = null;
            for (int i = 1; i <= max_times; i++)
            {
                if (!dlg(p1, p2, out p3))
                {
                    continue;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 使用超时和重试机制调用一个函数
        /// </summary>
        /// <param name="dlg"></param>
        /// <param name="p1"></param>
        /// <param name="timeout"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static bool InvokeWithTimeout(Delegate6 dlg, string p1, int timeout, int interval)
        {
            DateTime start = DateTime.Now;
            while (true)
            {
                if ((DateTime.Now - start).TotalMilliseconds > timeout)
                {
                    return false;
                }

                if (!dlg(p1))
                {
                    Thread.Sleep(interval);
                    continue;
                }

                return true;
            }
        }
    }
}
