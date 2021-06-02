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

        public ModuleStatus Status { get; set; }

        public string ID { get { return this.Definition.ID; } }

        public string Name { get { return this.Definition.Name; } }

        public string Description { get { return this.Definition.Description; } }

        public ModuleDefinition Definition { get; set; }

        public ModuleFactory Factory { get; set; }

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
            return DotNETCode.Success;
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
            if (this.InputParameters == null || this.InputParameters.Count == 0)
            {
                return;
            }

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

                bool isModule = property.PropertyType.IsSubclassOf(typeof(ModuleBase));

                // 该依赖属性的类型不是IModuleInstance
                if (!isModule)
                {
                    if (property.PropertyType.Name == "String")
                    {
                        property.SetValue(this, value, null);
                    }
                    else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericArguments()[0].IsSubclassOf(typeof(ModuleBase)))
                    {
                        // 如果是泛型，并且泛型里的参数类型是IModuleInstance

                        // 配置文件里的和实际类型不匹配
                        JArray jArray = value as JArray;
                        if (jArray == null)
                        {
                            continue;
                        }

                        // 获取泛型里元素的类型
                        Type elementType = property.PropertyType.GetGenericArguments()[0];

                        // 动态生成一个泛型
                        object v = Activator.CreateInstance(property.PropertyType);
                        MethodInfo addMethod = v.GetType().GetMethod("Add");
                        foreach (JToken token in jArray)
                        {
                            IModuleInstance minstance = this.Factory.LookupModule<IModuleInstance>(token.ToString());
                            if (minstance == null)
                            {
                                continue;
                            }

                            addMethod.Invoke(v, new object[] { minstance });
                        }

                        property.SetValue(this, v, null);
                    }
                    else
                    {
                        string json = value.ToString();
                        object v = JsonConvert.DeserializeObject(json, property.PropertyType);
                        property.SetValue(this, v, null);
                    }
                }
                else
                {
                    // 该依赖属性的类型是一个IModuleInstance，OK，反射赋值
                    // 存在对应的实例，那么反射赋值
                    string moduleID = value.ToString();
                    if (string.IsNullOrEmpty(moduleID))
                    {
                        continue;
                    }

                    IModuleInstance minstance = this.Factory.LookupModule<IModuleInstance>(moduleID);
                    if (minstance == null)
                    {
                        // 模块工厂里没有注册相应的模块
                        continue;
                    }

                    property.SetValue(this, minstance, null);
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

        #endregion
    }
}

