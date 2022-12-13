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

        /// <summary>
        /// 日志记录器
        /// </summary>
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleBase");

        #endregion

        #region 公开事件

        #endregion

        #region 实例变量

        #endregion

        #region 属性

        /// <summary>
        /// 该模块引用的其他模块ID
        /// </summary>
        //internal List<string> References { get { return this.Definition.References; } }

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
        internal IDictionary InputParameters { get; set; }

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// </summary>
        public ModuleBase()
        {
            this.Properties = new Dictionary<string, object>();
            this.InputParameters = new Dictionary<string, object>();
        }

        #endregion

        #region IModuleInstance

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <returns></returns>
        public int Initialize()
        {
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

        public T GetInputValue<T>(string key)
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
        public T GetInputValue<T>(string key, T defaultValue)
        {
            return this.InputParameters.GetValue<T>(key, defaultValue);
        }

        public T GetInputObject<T>(string key) where T : class
        {
            string json = this.InputParameters[key].ToString();
            return JSONHelper.Parse<T>(json);
        }

        /// <summary>
        /// 从InputParameter里读取一个JSON对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultObject">如果不存在该对象，那么要返回的默认值</param>
        /// <returns></returns>
        public T GetInputObject<T>(string key, T defaultObject) where T : class
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

        public void SetInputValue<T>(string key, T value)
        {
            this.InputParameters[key] = value;
        }

        public void SetInputObject<T>(string key, T objact) where T : class
        {
            string json = JsonConvert.SerializeObject(objact);
            this.InputParameters[key] = json;
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

