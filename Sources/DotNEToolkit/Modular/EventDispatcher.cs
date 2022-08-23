using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 事件分发器实现了
    /// 1. 对各个模块之间事件的管理（注册事件，发布事件）
    /// 2. 事件订阅者可以根据Token订阅不同的事件，当某个事件发布的时候，不会发布给所有的订阅者，而是只有拥有对应Token的订阅者才可以被触发
    /// </summary>
    public class EventDispatcher
    {
        public void SubscribeEvent()
        {

        }

        public void PublishEvent()
        {

        }
    }
}
