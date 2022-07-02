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
        /// 不要使用这个参数去获取配置参数，而是使用Initialize方法里穿进去的parameter去获取配置参数
        /// 因为InputParameters参数里有可能有绑定参数，需要动态计算
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
        public int Initialize(IDictionary parameters)
        {
            this.InputParameters = parameters;
            this.InitializeBinding();

            return this.OnInitialize();
        }

        /// <summary>
        /// 释放模块占用的资源
        /// </summary>
        /// <returns></returns>
        public void Release()
        {
            this.OnRelease();
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

        protected T GetInputValue<T>(string key)
        {
            return this.InputParameters.GetValue<T>(key);
        }

        /// <summary>
        /// 从InputParameter里读取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetInputValue<T>(string key, T defaultValue)
        {
            return this.InputParameters.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// 从InputParameter里读取一个JSON对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultObject">如果不存在该对象，那么要返回的默认值</param>
        /// <returns></returns>
        protected T GetInputObject<T>(string key, T defaultObject)
        {
            if (!this.InputParameters.Contains(key))
            {
                return defaultObject;
            }

            string json = this.InputParameters[key].ToString();
            if (string.IsNullOrEmpty(json))
            {
                return defaultObject;
            }

            return JSONHelper.Parse<T>(json, defaultObject);
        }

        /// <summary>
        /// 发布一个事件，该事件只有订阅了该事件的模块才能收到
        /// </summary>
        /// <param name="eventType">事件代码</param>
        /// <param name="eventData">事件参数</param>
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

        /// <summary>
        /// 子类初始化
        /// </summary>
        /// <returns></returns>
        protected abstract int OnInitialize();

        /// <summary>
        /// 子类释放资源
        /// </summary>
        protected abstract void OnRelease();
    }
}

