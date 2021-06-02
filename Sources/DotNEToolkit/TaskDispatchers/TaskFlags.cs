using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    public enum TaskFlags : ulong
    {
        /// <summary>
        /// 忽略任务执行错误
        /// 如果忽略，则工作流运行失败后继续运行下一个工作流
        /// 如果不忽略，工作流运行失败后结束工作流
        /// </summary>
        IgnoreFailure = 1
    }
}