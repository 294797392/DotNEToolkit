using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    /// <summary>
    /// 存储测试流程的测试结果
    /// </summary>
    public class TaskResult
    {
        /// <summary>
        /// 测试流程元数据信息
        /// </summary>
        public TaskDefinition TaskDefinition { get; set; }

        /// <summary>
        /// 测试结果
        /// </summary>
        public TaskModuleStatus Status { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 测试流程输出参数
        /// </summary>
        public IDictionary Properties { get; internal set; }

        public TaskResult() { }
    }
}
