using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit.Bindings
{
    internal class BindableProperty
    {
        public Type PropertyType { get { return this.PropertyInfo.PropertyType; } }

        public string Name { get { return this.PropertyInfo.Name; } }

        public PropertyInfo PropertyInfo { get; set; }

        public BindablePropertyAttribute Attribute { get; set; }

        public void SetValue(object obj, object value, object[] index)
        {
            this.PropertyInfo.SetValue(obj, value, index);
        }
    }
}
