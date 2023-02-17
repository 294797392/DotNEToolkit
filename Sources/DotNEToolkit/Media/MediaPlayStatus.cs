using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    public enum MediaPlayStatus
    {
        Stopped,

        Playing,

        ///// <summary>
        ///// 播放超时
        ///// 播放在线音视频的时候触发
        ///// </summary>
        //Timeout,

        /// <summary>
        /// 播放结束
        /// 播放本地音视频文件的时候触发
        /// </summary>
        //Completed
    }
}
