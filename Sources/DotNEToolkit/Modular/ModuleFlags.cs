using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 模块的标志位
    /// </summary>
    public static class ModuleFlags
    {
        /// <summary>
        /// 模块被禁用
        /// 告诉ModuleFactory不要加载该模块
        /// 该模块不会被加到缓存里
        /// </summary>
        public const int Disabled = 1;

        /// <summary>
        /// 不初始化
        /// 该模块会被加到缓存里，但是不调用初始化
        /// </summary>
        public const int NotInitial = 2;
    }
}
