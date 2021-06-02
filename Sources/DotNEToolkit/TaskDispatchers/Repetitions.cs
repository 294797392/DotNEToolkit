using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 工作流的执行次数
    /// </summary>
    public enum Repetitions
    {
        /// <summary>
        /// 按顺序只执行一遍工作流
        /// </summary>
        Single = 0,

        /// <summary>
        /// 按顺序执行完一遍工作流后继续从头开始执行
        /// </summary>
        Continuously
    }
}
