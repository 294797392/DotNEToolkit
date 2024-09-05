using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
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
        Started,

        /// <summary>
        /// 所有工作流执行结束
        /// </summary>
        Completed,

        /// <summary>
        /// 执行的任务状态改变
        /// </summary>
        TaskStatusChanged,

        /// <summary>
        /// 输出消息
        /// </summary>
        Message
    }

    /// <summary>
    /// 任务状态改变的时候触发的参数
    /// </summary>
    public class TaskStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 任务的状态
        /// </summary>
        public TaskModuleStatus Status { get; set; }

        /// <summary>
        /// 状态改变了的Task实例
        /// </summary>
        public TaskModule Task { get; set; }
    }
}
