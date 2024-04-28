using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DotNEToolkit.Utility
{
    /// <summary>
    /// 定时器回调函数
    /// </summary>
    /// <param name="userData"></param>
    public delegate void TimerCallback(TimerHandle timer, object userData);

    /// <summary>
    /// 定义定时器的执行粒度
    /// </summary>
    public enum TimerGranularities
    {
        /// <summary>
        /// 粒度设置为按照秒触发
        /// </summary>
        Second,

        /// <summary>
        /// 粒度设置为按照分钟触发
        /// </summary>
        Minute,

        ///// <summary>
        ///// 粒度设置为按照小时触发
        ///// </summary>
        //Hour,

        ///// <summary>
        ///// 粒度设置为按照天触发
        ///// </summary>
        //Day
    }

    public class TimerHandle
    {
        /// <summary>
        /// 定时器名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 定时器粒度
        /// </summary>
        public TimerGranularities Interval { get; set; }

        /// <summary>
        /// 粒度值
        /// </summary>
        public int GranularityValue { get; set; }

        /// <summary>
        /// 当定时器到期的时候触发的回调
        /// </summary>
        public TimerCallback Callback { get; set; }

        /// <summary>
        /// 定时器关联的用户数据
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// 剩余多少毫秒可以执行
        /// </summary>
        public long NextInterval { get; set; }
    }

    /// <summary>
    /// 可以在某一个指定的时刻执行指定的任务的定时器
    /// 线程安全的
    /// </summary>
    public class TimerUtils : SingletonObject<TimerUtils>
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("TimerUtils");

        #region 实例变量

        private List<TimerHandle> timers;
        private object timersLock;
        private bool timersChanged;
        private Thread timerThread;
        private AutoResetEvent timerEvent;        // 控制剩余时间的事件
        private ManualResetEvent timerEvent2;       // 控制定时器线程是否工作的事件

        #endregion

        #region 构造方法

        public TimerUtils()
        {
            this.timers = new List<TimerHandle>();
            this.timersLock = new object();
            this.timerEvent = new AutoResetEvent(false);
            this.timerEvent2 = new ManualResetEvent(true);
        }

        #endregion

        #region 实例方法

        private void ExecuteTimer(TimerHandle timer, long elapsed)
        {
            timer.NextInterval -= elapsed;
            if (timer.NextInterval <= 0)
            {
                //logger.InfoFormat("开始执行定时器, {0}", timer.Name);

                try
                {
                    timer.Callback(timer, timer.UserData);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("定时器执行异常, {0}, {1}", timer.Name, ex);
                }

                // 计算定时器下次执行剩余时间
                timer.NextInterval = this.GetNextInterval(timer.Interval, timer.GranularityValue);
            }
        }

        private long GetNextInterval(TimerGranularities granularity, int value)
        {
            switch (granularity)
            {
                //case TimerGranularities.Day:
                //    {
                //        DateTime now = DateTime.Now;
                //        DateTime endTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                //        TimeSpan timeSpan = endTime - now;
                //        return (int)timeSpan.TotalMilliseconds + 1000; // 加1000毫秒的余量
                //    }

                case TimerGranularities.Second:
                    {
                        return value * 1000;
                    }

                case TimerGranularities.Minute:
                    {
                        // 下一次要触发定时器的分钟数为value的整倍数
                        // 比如value是5，那么触发的时机就是5，10，15，20分钟
                        //int nextMinute = (int)Math.Ceiling((double)DateTime.Now.Minute / value);
                        //return (nextMinute - DateTime.Now.Minute) * 60 * 1000;
                        return value * 60 * 1000;
                    }

                //case TimerGranularities.Hour:
                //    {
                //        return (60 - DateTime.Now.Minute) * 60 * 1000 + 500; // 加500毫秒的余量
                //    }

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 新建一个定时器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="granularity">设置定时器的粒度</param>
        /// <param name="value">定时器的粒度值</param>
        /// <param name="callback"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public TimerHandle CreateTimer(string name, TimerGranularities granularity, int value, TimerCallback callback, object userData)
        {
            // 计算多少毫秒之后该定时器执行
            long nextInterval = this.GetNextInterval(granularity, value);

            TimerHandle timerHandle = new TimerHandle()
            {
                Name = name,
                Interval = granularity,
                Callback = callback,
                UserData = userData,
                NextInterval = nextInterval,
                GranularityValue = value
            };

            lock (this.timersLock)
            {
                this.timers.Add(timerHandle);
                this.timersChanged = true;
            }

            if (this.timerThread == null)
            {
                this.timerThread = new Thread(this.TimerThreadProc);
                this.timerThread.IsBackground = true;
                this.timerThread.Start();
            }
            else
            {
                // 如果此时定时器线程正在休眠，那么唤醒定时器线程
                this.timerEvent2.Set();

                // 唤醒定时器，让定时器重新计算下一个等待时间
                this.timerEvent.Set();
            }

            return timerHandle;
        }

        public void DeleteTimer(TimerHandle handle)
        {
            lock (this.timersLock)
            {
                this.timers.Remove(handle);
                this.timersChanged = true;
            }

            if (this.timers.Count == 0)
            {
                logger.InfoFormat("剩余定时器数量为0, 休眠定时器线程");
                this.timerEvent2.Reset();
            }
            else
            {
                // 唤醒定时器，让定时器重新计算下一个等待时间
                // 如果此时还没调用WaitOne在等待，那么下次调用WaitOne等待的时候将失效
                this.timerEvent.Set();
            }
        }

        #endregion

        #region 事件处理器

        private void TimerThreadProc()
        {
            logger.InfoFormat("定时器线程启动成功");

            Stopwatch stopwatch = new Stopwatch();
            List<TimerHandle> timers = new List<TimerHandle>();

            while (true)
            {
                this.timerEvent2.WaitOne();

                if (this.timersChanged)
                {
                    lock (this.timersLock)
                    {
                        timers = this.timers.ToList();
                        this.timersChanged = false;
                    }
                }

                // 永远选择最近要触发的定时器作为超时时间
                long timeout = timers.Min(v => v.NextInterval);

                //logger.InfoFormat("定时器线程剩余执行时间 = {0}毫秒", timeout);

                stopwatch.Start();
                this.timerEvent.WaitOne(TimeSpan.FromTicks(timeout * 10000));
                stopwatch.Stop();

                // 经过的毫秒数
                long elapsed = stopwatch.ElapsedMilliseconds;

                // 执行定时器
                foreach (TimerHandle timer in timers)
                {
                    this.ExecuteTimer(timer, elapsed);
                }
            }
        }

        #endregion
    }
}
