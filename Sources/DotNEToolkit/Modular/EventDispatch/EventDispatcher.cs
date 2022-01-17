using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.EventDispatch
{
    /// <summary>
    /// 处理事件的委托
    /// </summary>
    /// <param name="pubModuleID">发布事件的模块ID</param>
    /// <param name="eventID">发布的事件ID</param>
    /// <param name="eventData">发布的事件数据</param>
    public delegate void ModuleEventHandler(string pubModuleID, int eventID, object eventData);

    /// <summary>
    /// 实现各模块之间的事件的发布-订阅模式
    /// 目的是可以解耦各个模块之间的事件订阅
    /// </summary>
    public class EventDispatcher
    {
        /// <summary>
        /// 存储订阅的事件信息
        /// ModuleID+EventID -> SubscribedEvent
        /// </summary>
        private Dictionary<string, List<Subscriber>> subscriberMap;

        public EventDispatcher()
        {
            this.subscriberMap = new Dictionary<string, List<Subscriber>>();
        }

        /// <summary>
        /// 发布一个事件
        /// </summary>
        /// <param name="publisherID">发布事件的模块ID</param>
        /// <param name="eventID">发布的事件ID</param>
        /// <param name="eventData">发布的事件数据</param>
        public void PublishEvent(string publisherID, int eventID, object eventData)
        {
            string subID = this.GenerateSubscriptionID(publisherID, eventID);

            List<Subscriber> subscribers;
            if (!this.subscriberMap.TryGetValue(subID, out subscribers))
            {
                // 这个模块没有被人订阅
                return;
            }

            foreach (Subscriber subscriber in subscribers)
            {
                subscriber.EventHandler(publisherID, eventID, eventData);
            }
        }

        /// <summary>
        /// 订阅某个模块的事件
        /// </summary>
        /// <param name="subscriberID">订阅者ID</param>
        /// <param name="subModuleID">被订阅的模块ID</param>
        /// <param name="eventID">被订阅的事件的ID</param>
        public void SubscribeEvent(string subscriberID, string subModuleID, int eventID, ModuleEventHandler handler)
        {
            string subID = this.GenerateSubscriptionID(subModuleID, eventID);

            List<Subscriber> subscribers;
            if (!this.subscriberMap.TryGetValue(subID, out subscribers))
            {
                subscribers = new List<Subscriber>();
                this.subscriberMap[subID] = subscribers;
            }

            subscribers.Add(new Subscriber()
            {
                EventHandler = handler,
                ID = subscriberID
            });
        }

        private string GenerateSubscriptionID(string moduleID, int eventID)
        {
            return string.Format("{0} - {1}", moduleID, eventID);
        }
    }
}
