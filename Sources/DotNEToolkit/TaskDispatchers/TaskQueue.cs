using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    public abstract class TaskQueue
    {
        #region 实例变量

        #endregion

        #region 属性

        /// <summary>
        /// 队列的输入参数
        /// </summary>
        public IDictionary Parameters { get; internal set; }

        /// <summary>
        /// 要执行的任务列表
        /// </summary>
        public List<TaskDefinition> TaskList { get; internal set; }

        /// <summary>
        /// 存储任务的输出参数
        /// </summary>
        public Dictionary<string, IDictionary> TaskProperties { get; internal set; }

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

    public class ConditionalQueue : SequentialQueue
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("JumpQueue");

        #endregion

        #region Inner Class

        /// <summary>
        /// 跳转条件
        /// </summary>
        public class Condition
        {
            /// <summary>
            /// 初始的任务ID
            /// </summary>
            [JsonProperty("TaskID")]
            public string TaskID { get; set; }

            /// <summary>
            /// 任务的属性
            /// </summary>
            [JsonProperty("PropertyName")]
            public string PropertyName { get; set; }

            /// <summary>
            /// 属性值
            /// </summary>
            [JsonProperty("PropertyValue")]
            public string PropertyValue { get; set; }

            /// <summary>
            /// 要跳转到的任务ID
            /// </summary>
            [JsonProperty("TargetTaskID")]
            public string TargetTaskID { get; set; }
        }

        #endregion

        #region 实例变量

        /// <summary>
        /// 存储上一次运行的任务ID
        /// </summary>
        private string previouseTaskID;

        /// <summary>
        /// 跳转条件
        /// </summary>
        private List<Condition> conditions;

        #endregion

        public override void Initialize()
        {
            base.Initialize();

            this.conditions = this.Parameters.JArray2List<Condition>("Conditions");
        }

        public override void Reset()
        {
            base.Reset();

            this.previouseTaskID = string.Empty;
        }

        #region IVTaskQueue

        public override TaskDefinition Dequeue()
        {
            if (string.IsNullOrEmpty(this.previouseTaskID))
            {
                // 上次运行的任务ID是空的，说明是第一次运行，直接返回下一个任务
                return base.Dequeue();
            }

            Condition condition = this.conditions.FirstOrDefault(v => v.TaskID == this.previouseTaskID);
            if (condition == null)
            {
                // 没有跳转条件，直接返回下一个任务
                return base.Dequeue();
            }

            // 这里必定不为空，即使任务没有输出参数，那么也会实例化
            IDictionary properties = this.TaskProperties[condition.TaskID];

            if (!properties.Contains(condition.PropertyName))
            {
                // 不存在输出参数（可能是配置文件里配置错了？），那么直接返回下一个
                logger.WarnFormat("不存在任务跳转条件里的任务输出参数:{0}，请检查配置文件", condition.PropertyName);
                return base.Dequeue();
            }

            object value = properties[condition.PropertyName];
            if (value.ToString() != condition.PropertyValue)
            {
                logger.WarnFormat("任务跳转条件里的任务输出参数和真正的任务输出参数不一致，直接运行下一个任务");
                return base.Dequeue();
            }

            // 一致，那么跳转下一个工作流
            TaskDefinition toDequeue = this.TaskList.FirstOrDefault(v => v.ID == condition.TargetTaskID);
            if (toDequeue == null)
            {
                logger.WarnFormat("任务跳转条件成立，但是找不到要跳转的任务ID, 请检查配置文件");
                return base.Dequeue();
            }

            // 执行跳转
            this.currentIndex = this.TaskList.IndexOf(toDequeue);

            return base.Dequeue();
        }

        #endregion
    }
}