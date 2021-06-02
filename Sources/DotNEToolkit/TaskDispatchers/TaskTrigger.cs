using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 任务触发器
    /// </summary>
    public class TaskTrigger
    {
        /// <summary>
        /// 触发器的名字
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 要执行触发器的任务ID
        /// </summary>
        [JsonProperty("TaskID")]
        public string TaskID { get; set; }

        /// <summary>
        /// 属性名字
        /// </summary>
        [JsonProperty("Property")]
        public string Property { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        [JsonProperty("Value")]
        public object Value { get; set; }

        /// <summary>
        /// 触发器满足条件的时候要执行的任务列表
        /// </summary>
        [JsonProperty("TaskList")]
        public List<TaskDefinition> TaskList { get; set; }
    }
}
