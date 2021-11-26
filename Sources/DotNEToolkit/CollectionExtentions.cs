using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Extentions
{
    public static class CollectionExtentions
    {
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

        public static T GetValue<T>(this IDictionary settings, object key, T defaultValue)
        {
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
                else if (!typeof(T).IsValueType)
                {
                    return (T)v;
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
        public static T GetValueWithThrow<T>(this IDictionary map, string key)
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
        /// <returns></returns>
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

            if (useSeparator && list.Count() > 1)
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
