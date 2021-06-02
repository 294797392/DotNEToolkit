using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Modular.Attributes
{
    /// <summary>
    /// 定义Task的参数类型
    /// 工作流设计器使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleParameterAttribute : Attribute
    {
        public Type Type { get; set; }

        public ModuleParameterAttribute(Type t)
        {
            this.Type = t;
        }
    }
}