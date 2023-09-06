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
        /// ���ֵ��������ת�ɶ�Ӧ���͵�����
        /// ֧�ְ�ö���ַ�����ö��ֵת����ö������
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
                    // ö����Ҫ����ת��

                    if (v is string)
                    {
                        // ö�ٵ�ֵ���ַ���������ö�����ִ���
                        return (T)Enum.Parse(valueType, v.ToString());
                    }
                    else if (v is Int64)    // ���ֵĻ���JSON��������л���64λ��
                    {
                        // ö�ٵ�ֵ�����֣�����ö��ֵ����
                        string name = Enum.GetName(valueType, v);
                        return (T)Enum.Parse(valueType, name);
                    }
                    else if (v is Enum)
                    {
                        return (T)v;
                    }
                    else
                    {
                        // �Ȳ�������Ҳ�����ַ�������ô�ͷ���Ĭ��ֵ
                        logger.InfoFormat("ö�����Ͳ�����ֵ�Ȳ�������Ҳ�����ַ���������Ĭ��ֵ:{0}", defaultValue);
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
        /// ��������ڶ�Ӧ��Key����ôֱ�����쳣
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValue<T>(this IDictionary map, string key)
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
        /// <returns>����ƴ�ӿڵ��ַ��������ַ������û�зָ���</returns>
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
