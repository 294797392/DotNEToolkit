using System;
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
        public TaskDefinition TaskDefinition { get; set; }

        /// <summary>
        /// 测试结果
        /// </summary>
        public TaskModuleStatus Status { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }
    }
}
