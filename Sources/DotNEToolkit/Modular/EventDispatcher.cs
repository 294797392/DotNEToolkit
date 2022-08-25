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

    internal class Subscribtion
    {
        /// <summary>
        /// 订阅该事件的对象
        /// </summary>
        public IEventSubscriber Subscriber { get; set; }

        /// <summary>
        /// 处理该事件的处理器
        /// </summary>
        public ModuleEventDlg EventHandler { get; set; }
    }

    /// <summary>
    /// 事件分发器实现了
    /// 1. 对各个模块之间事件的管理（注册事件，发布事件）
    /// 2. 事件订阅者可以根据Token订阅不同的事件，当某个事件发布的时候，不会发布给所有的订阅者，而是只有拥有对应Token的订阅者才可以被触发
    /// 
    /// 老版本的模块事件全部通过ModuleFactory透传到外部，这样会导致几个问题：
    /// 1. 每次处理模块事件都需要对模块类型进行判断，才能知道是哪个模块触发了事件。要不就得把模块的事件定义成不相同的Id，这样就得知道每个模块的Id的范围，很难管理
    /// 2. 当模块触发一个事件的时候，所有注册了该事件的订阅者都会处理该事件（即使订阅者不需要处理）
    /// 
    /// 使用新的Subscribe - Publish模式来解决上述出现的两个问题
    /// 
    /// 一个事件可被多个订阅者订阅
    /// </summary>
    public static class EventDispatcher
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("EventDispatcher");

        static EventDispatcher()
        {
        }

        /// <summary>
        /// 通过模块工厂订阅某个模块的事件
        /// </summary>
        /// <typeparam name="TModule">要订阅的模块的类型</typeparam>
        /// <param name="factory">模块工厂</param>
        /// <param name="subscriber">订阅者</param>
        /// <param name="moduleId">要订阅的模块的Id</param>
        /// <param name="eventType">要订阅的事件类型</param>
        /// <param name="eventHandler">处理该事件的处理器</param>
        public static void SubscribeEvent<TModule>(this ModuleFactory factory, string moduleId, IEventSubscriber subscriber, int eventType, ModuleEventDlg eventHandler)
            where TModule : ModuleBase
        {
            TModule moduleInst = factory.LookupModule<TModule>(moduleId);
            if (moduleInst == null)
            {
                // 如果模块不存在，那么啥都不做直接返回
                logger.InfoFormat("订阅事件失败, 要订阅的模块不存在, 模块Id = {0}", moduleId);
                return;
            }

            List<Subscribtion> subscribtions;
            if (!moduleInst.EventSubscribtions.TryGetValue(eventType, out subscribtions))
            {
                // 该事件从没被订阅过
                subscribtions = new List<Subscribtion>();
            }

            if (subscribtions.Exists(v => v.Subscriber == subscriber))
            {
                logger.InfoFormat("事件{0}已经被{1}订阅过, 忽略本地订阅", eventType, moduleInst.Name);
                return;
            }

            logger.InfoFormat("模块{0}订阅事件:{1}", moduleInst.Name, eventType);
            Subscribtion subscribtion = new Subscribtion()
            {
                Subscriber = subscriber,
                EventHandler = eventHandler
            };
            subscribtions.Add(subscribtion);
        }

        public static void SubscribeEvent<TModule>(this ModuleFactory factory, IEventSubscriber subscriber, int eventType, ModuleEventDlg eventHandler)
            where TModule : ModuleBase
        {
            SubscribeEvent<TModule>(factory, string.Empty, subscriber, eventType, eventHandler);
        }

        /// <summary>
        /// 通过ModuleBase订阅一个模块的事件
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="subscriber"></param>
        /// <param name="moduleId"></param>
        /// <param name="eventType"></param>
        /// <param name="eventHandler"></param>
        public static void SubscribeEvent<TModule>(this ModuleBase subscriber, string moduleId, int eventType, ModuleEventDlg eventHandler)
            where TModule : ModuleBase
        {
            SubscribeEvent<TModule>(subscriber.Factory, moduleId, subscriber, eventType, eventHandler);
        }

        public static void SubscribeEvent<TModule>(this ModuleBase subscriber, int eventType, ModuleEventDlg eventHandler)
            where TModule : ModuleBase
        {
            SubscribeEvent<TModule>(subscriber, string.Empty, eventType, eventHandler);
        }

        /// <summary>
        /// 模块发布一个事件
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="eventType"></param>
        /// <param name="eventData"></param>
        public static void PublishEvent(this ModuleBase publisher, int eventType, object eventData)
        {
            List<Subscribtion> subscribtions;
            if (publisher.EventSubscribtions.TryGetValue(eventType, out subscribtions))
            {
                // 触发事件
                foreach (Subscribtion subscribtion in subscribtions)
                {
                    subscribtion.EventHandler(publisher, eventType, eventData);
                }
            }
        }
    }
}
