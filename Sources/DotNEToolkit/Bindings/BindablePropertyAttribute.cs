using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Bindings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BindablePropertyAttribute : Attribute
    {
        /// <summary>
        /// 成员的默认值
        /// </summary>
        public object DefaultValue { get; set; }

        public BindablePropertyAttribute()
        { }

        public BindablePropertyAttribute(object defaultValue)
        {
            this.DefaultValue = defaultValue;
        }
    }
}