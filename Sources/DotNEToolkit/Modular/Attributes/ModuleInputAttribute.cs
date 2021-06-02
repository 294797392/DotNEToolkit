using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.Attributes
{
    /// <summary>
    /// 定义模块的输入参数
    /// 1. 工作流设计器使用
    /// 2. 方便开发者一眼就可以看到模块有哪些输出值
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class ModuleInputAttribute : Attribute
    {
        /// <summary>
        /// 参数的键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 该参数是否是可选参数
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// 表示该输入是否是一个类类型
        /// </summary>
        public bool IsClass { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="key"></param>
        /// <param name="valueType"></param>
        /// <param name="optional">true：可空，false：不可空</param>
        public ModuleInputAttribute(string description, string key, Type valueType, bool optional = false)
        {
            this.Description = description;
            this.Key = key;
            this.ValueType = valueType;
            this.Optional = optional;
            this.IsClass = false;
        }

        /// <summary>
        /// 当输入是一个类类型使用
        /// </summary>
        /// <param name="description"></param>
        /// <param name="optional"></param>
        public ModuleInputAttribute(string description, Type valueType, bool optional = false)
        {
            this.Description = description;
            this.Optional = optional;
            this.ValueType = valueType;
            this.IsClass = true;
        }

        /// <summary>
        /// 当输入是一个属性类型的时候使用
        /// </summary>
        /// <param name="description"></param>
        /// <param name="optional"></param>
        public ModuleInputAttribute(string description, bool optional = false)
        {
            this.Description = description;
            this.Optional = optional;
        }
    }
}
