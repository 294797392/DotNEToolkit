using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotNEToolkit
{
    public class WindowScanner
    {
        #region 类变量

        private const int Interval = 300;

        private static log4net.ILog logger = log4net.LogManager.GetLogger("WindowScanner");

        #endregion

        #region 实例变量

        private bool isRunning;

        private Thread thread;

        #endregion

        #region 公开接口

        public void Start()
        {
            if (this.isRunning)
            {
                return;
            }

            this.isRunning = true;
            this.thread = new Thread(this.ThreadProc);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public void Stop()
        {
            this.isRunning = false;
            this.thread.Join();
        }

        #endregion

        #region 实例方法

        private void ThreadProc()
        {
            while (this.isRunning)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    logger.Error("ThreadProc异常", ex);
                }
                finally
                {
                    Thread.Sleep(Interval);
                }
            }
        }

        #endregion
    }
}
