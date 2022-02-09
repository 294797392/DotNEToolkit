using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 表示一个模块的状态
    /// </summary>
    public enum ModuleStatus
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        UnInitialized,

        /// <summary>
        /// 模块初始化成功
        /// </summary>
        Initialized,

        /// <summary>
        /// 初始化失败
        /// </summary>
        InitializeFailed,

        /// <summary>
        /// 初始化中
        /// </summary>
        Initializing,

        /// <summary>
        /// 初始化异常
        /// </summary>
        InitializeException
    }
}
