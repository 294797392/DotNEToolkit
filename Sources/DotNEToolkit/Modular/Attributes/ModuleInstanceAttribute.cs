using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.Attributes
{
    /// <summary>
    /// 表示一个模块的公开属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ModulePropertyAttribute : Attribute
    {

    }

    /// <summary>
    /// 表示一个模块的调用操作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleActionAttribute : Attribute
    {
        /// <summary>
        /// 动作的名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否在界面上手动触发
        /// </summary>
        public bool Manually { get; set; }

        public ModuleActionAttribute()
        {
            this.Manually = false;
        }
    }

    /// <summary>
    /// 工作流设计器使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleInstanceAttribute : Attribute
    {
        public string ID { get; set; }

        public ModuleInstanceAttribute(string id)
        {
            this.ID = id;
        }
    }
}
