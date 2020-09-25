using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Extentions
{
    public static class DictionaryExtentions
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
                object v = settings[key];
                if (v == null)
                {
                    return defaultValue;
                }
                else if (v is string)
                {
                    return (v as string).Parse<T>();
                }
                else
                {
                    return (T)v;
                }
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
