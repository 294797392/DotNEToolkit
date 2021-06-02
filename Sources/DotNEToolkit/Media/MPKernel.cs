using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 播放器核心引擎
    /// </summary>
    public abstract class MPKernel
    {
        /// <summary>
        /// 播放进度回调事件
        /// </summary>
        public event Action<MPKernel, double> ProgressChanged;

        /// <summary>
        /// 获取以秒为单位的媒体文件时长
        /// </summary>
        /// <returns></returns>
        public abstract int GetDuration();

        /// <summary>
        /// 通过URI打开播放器
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public abstract int Open(string uri);

        /// <summary>
        /// 开始播放
        /// </summary>
        public abstract int Play();

        /// <summary>
        /// 停止播放
        /// 所有的播放数据会重置
        /// </summary>
        /// <returns></returns>
        public abstract int Stop();

        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <returns></returns>
        public abstract int Pause();

        /// <summary>
        /// 继续播放
        /// </summary>
        /// <returns></returns>
        public abstract int Resume();

        /// <summary>
        /// 快进按钮
        /// </summary>
        /// <param name="ts">快进的时间</param>
        /// <returns></returns>
        public abstract int FastForward(TimeSpan ts);

        /// <summary>
        /// 快退按钮
        /// </summary>
        /// <param name="ts">倒退的时间</param>
        /// <returns></returns>
        public abstract int FastRewind(TimeSpan ts);

        /// <summary>
        /// 关闭播放器
        /// </summary>
        public abstract void Close();

        protected void NotifyProgressChanged(double progress)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, progress);
            }
        }
    }
}
