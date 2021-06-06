using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 保存线程上下文信息
    /// </summary>
    internal class ThreadContext
    {
        /// <summary>
        /// 线程ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 线程所对应的Task
        /// </summary>
        public Task Task { get; set; }
    }

    /// <summary>
    /// 并行操作的封装
    /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// 默认开启5线程运行任务
        /// </summary>
        private const int DefaultThreadNumber = 5;

        private static log4net.ILog logger = log4net.LogManager.GetLogger("Parallel");

        public static void Foreach<T>(IList<T> source, Action<T, object> action, object userData)
        {
            Foreach<T>(source, DefaultThreadNumber, action, userData);
        }

        public static void Foreach<T>(IList<T> source, int threadNum, Action<T, object> action, object userData)
        {
            Foreach<T>(source, threadNum, action, userData, null);
        }

        /// <summary>
        /// 对source集合进行并行操作
        /// 该方法是异步的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="threadNum">要开启的线程数量</param>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <param name="userData"></param>
        /// <param name="callback">当所有action都运行完了之后的callback</param>
        public static void Foreach<T>(IList<T> source, int threadNum, Action<T, object> action, object userData, Action<object> callback)
        {
            // item总数
            int length = source.Count();

            // 下一个要执行操作的元素的索引
            int toProcess = -1;

            object indexLock = new object();

            // 运行最后一个任务的线程
            ThreadContext lastContext = null;

            // 保存运行的线程上下文信息
            List<ThreadContext> threadContexts = new List<ThreadContext>();

            for (int i = 0; i < threadNum; i++)
            {
                ThreadContext context = new ThreadContext();
                context.ID = i;
                context.Task = new Task((v) =>
                {
                    ThreadContext innerContext = v as ThreadContext;

                    while (true)
                    {
                        lock (indexLock)
                        {
                            toProcess++;

                            if (toProcess == length - 1)
                            {
                                // 运行最后一个任务的线程
                                lastContext = innerContext;
                            }
                        }

                        if (toProcess >= length)
                        {
                            // 所有的数据都处理完了
                            if (lastContext == innerContext)
                            {
                                // 运行到这里说明，lastContext已经运行完了，但是又抢到了一个资源，那么此时表示所有的任务全部都运行完了
                                // 直接回调
                                if (callback != null)
                                {
                                    callback(userData);
                                }
                            }
                            break;
                        }

                        T value = source[toProcess];

                        try
                        {
                            logger.DebugFormat("线程{0}开始处理{1}", innerContext.ID, value);
                            action(value, userData);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("action执行异常", ex);
                        }
                    }

                }, context);

                context.Task.Start();
            }
        }
    }
}
