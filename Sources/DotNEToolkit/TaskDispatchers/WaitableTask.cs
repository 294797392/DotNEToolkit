using DotNEToolkit.Modular.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 表示一个可等待的任务
    /// </summary>
    public abstract class WaitableTask : Task
    {
        #region 实例变量

        private AutoResetEvent waitEvent;

        #endregion

        /// <summary>
        /// 让任务继续运行
        /// </summary>
        [ModuleAction(Name = "继续运行", Manually = true)]
        public void Continue()
        {
            if (this.waitEvent == null)
            {
                return;
            }

            this.waitEvent.Set();
            this.waitEvent.Dispose();
            this.waitEvent = null;
        }

        /// <summary>
        /// 等待任务
        /// </summary>
        public void Wait()
        {
            if (this.waitEvent == null)
            {
                this.waitEvent = new AutoResetEvent(false);
            }
            this.waitEvent.WaitOne();
        }
    }
}
