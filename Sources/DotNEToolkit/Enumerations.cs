using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit.Extentions
{
    /// <summary>
    /// 描述一个枚举成员
    /// </summary>
    public class EnumMember
    {
        /// <summary>
        /// 枚举的名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 枚举的值
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 枚举的描述信息
        /// 使用Attribute指定
        /// </summary>
        public string Description { get; set; }
    }

    public static class Enumerations
    {
        public static int UnsetFlag(this int source, int toUnset)
        {
            return source &= ~toUnset;
        }

        /// <summary>
        /// 把一个枚举类型转换成实体集合
        /// </summary>
        /// <typeparam name="T">要转换的枚举类型</typeparam>
        /// <returns></returns>
        public static List<EnumMember> GetMemberList<T>()
        {
            Type enumType = typeof(T);
            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

            List<EnumMember> result = new List<EnumMember>();

            foreach (FieldInfo field in fields)
            {
                object value = Enum.Parse(enumType, field.Name);

                result.Add(new EnumMember()
                {
                    Value = (int)value,
                    Name = field.Name,
                    Description = ParseDescription(field)
                });
            }

            return result;
        }

        /// <summary>
        /// 解析枚举字段上的DescriptionAttribute特性
        /// </summary>
        /// <param name="enumField"></param>
        /// <returns></returns>
        private static string ParseDescription(FieldInfo enumField)
        {
            DescriptionAttribute descriptionAttribute = enumField.GetCustomAttributes(typeof(DescriptionAttribute), false).OfType<DescriptionAttribute>().FirstOrDefault();
            if (descriptionAttribute == null)
            {
                return string.Empty;
            }
            else
            {
                return descriptionAttribute.Description;
            }
        }
    }
}

