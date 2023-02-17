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

        #endregion

        #region 属性

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            base.OnInitialize();

            return DotNETCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        #endregion

        #region 抽象方法

        #endregion

        #region 实例方法

        #endregion
    }
}
