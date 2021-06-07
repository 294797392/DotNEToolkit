using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit
{
    public class PropertyAttribute<TAttribute>
    {
        public PropertyInfo Property { get; set; }

        public TAttribute Attribute { get; set; }
    }

    /// <summary>
    /// 反射工具函数
    /// </summary>
    public static class Reflections
    {
        public static List<PropertyAttribute<TAttribute>> GetPropertyAttribute<TAttribute>(Type t) where TAttribute : Attribute
        {
            List<PropertyAttribute<TAttribute>> result = new List<PropertyAttribute<TAttribute>>();

            PropertyInfo[] properties = t.GetProperties(System.Reflection.BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(typeof(TAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    result.Add(new PropertyAttribute<TAttribute>() 
                    {
                        Property = property,
                        Attribute = attributes.Cast<TAttribute>().ElementAt(0)
                    });
                }
            }

            return result;
        }

        ///// <summary>
        ///// 获取集合里的元素的类型
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="fieldType">要获取的字段类型</param>
        ///// <returns></returns>
        //public static Type GetElementType(Type fieldType)
        //{
        //    if (fieldType.IsGenericType)
        //    {
        //        // 是List
        //        return fieldType.GetGenericArguments()[0];
        //    }
        //    else if (fieldType.IsArray)
        //    {
        //        // 是数组类型, 数组类型先转List

            //        // 不能在原有实例上修改，要先创建一个全新的实例
            //        object copy = Activator.CreateInstance(fieldType);

            //        // 调用ToList，变成一个集合
            //        copy.GetType().GetMethod("ToList").Invoke(copy, null);

            //        return copy.GetType().GetGenericArguments()[0];
            //    }
            //    else
            //    {
            //        // 其他的暂时不支持
            //        throw new NotImplementedException();
            //    }
            //}
    }
}
