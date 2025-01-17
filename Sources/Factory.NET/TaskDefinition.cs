using DotNEToolkit.Modular;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    public enum PostTaskStrategy 
    {
        /// <summary>
        /// 该测试流程不是一个PostTask
        /// </summary>
        None,

        /// <summary>
        /// 总是运行，不管工作流的结果
        /// 这个选项在所有测试流运行完最后运行
        /// </summary>
        Always,

        /// <summary>
        /// 只有在所有工作流PASS之后再运行
        /// 这个选项在Always之前运行
        /// </summary>
        OnlyPass,

        /// <summary>
        /// 只有在工作流FAIL之后再运行
        /// 这个选项在Always之前运行
        /// </summary>
        OnlyFail
    }

    /// <summary>
    /// 存储任务详细信息
    /// </summary>
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
        /// 重试工作流之间的间隔时间
        /// </summary>
        [JsonProperty("retryInterval")]
        public int RetryInterval { get; set; }

        /// <summary>
        /// 工作流结束之后运行的策略
        /// </summary>
        [JsonProperty("postTask")]
        [EnumDataType(typeof(PostTaskStrategy))]
        public int PostTask { get; set; }

        /// <summary>
        /// 执行序号
        /// </summary>
        [JsonProperty("ordinal")]
        public int Ordinal { get; set; }

        public TaskDefinition()
        {
        }

        public override string ToString()
        {
            return string.Format("Name:{0}, ID:{1}", this.Name, this.ID);
        }
    }
}
