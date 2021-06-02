using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 表示媒体播放器的状态
    /// </summary>
    public enum MPStates
    {
        /// <summary>
        /// 已停止/空闲状态
        /// </summary>
        Stopped,

        /// <summary>
        /// 播放中
        /// </summary>
        Playing,

        /// <summary>
        /// 暂停了
        /// </summary>
        Paused,
    }
}
