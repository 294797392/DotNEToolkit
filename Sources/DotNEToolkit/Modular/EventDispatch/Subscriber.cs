using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.EventDispatch
{
    /// <summary>
    /// 存储一个订阅者的信息
    /// </summary>
    internal class Subscriber
    {
        /// <summary>
        /// 订阅者的ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 事件处理器
        /// </summary>
        public ModuleEventHandler EventHandler { get; set; }
    }
}

