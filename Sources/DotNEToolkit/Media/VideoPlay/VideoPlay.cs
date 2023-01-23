using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    /// <summary>
    /// 封装实时视频播放器
    /// 
    /// 1. 视频缓冲区管理
    /// 外部调用的时候，只需要调用PuhData和RequestData就可以了
    /// 当外部调用者收到了视频流的时候，调用PutData把视频流放入缓冲区
    /// 播放器会自动使用缓冲区里的视频流数据
    /// </summary>
    public abstract class VideoPlay : MediaPlay
    {
        #region 类变量

        /// <summary>
        /// 默认的播放超时时间是5秒钟
        /// </summary>
        private const int DefaultTimeout = 5000;

        ///// <summary>
        ///// 视频播放超时
        ///// 当播放的是实时流的时候，才可能会触发这个事件
        ///// </summary>
        //public const int EV_TIMEOUT = 1;

        /// <summary>
        /// 视频播放结束
        /// 当播放的是文件的时候，才可能会触发这个事件
        /// </summary>
        public const int EV_EOF = 2;

        #endregion

        #region 实例变量

        internal MediaStream videoStream;

        /// <summary>
        /// 播放超时时间
        /// 如果超过这个时间还是没有取到视频流，那么默认超时，采取的操作是关闭播放器
        /// </summary>
        protected int timeout;

        #endregion

        #region 属性

        /// <summary>
        /// 渲染视频的窗口句柄
        /// 音频写Inptr.Zero
        /// </summary>
        public IntPtr Hwnd { get; set; }

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            base.OnInitialize();

            this.timeout = this.GetParameter<int>("timeout", DefaultTimeout);
            this.videoStream = MediaStream.Create();

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        #endregion

        #region 抽象方法

        #endregion

        #region 实例方法

        /// <summary>
        /// 写入媒体数据
        /// </summary>
        /// <param name="videoData"></param>
        public void Write(byte[] videoData)
        {
            this.videoStream.Write(videoData);
        }

        #endregion
    }
}
