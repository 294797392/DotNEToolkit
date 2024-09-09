using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 模块的标志位
    /// 0x00 - 0xFF为DotNEToolkit预留使用
    /// </summary>
    public static class ModuleFlags
    {
        /// <summary>
        /// 没有标志位
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// 模块被禁用
        /// 告诉ModuleFactory不要加载该模块
        /// 该模块不会被加到缓存里
        /// </summary>
        public const int Disabled = 1;
    }
}
