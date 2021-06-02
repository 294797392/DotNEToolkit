using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 实现一个媒体播放器
    /// </summary>
    public class MediaPlayer
    {
        #region 公开事件

        /// <summary>
        /// 播放进度回调事件
        /// </summary>
        public event Action<MediaPlayer, double> ProgressChanged;

        /// <summary>
        /// 播放器状态改变回调
        /// </summary>
        public event Action<MediaPlayer, MPStates> StatusChanged;

        #endregion

        #region 实例变量

        /// <summary>
        /// 播放器内核
        /// </summary>
        private MPKernel mpk;

        #endregion

        #region 公开接口

        /// <summary>
        /// 获取以秒为单位的媒体文件时长
        /// </summary>
        /// <returns></returns>
        public int GetDuration()
        {
            return this.mpk.GetDuration();
        }

        /// <summary>
        /// 通过URI打开播放器
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public int Open(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return DotNETCode.InvalidParams;
            }

            return this.mpk.Open(uri);
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        public int Play()
        {
            return this.mpk.Play();
        }

        /// <summary>
        /// 停止播放
        /// 所有的播放数据会重置
        /// </summary>
        /// <returns></returns>
        public int Stop()
        {
            return this.mpk.Stop();
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <returns></returns>
        public int Pause()
        {
            return this.mpk.Pause();
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        /// <returns></returns>
        public int Resume()
        {
            return this.mpk.Resume();
        }

        /// <summary>
        /// 快进按钮
        /// </summary>
        /// <param name="ts">快进的时间</param>
        /// <returns></returns>
        public int FastForward(TimeSpan ts)
        {
            return this.mpk.FastForward(ts);
        }

        /// <summary>
        /// 快退按钮
        /// </summary>
        /// <param name="ts">倒退的时间</param>
        /// <returns></returns>
        public int FastRewind(TimeSpan ts)
        {
            return this.mpk.FastRewind(ts);
        }

        /// <summary>
        /// 关闭播放器
        /// </summary>
        public void Close()
        {
            this.mpk.Close();
        }

        #endregion

        #region 实例方法

        private void NotifyProgressChanged(double progress)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, progress);
            }
        }

        #endregion
    }
}
