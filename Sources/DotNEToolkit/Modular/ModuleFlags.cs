﻿using System;
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
    }
}
