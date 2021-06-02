using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    public enum TaskDispatcherEvent
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        IDLE,

        /// <summary>
        /// 开始执行工作流
        /// </summary>
        ExecutionStarted,

        /// <summary>
        /// 引擎执行失败
        /// </summary>
        ExecutionFailure,

        /// <summary>
        /// 引擎执行成功
        /// </summary>
        ExecutionSuccess,

        /// <summary>
        /// 执行的任务状态改变
        /// </summary>
        TaskStatusChanged
    }

    /// <summary>
    /// 任务状态改变的时候触发的参数
    /// </summary>
    public class TaskStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 任务的状态
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// 状态改变了的Task实例
        /// </summary>
        public Task Task { get; set; }
    }
}
