using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    public interface IEventSubscriber
    {
    }

    public interface IEventPublisher : IModuleInstance
    {
        Dictionary<int, List<Subscribtion>> EventSubscribtions { get; }
    }

    /// <summary>
    /// 实现一个参与事件订阅-发布系统的模块
    /// </summary>
    public abstract class EventableModule : ModuleBase, IEventSubscriber, IEventPublisher
    {
        /// <summary>
        /// 存储该模块的事件订阅信息
        /// eventType -> 订阅列表
        /// </summary>
        public Dictionary<int, List<Subscribtion>> EventSubscribtions { get; private set; }

        public EventableModule()
        {
            this.EventSubscribtions = new Dictionary<int, List<Subscribtion>>();
        }
    }
}