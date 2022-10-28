using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    internal abstract class RealtimeStream
    {
        #region 抽象接口

        /// <summary>
        /// 从缓冲区里拿到小于等于requestSize长度的数据
        /// 如果缓冲区里没有数据，那么会返回false，否则返回true
        /// 注意buffer的长度有可能比requestSize小
        /// </summary>
        /// <param name="requestSize">要获取的数据长度</param>
        /// <param name="buffer">存储视频数据的缓冲区</param>
        /// <returns></returns>
        internal abstract bool Read(int requestSize, out byte[] buffer);

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

        #endregion

        #region 公开接口

        /// <summary>
        /// 使用一个超时时间读取数据
        /// </summary>
        /// <param name="requestSize"></param>
        /// <param name="timeout"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal bool Read(int requestSize, int timeout, out byte[] buffer)
        {
            buffer = null;

            int timeoutRemain = timeout;

            while (timeoutRemain > 0)
            {
                if (!this.Read(requestSize, out buffer))
                {
                    Thread.Sleep(100);
                    timeoutRemain -= 100;
                    continue;
                }

                return true;
            }

            return false;
        }

        #endregion

        public static RealtimeStream Create()
        {
            return new DefaultRealtimeStreamImpl();
        }
    }

    internal class DefaultRealtimeStreamImpl : RealtimeStream
    {
        #region 实例变量

        private object bufferLock;
        private List<byte> bufferList;

        #endregion

        #region 构造方法

        public DefaultRealtimeStreamImpl() 
        {
            this.bufferLock = new object();
            this.bufferList = new List<byte>();
        }

        #endregion

        #region RealtimeStream

        internal override bool Read(int requestSize, out byte[] buffer)
        {
            buffer = null;

            lock (this.bufferLock)
            {
                if (this.bufferList.Count == 0)
                {
                    // 此时缓冲区里没有数据
                    return false;
                }

                if (requestSize > this.bufferList.Count)
                {
                    // 缓冲区里的数据不够用的，那么一次性全部返回
                    buffer = this.bufferList.ToArray();
                    this.bufferList.Clear();
                    return true;
                }

                buffer = new byte[requestSize];
                this.bufferList.CopyTo(0, buffer, 0, requestSize);
                this.bufferList.RemoveRange(0, requestSize);
                return true;
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

        #endregion
    }
}


