using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    /// <summary>
    /// 播放器类型
    /// </summary>
    public enum VideoPlayType
    {
        /// <summary>
        /// 使用libvlc播放
        /// </summary>
        libvlc,
    }

    public static class VideoPlayFactory
    {
        public static VideoPlay Create(VideoPlayType type)
        {
            switch (type)
            {
                case VideoPlayType.libvlc: return new libvlcPlay();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
