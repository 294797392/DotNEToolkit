using System;
using System.Collections.Generic;
using System.Threading;

namespace DotNEToolkit
{
    /// <summary>
    /// Queue
    /// 用于生产者消费者的缓冲池。和传统模式不同的是，当待处理队列已满时，策略是丢弃某个对象而不是让生产者等待
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BufferQueue<T> : IDisposable
    {
        #region 私有变量

        private const int DefaultPoolSize = 256;

        private object syncRoot = new object();

        private Semaphore poolSemaphore; // 利用信号量机制处理资源池

        private AbandonStrategy abandonStrategy = AbandonStrategy.Oldest;

        private Queue<T> queueBase;

        private int maxPoolSize = 0;

        #endregion

        #region 构造函数

        public BufferQueue(int maxPoolSize = DefaultPoolSize)
        {
            this.maxPoolSize = maxPoolSize;

            this.queueBase = new Queue<T>(maxPoolSize);

            this.poolSemaphore = new Semaphore(0, maxPoolSize);
        }

        ~BufferQueue()
        {
            this.Dispose(true);
        }

        #endregion

        #region 公开属性

        public AbandonStrategy Strategy
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.abandonStrategy;
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.abandonStrategy = value;
                }
            }
        }

        public object SyncRoot
        {
            get { return this.syncRoot; }
        }

        /// <summary>
        /// 缓冲池最大允许长度，0 表示没有最大长度
        /// </summary>
        public int MaxPoolSize
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.maxPoolSize;
                }
            }
        }

        /// <summary>
        /// 获取队列里元素的数量
        /// </summary>
        public int Size
        {
            get
            {
                return this.queueBase.Count;
            }
        }

        /// <summary>
        /// 将缓冲对象进入缓冲池
        /// </summary>
        /// <param name="t"></param>
        /// <returns>0 没有任何遗弃，1 缓冲池已满，遗弃一个对象</returns>
        public int Enqueue(T t)
        {
            lock (syncRoot)
            {
                if (this.MaxPoolSize > this.queueBase.Count)
                {
                    this.queueBase.Enqueue(t);

                    try
                    {
                        this.poolSemaphore.Release();  // 信号量+1（通知等候队列）
                    }
                    catch
                    {
                    }

                    return 0;
                }
                else
                {
                    // 缓冲池已到达最大值，抛弃一帧

                    if (this.abandonStrategy == AbandonStrategy.Oldest)
                    {
                        this.queueBase.Dequeue();
                        this.queueBase.Enqueue(t);
                    }

                    return 1;
                }
            }
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 取出一个缓冲对象.如果池中已空，将会挂起线程等待
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            this.poolSemaphore.WaitOne();

            lock (syncRoot)
            {
                return this.queueBase.Dequeue();
            }
        }

        public void Clear()
        {
            lock (this.syncRoot)
            {
                // 当queue.Count == 0时, poolSemaphore剩余信号也应该为0，因此不需要任何处理
                // 当queue.Count > 0时, poolSemaphore剩余信号也大于0, 因此应该没有线程在WaitOne中等待
                // 直接将信号量恢复即可
                if (this.queueBase.Count > 0)
                {
                    for (int i = 0; i < this.queueBase.Count; i++)
                    {
                        this.poolSemaphore.WaitOne(0);
                    }

                    this.queueBase.Clear();
                }
            }
        }

        #endregion

        #region inner class

        public enum AbandonStrategy
        {
            Latest, // 当缓冲池满后抛弃最新的
            Oldest  // 当缓冲池满后抛弃最老的
        }

        #endregion

        #region IDisposable

        private bool isDisposed = false;

        public void Dispose()
        {
            this.Dispose(false);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool byGC)
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                if (!byGC)
                {
                    this.queueBase.Clear();
                }

                this.poolSemaphore.Dispose();
            }
        }

        #endregion
    }
}
