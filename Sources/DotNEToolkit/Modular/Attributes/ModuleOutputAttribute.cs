using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.Attributes
{
    /// <summary>
    /// 定义模块的输出数据
    /// 1. 工作流设计器使用
    /// 2. 方便开发者一眼就可以看到模块有哪些输出值
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ModuleOutputAttribute : Attribute
    {
        public string Key { get; set; }

        public Type ValueType { get; set; }

        public string Description { get; set; }

        public ModuleOutputAttribute(string key, Type valueType)
        {
            this.Key = key;
            this.ValueType = valueType;
        }

        public ModuleOutputAttribute(string description, string key, Type valueType)
        {
            this.Description = description;
            this.Key = key;
            this.ValueType = valueType;
        }
    }
}
