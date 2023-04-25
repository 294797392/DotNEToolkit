using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit.Utility
{
    public class PropertyAttribute<TAttribute>
    {
        /// <summary>
        /// 该特性所对应的属性信息
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// 该属性所拥有的特性实例
        /// </summary>
        public TAttribute Attribute { get; set; }
    }

    public class EnumAttribute<TAttribute>
    {
        public int Value { get; set; }

        public TAttribute Attribute { get; set; }
    }

    /// <summary>
    /// 反射工具函数
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// 获取某个类里的所有公开属性的某个特定类型的特性
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="classType">要获取的类的类型</param>
        /// <returns></returns>
        public static List<PropertyAttribute<TAttribute>> GetPropertyAttribute<TAttribute>(Type classType) where TAttribute : Attribute
        {
            List<PropertyAttribute<TAttribute>> result = new List<PropertyAttribute<TAttribute>>();

            PropertyInfo[] properties = classType.GetProperties(System.Reflection.BindingFlags.Public | BindingFlags.Instance);
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

        /// <summary>
        /// 获取某个类里的所有公开属性的某个特定类型的特性
        /// </summary>
        /// <typeparam name="TAttribute">要获取的特性的类型</typeparam>
        /// <typeparam name="TClass"></typeparam>
        /// <returns>所有属性的特性集合</returns>
        public static List<PropertyAttribute<TAttribute>> GetPropertyAttribute<TAttribute, TClass>() where TAttribute : Attribute
        {
            Type t = typeof(TClass);
            return GetPropertyAttribute<TAttribute>(t);
        }

        /// <summary>
        /// 获取某个类上的自定义特性
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TClass"></typeparam>
        /// <returns></returns>
        public static TAttribute GetClassAttribute<TAttribute, TClass>()
        {
            Type t = typeof(TClass);

            return GetClassAttribute<TAttribute>(t);
        }

        public static TAttribute GetClassAttribute<TAttribute>(Type classType)
        {
            object[] attributes = classType.GetCustomAttributes(typeof(TAttribute), true);
            if (attributes == null || attributes.Length == 0)
            {
                return default(TAttribute);
            }

            return (TAttribute)attributes[0];
        }

        //public static List<EnumAttribute<TAttribute>> GetEnumAttributes<TAttribute>(Type enumType) where TAttribute : Attribute
        //{
        //    List<EnumAttribute<TAttribute>> result = new List<EnumAttribute<TAttribute>>();

        //    FieldInfo field enumType.GetFields();
        //}

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
