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
        /// ��������ڶ�Ӧ��Key����ôֱ�����쳣
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValueWithThrow<T>(this IDictionary map, string key)
        {
            if (!map.Contains(key))
            {
                throw new KeyNotFoundException(string.Format("δ�ҵ�key:{0}", key));
            }

            return map.GetValue<T>(key, default(T));
        }

        /// <summary>
        /// ���ַ��������е�����Ԫ�ط���һ���ַ�����
        /// </summary>
        /// <param name="list"></param>
        /// <param name="separator">ָ��Ҫʹ�õķָ���</param>
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
                // ȥ��β���ķָ���
                result = result.Substring(0, result.Length - separator.Length);
            }

            return result;
        }

        /// <summary>
        /// �ж�һ���������Ƿ����ظ�Ԫ��
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
