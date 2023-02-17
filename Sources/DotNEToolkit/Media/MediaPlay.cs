using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    public abstract class MediaPlay : ModuleBase
    {
        #region 类变量

        /// <summary>
        /// 默认的播放超时时间是5秒钟
        /// </summary>
        private const int DefaultTimeout = 5000;

        #endregion

        #region 公开事件

        /// <summary>
        /// 播放器状态发生改变的时候触发
        /// </summary>
        public event Action<MediaPlay, MediaPlayStatus> StatusChanged;

        #endregion

        #region 实例变量

        internal MediaStream stream;

        /// <summary>
        /// 播放超时时间
        /// 如果超过这个时间还是没有取到视频流，那么默认超时，采取的操作是关闭播放器
        /// </summary>
        protected int timeout;

        protected AVFormats format;

        #endregion

        #region 属性

        /// <summary>
        /// 渲染视频的窗口句柄
        /// 音频写Inptr.Zero
        /// </summary>
        public IntPtr Hwnd { get; set; }

        /// <summary>
        /// 指定要播放的文件的路径
        /// 可以是网络路径
        /// </summary>
        public string FileURI
        {
            get; set;
        }

        /// <summary>
        /// 该播放器支持的格式
        /// </summary>
        public AVFormats Format { get { return this.format; } }

        /// <summary>
        /// 当前播放状态
        /// </summary>
        public MediaPlayStatus PlayStatus { get; protected set; }

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.timeout = this.GetParameter<int>("timeout", DefaultTimeout);
            this.format = this.GetParameter<AVFormats>("format");
            this.stream = MediaStream.Create();
            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 开始播放
        /// </summary>
        /// <returns></returns>
        public abstract int Start();

        /// <summary>
        /// 停止播放
        /// </summary>
        public abstract void Stop();

        #endregion

        #region 公开接口

        /// <summary>
        /// 写入媒体数据
        /// </summary>
        /// <param name="videoData"></param>
        public void Write(byte[] videoData)
        {
            this.stream.Write(videoData);
        }

        #endregion

        #region 实例方法

        protected void NotifyStatusChanged(MediaPlayStatus status)
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, status);
            }
        }

        #endregion
    }
}
