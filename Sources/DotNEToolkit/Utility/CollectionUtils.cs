using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    public static class CollectionUtils
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Collections");

        public static T Parse<T>(this string value)
        {
            Type conversionType = typeof(T);

            if (conversionType.IsEnum)
            {
                return (T)Enum.Parse(conversionType, value);
            }
            else if (conversionType == typeof(TimeSpan))
            {
                object v = TimeSpan.Parse(value);
                return (T)v;
            }
            else if (conversionType.IsGenericType
                 && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return default(T);
                }

                NullableConverter nullableConverter = new NullableConverter(conversionType);

                conversionType = nullableConverter.UnderlyingType;
            }

            return (T)Convert.ChangeType(value, conversionType);
        }

        /// <summary>
        /// 把字典里的数据转成对应类型的数据
        /// 支持把枚举字符串和枚举值转换成枚举类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetValue<T>(this IDictionary settings, object key, T defaultValue)
        {
            Type valueType = typeof(T);

            try
            {
                if (!settings.Contains(key))
                {
                    return defaultValue;
                }

                object v = settings[key];
                if (v == null)
                {
                    return defaultValue;
                }
                else if (!valueType.IsValueType)
                {
                    return (T)v;
                }
                else if (valueType.IsEnum)
                {
                    // 枚举需要单独转换

                    if (v is string)
                    {
                        // 枚举的值是字符串，当成枚举名字处理
                        return (T)Enum.Parse(valueType, v.ToString());
                    }
                    else if (v is Int64)    // 数字的话，JSON库把它序列化成64位数
                    {
                        // 枚举的值是数字，当成枚举值处理
                        string name = Enum.GetName(valueType, v);
                        return (T)Enum.Parse(valueType, name);
                    }
                    else if (v is Enum)
                    {
                        return (T)v;
                    }
                    else
                    {
                        // 既不是数字也不是字符串，那么就返回默认值
                        logger.InfoFormat("枚举类型参数的值既不是数字也不是字符串，返回默认值:{0}", defaultValue);
                        return defaultValue;
                    }
                }
                else
                {
                    return (T)Convert.ChangeType(v, typeof(T));
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 如果不存在对应的Key，那么直接抛异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValue<T>(this IDictionary map, string key)
        {
            if (!map.Contains(key))
            {
                throw new KeyNotFoundException(string.Format("未找到key:{0}", key));
            }

            return map.GetValue<T>(key, default(T));
        }

        /// <summary>
        /// 把字符串数组中的所有元素放入一个字符串。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="separator">指定要使用的分隔符</param>
        /// <returns>返回拼接口的字符串，该字符串最后没有分隔符</returns>
        public static string Join(this IEnumerable<string> list, string separator = null)
        {
            bool useSeparator = separator == null || separator.Length == 0 ? false : true;

            string result = string.Empty;

            foreach (string value in list)
            {
                result += value;

                if (useSeparator)
                {
                    result += separator;
                }
            }

            if (useSeparator && list.Count() > 0)
            {
                // 去掉尾部的分隔符
                result = result.Substring(0, result.Length - separator.Length);
            }

            return result;
        }

        /// <summary>
        /// 判断一个集合里是否有重复元素
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasDuplicated(this List<string> source)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i != j)
                    {
                        if (source[i] == source[j])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
