using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media.Video
{
    /// <summary>
    /// 视频解码器类型
    /// </summary>
    public enum VideoDecodeType
    {
        libvideo
    }

    public static class VideoDecodeFactory
    {
        public static VideoDecode Create(VideoDecodeType type)
        {
            switch (type)
            {
                case VideoDecodeType.libvideo: return new libvideoVideoDecode();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
