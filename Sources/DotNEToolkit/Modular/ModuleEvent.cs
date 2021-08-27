using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 提供一组预定义的模块事件
    /// </summary>
    public class ModuleEvent
    {
        /// <summary>
        /// 模块向外部输出消息
        /// </summary>
        public const string MessageEvent = "ModuleEventMessage";

        /// <summary>
        /// 模块状态发生改变
        /// </summary>
        public const string StatusChanged = "ModuleStatusChanged";
    }
}
