using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    /// <summary>
    /// 定义测试流程的状态
    /// </summary>
    public enum TaskModuleStatus
    {
        /// <summary>
        /// 等待运行
        /// </summary>
        WAIT,

        /// <summary>
        /// 运行中
        /// </summary>
        RUN,

        /// <summary>
        /// 测试成功
        /// </summary>
        PASS,

        /// <summary>
        /// 测试失败
        /// </summary>
        FAIL,

        /// <summary>
        /// 跳过测试
        /// </summary>
        SKIP,

        /// <summary>
        /// 运行测试流程出现异常
        /// </summary>
        EXCEPTION
    }
}
