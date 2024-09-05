using DotNEToolkit.Modular;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public enum PostTaskStrategy 
    {
        /// <summary>
        /// 总是运行，不管工作流的结果
        /// </summary>
        Always,

        /// <summary>
        /// 只有在所有工作流PASS之后再运行
        /// </summary>
        OnlyPass,

        /// <summary>
        /// 只有在工作流FAIL之后再运行
        /// </summary>
        OnlyFail
    }

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

        /// <summary>
        /// 是否在所有工作流运行结束之后运行
        /// </summary>
        [JsonProperty("post")]
        public bool PostTask { get; set; }

        /// <summary>
        /// 工作流结束之后运行的策略
        /// </summary>
        [JsonProperty("postStrategy")]
        public int PostTaskStrategy { get; set; }

        public TaskDefinition()
        {
        }

        public override string ToString()
        {
            return string.Format("Name:{0}, ID:{1}", this.Name, this.ID);
        }
    }
}
