using DotNEToolkit.Bindings;
using DotNEToolkit.Extentions;
using DotNEToolkit.Modular.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit.Modular
{
    public abstract class ModuleBase : IModuleInstance
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleBase");

        #region 公开事件

        /// <summary>
        /// 当前模块有事件触发的时候触发
        /// 第一个参数：事件发送者
        /// 第二个参数：事件代码
        /// 第二个参数：eventXml（由用户定义）
        /// </summary>
        public event Action<IModuleInstance, string, object> PublishEvent;

        #endregion

        #region 属性

        /// <summary>
        /// 模块当前的状态
        /// </summary>
        public ModuleStatus Status { get; internal set; }

        /// <summary>
        /// 模块ID
        /// </summary>
        public string ID { get { return this.Definition.ID; } }

        /// <summary>
        /// 模块名字
        /// </summary>
        public string Name { get { return this.Definition.Name; } }

        /// <summary>
        /// 模块的描述信息
        /// </summary>
        public string Description { get { return this.Definition.Description; } }

        /// <summary>
        /// 模块的定义
        /// </summary>
        public ModuleDefinition Definition { get; internal set; }

        /// <summary>
        /// 模块所属工厂
        /// </summary>
        public ModuleFactory Factory { get; internal set; }

        /// <summary>
        /// 模块当前的输出参数
        /// </summary>
        public IDictionary Properties { get; private set; }

        /// <summary>
        /// 模块的输入参数
        /// </summary>
        public IDictionary InputParameters { get; private set; }

        #endregion

        #region 构造方法

        public ModuleBase()
        {
            this.Properties = new Dictionary<string, object>();
        }

        #endregion

        #region IModuleInstance

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="parameters">模块参数</param>
        /// <returns></returns>
        public virtual int Initialize(IDictionary parameters)
        {
            this.InputParameters = parameters;
            this.InitializeBinding();
            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 释放模块占用的资源
        /// </summary>
        /// <returns></returns>
        public virtual void Release()
        {
        }

        #endregion

        #region 实例方法

        private void InitializeBinding()
        {
            //DateTime start = DateTime.Now;
            //Console.WriteLine("开始反射");

            List<BindableProperty> properties = BindableProperties.Context.GetBindableProperties(this.GetType());
            foreach (BindableProperty property in properties)
            {
                object value = this.InputParameters.GetValue<object>(property.Name, property.Attribute.DefaultValue);
                if (value == null)
                {
                    // 没有配置对应的属性值
                    continue;
                }

                if (property.PropertyType.Name == "String")
                {
                    property.SetValue(this, value, null);
                }
                else
                {
                    string json = value.ToString();
                    object v = JsonConvert.DeserializeObject(json, property.PropertyType);
                    property.SetValue(this, v, null);
                }
            }

            //Console.WriteLine("反射完成, {0}", (DateTime.Now - start).TotalMilliseconds);
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 发布一个事件，该事件只有订阅了该事件的模块才能收到
        /// </summary>
        /// <param name="eventCode">事件代码</param>
        /// <param name="eventParams">事件参数</param>
        protected void PubEvent(string eventCode, object eventParams)
        {
            if (this.PublishEvent != null)
            {
                this.PublishEvent(this, eventCode, eventParams);
            }
        }

        protected void PubMessage(string message, params object[] param)
        {
            this.PubEvent(ModuleEvent.MessageEvent, string.Format(message, param));
        }

        #endregion
    }
}

