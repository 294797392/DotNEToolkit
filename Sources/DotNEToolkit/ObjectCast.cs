using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;

namespace DotNEToolkit
{
    /// <summary>
    /// 属性转换类，将一个类的属性值转换给另外一个类的同名属性，注意该类使用的是浅表复制。
    /// <example>
    ///        下面几种用法一样:
    ///        ModuleCast.GetCast(typeof(CarInfo), typeof(ImplCarInfo)).Cast(info, ic);
    ///        ModuleCast.CastObject《CarInfo, ImplCarInfo》(info, ic);
    ///        ModuleCast.CastObject(info, ic);
    ///
    ///        ImplCarInfo icResult= info.CopyTo《ImplCarInfo》(null);
    ///
    ///        ImplCarInfo icResult2 = new ImplCarInfo();
    ///        info.CopyTo《ImplCarInfo》(icResult2);
    /// 
    /// </example>
    /// </summary>
    public class ObjectCast
    {
        private List<CastProperty> mProperties = new List<CastProperty>();

        static Dictionary<Type, Dictionary<Type, ObjectCast>> mCasters = new Dictionary<Type, Dictionary<Type, ObjectCast>>(256);

        private static Dictionary<Type, ObjectCast> GetModuleCast(Type sourceType)
        {
            Dictionary<Type, ObjectCast> result;
            lock (mCasters)
            {
                if (!mCasters.TryGetValue(sourceType, out result))
                {
                    result = new Dictionary<Type, ObjectCast>(8);
                    mCasters.Add(sourceType, result);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取要转换的当前转换类实例
        /// </summary>
        /// <param name="sourceType">要转换的源类型</param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public static ObjectCast GetCast(Type sourceType, Type targetType)
        {
            Dictionary<Type, ObjectCast> casts = GetModuleCast(sourceType);
            ObjectCast result;
            lock (casts)
            {
                if (!casts.TryGetValue(targetType, out result))
                {
                    result = new ObjectCast(sourceType, targetType);
                    casts.Add(targetType, result);
                }
            }
            return result;
        }

        /// <summary>
        /// 以两个要转换的类型作为构造函数，构造一个对应的转换类
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        public ObjectCast(Type sourceType, Type targetType)
        {
            PropertyInfo[] targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo sp in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (PropertyInfo tp in targetProperties)
                {
                    if (sp.Name == tp.Name && sp.PropertyType == tp.PropertyType)
                    {
                        CastProperty cp = new CastProperty();
                        cp.SourceProperty = new PropertyAccessorHandler(sp);
                        cp.TargetProperty = new PropertyAccessorHandler(tp);
                        mProperties.Add(cp);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 将源类型的属性值转换给目标类型同名的属性
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void Cast(object source, object target)
        {
            Cast(source, target, null);
        }

        /// <summary>
        /// 将源类型的属性值转换给目标类型同名的属性，排除要过滤的属性名称
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void Cast(object source, object target, string[] filter)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            for (int i = 0; i < mProperties.Count; i++)
            {
                CastProperty cp = mProperties[i];
                if (cp.SourceProperty.Getter != null)
                {
                    object Value = cp.SourceProperty.Getter(source, null); //PropertyInfo.GetValue(source,null);
                    if (cp.TargetProperty.Setter != null)
                    {
                        if (filter == null)
                        {
                            cp.TargetProperty.Setter(target, Value, null);
                        }
                        else if (!filter.Contains(cp.TargetProperty.PropertyName))
                        {
                            cp.TargetProperty.Setter(target, Value, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 转换对象
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        public static void CastObject<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            ObjectCast.GetCast(typeof(TSource), typeof(TTarget)).Cast(source, target);
        }


        /// <summary>
        /// 转换属性对象
        /// </summary>
        public class CastProperty
        {
            public PropertyAccessorHandler SourceProperty
            {
                get;
                set;
            }

            public PropertyAccessorHandler TargetProperty
            {
                get;
                set;
            }
        }

        /// <summary>
        /// 属性访问器
        /// </summary>
        public class PropertyAccessorHandler
        {
            public PropertyAccessorHandler(PropertyInfo propInfo)
            {
                this.PropertyName = propInfo.Name;
                //var obj = Activator.CreateInstance(classType);
                //var getterType = typeof(FastPropertyAccessor.GetPropertyValue<>).MakeGenericType(propInfo.PropertyType);
                //var setterType = typeof(FastPropertyAccessor.SetPropertyValue<>).MakeGenericType(propInfo.PropertyType);

                //this.Getter = Delegate.CreateDelegate(getterType, null, propInfo.GetGetMethod());
                //this.Setter = Delegate.CreateDelegate(setterType, null, propInfo.GetSetMethod());
                if (propInfo.CanRead)
                    this.Getter = propInfo.GetValue;

                if (propInfo.CanWrite)
                    this.Setter = propInfo.SetValue;
            }
            public string PropertyName { get; set; }
            public Func<object, object[], object> Getter { get; private set; }
            public Action<object, object, object[]> Setter { get; private set; }
        }

        private static ConcurrentDictionary<string, Delegate> myDelegateCache = new ConcurrentDictionary<string, Delegate>();
    }

    ///// <summary>
    ///// 快速属性访问器，注意，这将于具体的对象相绑定，如果某对象用得不多，请使用 NoCache 相关的方法
    ///// </summary>
    //public class FastPropertyAccessor
    //{
    //    public delegate T GetPropertyValue<T>();
    //    public delegate void SetPropertyValue<T>(T Value);

    //    private static ConcurrentDictionary<string, Delegate> myDelegateCache = new ConcurrentDictionary<string, Delegate>();

    //    public static GetPropertyValue<T> CreateGetPropertyValueDelegate<TSource, T>(TSource obj, string propertyName)
    //    {
    //        string key = string.Format("DGP-{0}-{1}", typeof(TSource).FullName, propertyName);//Delegate-GetProperty-{0}-{1}
    //        GetPropertyValue<T> result = (GetPropertyValue<T>)myDelegateCache.GetOrAdd(
    //            key,
    //            newkey =>
    //            {
    //                return Delegate.CreateDelegate(typeof(GetPropertyValue<T>), obj, typeof(TSource).GetProperty(propertyName).GetGetMethod());
    //            }
    //            );

    //        return result;
    //    }
    //    public static SetPropertyValue<T> CreateSetPropertyValueDelegate<TSource, T>(TSource obj, string propertyName)
    //    {
    //        string key = string.Format("DSP-{0}-{1}", typeof(TSource).FullName, propertyName);//Delegate-SetProperty-{0}-{1}
    //        SetPropertyValue<T> result = (SetPropertyValue<T>)myDelegateCache.GetOrAdd(
    //           key,
    //           newkey =>
    //           {
    //               return Delegate.CreateDelegate(typeof(SetPropertyValue<T>), obj, typeof(TSource).GetProperty(propertyName).GetSetMethod());
    //           }
    //           );

    //        return result;
    //    }

    //    public static GetPropertyValue<T> CreateGetPropertyValueDelegateNoCache<TSource, T>(TSource obj, string propertyName)
    //    {
    //        return (GetPropertyValue<T>)Delegate.CreateDelegate(typeof(GetPropertyValue<T>), obj, typeof(TSource).GetProperty(propertyName).GetGetMethod()); ;
    //    }
    //    public static SetPropertyValue<T> CreateSetPropertyValueDelegateNoCache<TSource, T>(TSource obj, string propertyName)
    //    {
    //        return (SetPropertyValue<T>)Delegate.CreateDelegate(typeof(SetPropertyValue<T>), obj, typeof(TSource).GetProperty(propertyName).GetSetMethod()); ;
    //    }
    //}

    /// <summary>
    /// 对象转换扩展
    /// </summary>
    public static class ObjectCastExtension
    {
        /// <summary>
        /// 将当前对象的属性值复制到目标对象，使用浅表复制
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象，如果为空，将生成一个</param>
        /// <returns>复制过后的目标对象</returns>
        public static T CopyTo<T>(this object source, T target = null, string[] filter = null) where T : class,new()
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                target = new T();
            ObjectCast.GetCast(source.GetType(), typeof(T)).Cast(source, target, filter);
            return target;
        }
    }

}
