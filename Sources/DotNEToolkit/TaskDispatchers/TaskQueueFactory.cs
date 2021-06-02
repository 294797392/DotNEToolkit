using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    public enum TaskQueueType
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

    public static class TaskQueueFactory
    {
        public static TaskQueue Create(TaskQueueType type)
        {
            switch (type)
            {
                case TaskQueueType.Sequential:
                    return new SequentialQueue();

                case TaskQueueType.Conditional:
                    return new ConditionalQueue();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}

