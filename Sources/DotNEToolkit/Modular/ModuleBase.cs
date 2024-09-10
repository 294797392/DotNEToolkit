using DotNEToolkit.Bindings;
using DotNEToolkit.Extentions;
using DotNEToolkit.Modular.Attributes;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// 当有事件触发的时候触发
        /// int:EventTyep
        /// object:EventParam
        /// </summary>
        public event Action<ModuleBase, int, object> OnEvent;

        /// <summary>
        /// 模块消息，可以用来给用户看
        /// </summary>
        public event Action<ModuleBase, string> OnMessage;

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

        /// <summary>
        /// 通知外部模块有事件触发
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventParam"></param>
        protected void NotifyEvent(int eventType, object eventParam)
        {
            if (this.OnEvent != null)
            {
                this.OnEvent(this, eventType, eventParam);
            }
        }

        protected void NotifyMessage(string message, params object[] param)
        {
            string msg = string.Format(message, param);

            if (this.OnMessage != null)
            {
                this.OnMessage(this, msg);
            }
        }

        protected T GetParameter<T>(IDictionary dictionary, string key)
        {
            Type t = typeof(T);

            if (t == typeof(string))
            {
                return dictionary.GetValue<T>(key);
            }

            if (t.IsClass)
            {
                string json = dictionary[key].ToString();
                return JsonConvert.DeserializeObject<T>(json);
            }

            return dictionary.GetValue<T>(key);
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取该模块的输入参数，如果参数不存在则报异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetParameter<T>(string key)
        {
            IDictionary parameters = this.InputParameters;

            if (!parameters.Contains(key))
            {
                logger.ErrorFormat("没有找到必需的参数:{0}", key);
                throw new KeyNotFoundException();
            }

            return this.GetParameter<T>(parameters, key);
        }

        /// <summary>
        /// 读取该模块的输入参数，如果参数不存在则返回defaultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetParameter<T>(string key, T defaultValue)
        {
            IDictionary parameters = this.InputParameters;

            if (!parameters.Contains(key))
            {
                return defaultValue;
            }

            return this.GetParameter<T>(parameters, key);
        }

        /// <summary>
        /// 设置参数
        /// 注意如果设置的参数是类类型，那么会先把类序列化成字符串
        /// 所以不能序列化带有状态的类，只能序列化一些数据模型类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetParameter<T>(string key, T value)
        {
            IDictionary parameters = this.InputParameters;

            Type t = typeof(T);

            if (t == typeof(string))
            {
                parameters[key] = value.ToString();
                return;
            }

            if (t.IsClass)
            {
                string json = JsonConvert.SerializeObject(value);
                parameters[key] = json;
                return;
            }

            parameters[key] = value;

            return;

            throw new NotImplementedException();
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

