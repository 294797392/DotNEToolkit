using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 封装实时视频播放器
    /// 
    /// 1. 视频缓冲区管理
    /// 外部调用的时候，只需要调用PuhData和RequestData就可以了
    /// 当外部调用者收到了视频流的时候，调用PutData把视频流放入缓冲区
    /// 播放器会自动使用缓冲区里的视频流数据
    /// </summary>
    public abstract class VideoPlay : EventableModule
    {
        #region 类变量

        /// <summary>
        /// 默认的播放超时时间是5秒钟
        /// </summary>
        private const int DefaultTimeout = 5000;

        /// <summary>
        /// 视频播放超时事件
        /// </summary>
        public const int EV_TIMEOUT = 1;

        #endregion

        #region 实例变量

        private object bufferLock;
        private List<byte> bufferList;

        /// <summary>
        /// 播放超时时间
        /// 如果超过这个时间还是没有取到视频流，那么默认超时，采取的操作是关闭播放器
        /// </summary>
        protected int timeout;

        #endregion

        #region 属性

        /// <summary>
        /// 渲染视频的窗口句柄
        /// </summary>
        public IntPtr Hwnd { get; set; }

        #endregion

        #region ModuleBase

        /// <summary>
        /// 开始播放
        /// </summary>
        /// <returns></returns>
        public abstract int Start();

        /// <summary>
        /// 停止播放
        /// </summary>
        public abstract void Stop();

        protected override int OnInitialize()
        {
            this.timeout = this.GetInputValue<int>("timeout", DefaultTimeout);

            this.bufferLock = new object();
            this.bufferList = new List<byte>();

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        #endregion

        #region 实例方法

        #endregion

        #region 公开接口

        /// <summary>
        /// 把视频数据放入缓冲区
        /// </summary>
        /// <param name="videoData"></param>
        public void PutData(byte[] videoData)
        {
            lock (this.bufferLock)
            {
                this.bufferList.AddRange(videoData);
            }
        }

        /// <summary>
        /// 一次性返回缓冲区里的所有数据
        /// </summary>
        /// <returns></returns>
        protected byte[] RequestData()
        {
            lock (this.bufferLock)
            {
                if (this.bufferList.Count == 0)
                {
                    return null;
                }

                byte[] videoData = this.bufferList.ToArray();
                this.bufferList.Clear();
                return videoData;
            }
        }

        /// <summary>
        /// 从缓冲区里拿到小于等于requestSize长度的数据
        /// 如果缓冲区里没有数据，那么会返回false，否则返回true
        /// </summary>
        /// <param name="requestSize">要获取的数据长度</param>
        /// <param name="buffer">存储视频数据的缓冲区</param>
        /// <returns></returns>
        protected bool RequestData(int requestSize, out byte[] buffer)
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
                    // 缓冲区里的数不够用的
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

        protected bool RequestData(int requestSize, int timeout, out byte[] buffer)
        {
            buffer = null;

            int timeoutRemain = timeout;

            while (timeoutRemain > 0)
            {
                if (!this.RequestData(requestSize, out buffer))
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
    }
}
