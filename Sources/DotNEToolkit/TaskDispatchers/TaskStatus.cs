using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 任务的状态
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 空闲状态，等待运行
        /// </summary>
        IDLE = 0,

        /// <summary>
        /// 运行中
        /// </summary>
        RUN,

        /// <summary>
        /// 运行成功
        /// </summary>
        PASS,

        /// <summary>
        /// 测试失败, NotGood
        /// </summary>
        FAIL,

        /// <summary>
        /// 跳过
        /// </summary>
        SKIP,

        /// <summary>
        /// 等待PASS或者FAIL
        /// </summary>
        WAIT,

        /// <summary>
        /// 异常情况
        /// </summary>
        EXCEPTION
    }
}