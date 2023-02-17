using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    internal abstract class MediaStream
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("RealtimeStream");

        #endregion

        #region 属性

        /// <summary>
        /// 当前的数据总大小
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// 流是否被关闭
        /// </summary>
        internal bool IsClosed { get; set; }

        #endregion

        #region 抽象接口

        /// <summary>
        /// 从缓冲区里拿到小于等于buffer长度的数据
        /// 如果缓冲区里没有数据，那么会返回false，否则返回true
        /// 注意buffer的长度有可能比buffer.Length小
        /// </summary>
        /// <param name="buffer">存储视频数据的缓冲区</param>
        /// <returns>
        /// 0：没取到数据
        /// 大于0：取到的数据大小
        /// </returns>
        internal abstract int Read(byte[] buffer);

        /// <summary>
        /// 从缓冲区里拿到等于buffer长度的数据
        /// 如果当前缓冲区的大小小于buffer，那么直接返回0
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal abstract int Read2(byte[] buffer);

        /// <summary>
        /// 一次性读取缓冲区里的所有数据
        /// 如果缓冲区里没有数据，那么返回null
        /// </summary>
        /// <returns></returns>
        internal abstract byte[] ReadAll();

        /// <summary>
        /// 把数据写入缓冲区
        /// </summary>
        /// <param name="buffer"></param>
        internal abstract void Write(byte[] buffer);

        /// <summary>
        /// 清空缓冲区中的所有数据
        /// </summary>
        internal abstract void Clear();

        #endregion

        #region 公开接口

        ///// <summary>
        ///// 使用一个超时时间读取数据
        ///// </summary>
        ///// <param name="requestSize"></param>
        ///// <param name="timeout"></param>
        ///// <param name="buffer"></param>
        ///// <returns></returns>
        //internal bool Read(int requestSize, int timeout, out byte[] buffer)
        //{
        //    buffer = new byte[requestSize];

        //    int timeoutRemain = timeout;

        //    while (timeoutRemain > 0 && !this.IsClosed)
        //    {
        //        if (!this.Read(buffer))
        //        {
        //            Thread.Sleep(5);
        //            timeoutRemain -= 5;
        //            continue;
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        #endregion

        public static MediaStream Create()
        {
            return new RealtimeMediaStream();
        }
    }

    internal class RealtimeMediaStream : MediaStream
    {
        #region 实例变量

        private object bufferLock;
        private List<byte> bufferList;

        #endregion

        public override int Size => this.bufferList.Count;

        #region 构造方法

        public RealtimeMediaStream()
        {
            this.bufferLock = new object();
            this.bufferList = new List<byte>();
        }

        #endregion

        #region RealtimeStream

        internal override int Read(byte[] buffer)
        {
            buffer = null;

            int requestSize = buffer.Length;

            lock (this.bufferLock)
            {
                if (this.bufferList.Count == 0)
                {
                    // 此时缓冲区里没有数据
                    return 0;
                }

                // 缓冲区里的数据不够用的，那么一次性全部返回
                if (this.bufferList.Count < requestSize)
                {
                    this.bufferList.CopyTo(buffer);
                    this.bufferList.Clear();
                    return this.bufferList.Count;
                }

                // 缓冲区里的数据比buffer大
                this.bufferList.CopyTo(0, buffer, 0, buffer.Length);
                this.bufferList.RemoveRange(0, requestSize);
                return buffer.Length;
            }
        }

        internal override int Read2(byte[] buffer)
        {
            buffer = null;

            int requestSize = buffer.Length;

            lock (this.bufferLock)
            {
                if (this.bufferList.Count == 0)
                {
                    // 此时缓冲区里没有数据
                    return 0;
                }

                // 缓冲区里的数据不够用的，那么一次性全部返回
                if (this.bufferList.Count < requestSize)
                {
                    return 0;
                }

                // 缓冲区里的数据比buffer大
                this.bufferList.CopyTo(0, buffer, 0, requestSize);
                this.bufferList.RemoveRange(0, requestSize);
                return buffer.Length;
            }
        }

        internal override byte[] ReadAll()
        {
            lock (this.bufferLock)
            {
                if (this.bufferList.Count == 0)
                {
                    return null;
                }

                byte[] buffer = this.bufferList.ToArray();
                this.bufferList.Clear();
                return buffer;
            }
        }

        internal override void Write(byte[] buffer)
        {
            lock (this.bufferLock)
            {
                this.bufferList.AddRange(buffer);
            }
        }

        internal override void Clear()
        {
            lock (this.bufferLock)
            {
                this.bufferList.Clear();
            }
        }

        #endregion
    }
}


