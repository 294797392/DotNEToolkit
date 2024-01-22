using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Utility
{
    /// <summary>
    /// 定时器回调函数
    /// </summary>
    /// <param name="userData"></param>
    public delegate void TimerCallback(TimerHandle timer);

    /// <summary>
    /// 定义定时器的执行时机
    /// </summary>
    public enum ExecutionTiming
    {
        /// <summary>
        /// 每小时触发定时器
        /// </summary>
        Hour,

        /// <summary>
        /// 每天凌晨0点触发定时器
        /// </summary>
        Day
    }

    public class TimerHandle
    {
        /// <summary>
        /// 定时器名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 定时器间隔时间
        /// </summary>
        public ExecutionTiming Interval { get; set; }

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
        public int RemainMS { get; set; }
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
        private List<TimerHandle> timersCopy;
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

        private void ExecuteTimer(TimerHandle timer, int elapsed)
        {
            timer.RemainMS -= elapsed;
            if (timer.RemainMS <= 0)
            {
                logger.InfoFormat("开始执行定时器, {0}", timer.Name);

                try
                {
                    timer.Callback(timer);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("定时器执行异常, {0}, {1}", timer.Name, ex);
                }
                timer.RemainMS = this.GetRemainMS(timer.Interval);
            }
        }

        private int GetRemainMS(ExecutionTiming interval)
        {
            switch (interval)
            {
                case ExecutionTiming.Day:
                    {
                        DateTime now = DateTime.Now;
                        DateTime endTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                        TimeSpan timeSpan = endTime - now;
                        return (int)timeSpan.TotalMilliseconds + 1000; // 加1000毫秒的余量
                    }

                case ExecutionTiming.Hour:
                    {
                        return (60 - DateTime.Now.Minute) * 60 * 1000 + 500; // 加500毫秒的余量
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region 公开接口

        public TimerHandle CreateTimer(string name, ExecutionTiming interval, TimerCallback callback, object userData)
        {
            // 计算多少毫秒之后该定时器执行
            int remainMS = this.GetRemainMS(interval);

            TimerHandle timerHandle = new TimerHandle()
            {
                Name = name,
                Interval = interval,
                Callback = callback,
                UserData = userData,
                RemainMS = remainMS
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
                // 线程已经创建了，但是创建完Timer之后列表里只有一个元素，说明此时该线程正在休眠状态，需要唤醒
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

            while (true)
            {
                this.timerEvent2.WaitOne();

                if (this.timersChanged)
                {
                    lock (this.timersLock)
                    {
                        this.timersCopy = this.timers.ToList();
                        this.timersChanged = false;
                    }
                }

                // 没有定时器，那么直接休眠
                if (this.timersCopy.Count == 0)
                {
                    Thread.Sleep(50);
                    continue;
                }

                // 永远选择最近要触发的定时器作为超时时间
                int timeout = this.timersCopy.Min(v => v.RemainMS);

                logger.InfoFormat("定时器线程剩余执行时间 = {0}毫秒", timeout);

                DateTime start = DateTime.Now;
                this.timerEvent.WaitOne(timeout);
                DateTime end = DateTime.Now;

                // 经过的毫秒数
                int elapsed = (int)(end - start).TotalMilliseconds;

                // 执行定时器
                foreach (TimerHandle timer in this.timersCopy)
                {
                    this.ExecuteTimer(timer, elapsed);
                }
            }
        }

        #endregion
    }
}
