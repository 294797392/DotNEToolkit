using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit.Bindings
{
    internal class BindableProperties : SingletonObject<BindableProperties>
    {
        public BindableProperties()
        {
        }

        internal List<BindableProperty> GetBindableProperties(Type t)
        {
            string typeName = t.FullName;

            List<BindableProperty> propertyList = new List<BindableProperty>();

            // 反射获取所有属性
            List<PropertyInfo> properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            // 把特性为attrType的属性加到集合里
            foreach (PropertyInfo prop in properties)
            {
                object[] attributes = prop.GetCustomAttributes(typeof(BindablePropertyAttribute), true);
                if (attributes == null || attributes.Length == 0)
                {
                    continue;
                }

                propertyList.Add(new BindableProperty() 
                {
                    PropertyInfo = prop,
                    Attribute = attributes[0] as BindablePropertyAttribute
                });
            }

            return propertyList;
        }
    }
}
