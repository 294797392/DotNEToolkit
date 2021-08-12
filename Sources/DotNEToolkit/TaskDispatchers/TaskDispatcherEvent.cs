using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    public enum TaskDispatcherStatus
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        IDLE,

        /// <summary>
        /// 正在运行工作流
        /// </summary>
        RUN,

        /// <summary>
        /// 引擎执行失败
        /// </summary>
        FAIL,

        /// <summary>
        /// 引擎执行成功
        /// </summary>
        PASS,
    }

    /// <summary>
    /// 任务状态改变的时候触发的参数
    /// </summary>
    public class TaskStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 任务的状态
        /// </summary>
        public WorkflowStatus Status { get; set; }

        /// <summary>
        /// 状态改变了的Task实例
        /// </summary>
        public WorkflowTask Task { get; set; }
    }
}
