using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNEToolkit.Modular;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 存储任务详细信息
    /// </summary>
    [JsonObject("Task")]
    public class TaskDefinition : ModuleDefinition
    {
        /// <summary>
        /// 执行工作流的延迟时间
        /// </summary>
        [JsonProperty("Delay")]
        public int Delay { get; set; }

        /// <summary>
        /// 执行工作流的最多的尝试次数
        /// </summary>
        [JsonProperty("RetryTimes")]
        public int RetryTimes { get; set; }

        public TaskDefinition()
        {
        }

        public override string ToString()
        {
            return string.Format("Name:{0}, TypeID:{1}, ID:{2}", this.Name, this.TypeID, this.ID);
        }
    }
}
