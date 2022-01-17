using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 提供一组预定义的模块事件
    /// </summary>
    public static class ModuleEvent
    {
        /// <summary>
        /// 模块向外部输出消息
        /// </summary>
        public const int MessageEvent = 0x00;

        /// <summary>
        /// 模块状态发生改变
        /// </summary>
        public const int StatusChanged = 0x01;
    }
}
