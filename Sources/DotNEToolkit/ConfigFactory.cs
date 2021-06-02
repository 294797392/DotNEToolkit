using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 根据配置文件生成相应的实例
    /// </summary>
    public class ConfigFactory<T>
    {
        private static ILog logger = LogManager.GetLogger(typeof(ConfigFactory<T>));

        /// <summary>
        /// Factory 读取AppSetting配置所用的key
        /// </summary>
        public static string ConfigKey
        {
            get;
            set;
        }

        /// <summary>
        /// 创建一个接口对象T的实例
        /// </summary>
        /// <typeparam name="T">需要实现的接口</typeparam>
        /// <returns></returns>
        /// <remarks>
        /// 该函数将自动读取配置文件，并根据配置文件里AppSetting中ConfigKey所对应的值自动选择合适的实现。
        /// ConfigKey所对应的键值为所需接口T的完整类名。如果需要指定assembly名, 则在类名后用逗号隔开
        /// 例如："ICare.EventSender.DirectSender, EventSender"
        /// </remarks>
        public static T CreateInstance()
        {
            string config = ConfigurationManager.AppSettings[ConfigKey];

            if (string.IsNullOrEmpty(config))
            {
                throw new ArgumentException("No configuration for interface!");
            }

            string[] implementation = config.Split(',');
            if (implementation != null && implementation.Length > 0)
            {
                return implementation.Length > 1 ?
                    CreateInstance(implementation[1], implementation[0]) :
                    CreateInstance(implementation[0]);
            }
            else
            {
                throw new ArgumentException("Invalid configuration for EventSender!");
            }
        }


        /// <summary>
        /// 根据类名和Assebmly名创建IEventDispatcher
        /// </summary>
        /// <param name="className"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static T CreateInstance(string className, string assemblyName)
        {
            try
            {
                logger.Debug("CreateInstance: " + className + ", " + assemblyName);

                Assembly assembly = Assembly.Load(assemblyName);
                Type type = assembly.GetType(className, true);
                T instance = (T)Activator.CreateInstance(type);

                return instance;
            }
            catch (Exception ex)
            {
                logger.Error("Cannot load class", ex);
                throw new ArgumentException("Invalid configuration for EventSender!", ex);
            }
        }

        /// <summary>
        /// 根据指定类名创建IEventDispatcher
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static T CreateInstance(string className)
        {
            try
            {
                logger.Debug("Create instance: " + className);
                Type type = Type.GetType(className, true);
                T instance = (T)Activator.CreateInstance(type);
                return instance;
            }
            catch (Exception ex)
            {
                logger.Error("Cannot load instance", ex);
                throw new ArgumentException("创建实例出错：" + ex.Message, ex);
            }
        }
    }
}
