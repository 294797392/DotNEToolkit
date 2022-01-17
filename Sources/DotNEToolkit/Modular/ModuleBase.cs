using DotNEToolkit.Bindings;
using DotNEToolkit.Extentions;
using DotNEToolkit.Modular.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 表示一个抽象的模块
    /// </summary>
    public abstract class ModuleBase : IModuleInstance
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleBase");

        #endregion

        #region 公开事件

        /// <summary>
        /// 当前模块有事件触发的时候触发
        /// 第一个参数：事件发送者
        /// 第二个参数：事件代码
        /// 第二个参数：eventXml（由用户定义）
        /// </summary>
        public event Action<IModuleInstance, int, object> PublishEvent;

        #endregion

        #region 实例变量

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
        /// 在ModuleFactory里被赋值
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
                else if (property.PropertyType.IsEnum)
                {
                    object o = Enum.Parse(property.PropertyType, value.ToString());
                    property.SetValue(this, o, null);
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
        /// 从InputParameter里读取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected T GetInputValue<T>(string key)
        {
            string json = this.InputParameters[key].ToString();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 从InputParameter里读取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue">如果不存在该对象，那么要返回的默认值</param>
        /// <returns></returns>
        protected T GetInputValue<T>(string key, T defaultValue)
        {
            if (!this.InputParameters.Contains(key))
            {
                return defaultValue;
            }

            return this.GetInputValue<T>(key);
        }

        /// <summary>
        /// 发布一个事件，该事件只有订阅了该事件的模块才能收到
        /// </summary>
        /// <param name="eventCode">事件代码</param>
        /// <param name="eventParams">事件参数</param>
        protected void PubEvent(int eventType, object eventData)
        {
            if (this.PublishEvent != null)
            {
                this.PublishEvent(this, eventType, eventData);
            }
        }

        protected void PubMessage(string message, params object[] param)
        {
            this.PubEvent(ModuleEvent.MessageEvent, string.Format(message, param));
        }

        #endregion
    }
}

