using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 表示一个单例对象
    /// </summary>
    /// <typeparam name="T">单例类的类型</typeparam>
    public class SingletonObject<T> where T : class
    {
        private static object contextLock = new object();

        private static T context;

        public static T Context
        {
            get
            {
                if (context == null)
                {
                    lock (contextLock)
                    {
                        if (context == null)
                        {
                            context = Activator.CreateInstance<T>();
                        }
                    }
                }
                return context;
            }
        }

        protected SingletonObject()
        { }
    }
}
