using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public enum IVTaskQueueType
    {
        /// <summary>
        /// 按顺序执行的队列
        /// </summary>
        Sequential = 0,

        /// <summary>
        /// 可以根据条件进行跳转的队列
        /// </summary>
        Conditional
    }

    public static class IVTaskQueueFactory
    {
        public static TaskQueue Create(IVTaskQueueType type)
        {
            switch (type)
            {
                case IVTaskQueueType.Sequential:
                    return new SequentialQueue();

                case IVTaskQueueType.Conditional:
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public abstract class TaskQueue
    {
        #region 实例变量

        #endregion

        #region 属性

        /// <summary>
        /// 执行的任务上下文信息
        /// </summary>
        public DispatcherContext Context { get; internal set; }

        /// <summary>
        /// 队列的输入参数
        /// </summary>
        public IDictionary Parameters { get; internal set; }

        /// <summary>
        /// 要执行的任务列表
        /// </summary>
        public List<TaskDefinition> TaskList { get { return this.Context.Tasks; } }

        #endregion

        #region 公开接口

        public virtual void Initialize() { }

        /// <summary>
        /// 从任务队列里取出下一个要执行的任务
        /// 如果队列里没有元素了，那么返回null
        /// </summary>
        /// <returns></returns>
        public abstract TaskDefinition Dequeue();

        /// <summary>
        /// 重置队列
        /// </summary>
        public abstract void Reset();

        #endregion
    }

    public class SequentialQueue : TaskQueue
    {
        protected int currentIndex = 0;

        public override TaskDefinition Dequeue()
        {
            if (this.TaskList.Count == currentIndex)
            {
                return null;
            }

            return this.TaskList[currentIndex++];
        }

        public override void Reset()
        {
            this.currentIndex = 0;
        }
    }
}
