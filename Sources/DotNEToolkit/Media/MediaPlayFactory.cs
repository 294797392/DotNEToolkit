using DotNEToolkit.Media.Audio;
using DotNEToolkit.Media.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 创建返回值是MediaPlay的播放器实例
    /// </summary>
    public static class MediaPlayFactory
    {
        public static MediaPlay CreateAudioPlay(AudioPlayType type)
        {
            switch (type)
            {
                case AudioPlayType.DirectSound: return new DirectSoundPlay();
                case AudioPlayType.libvlc: return new libvlcPlay();
                default:
                    throw new NotImplementedException();
            }
        }

        public static MediaPlay CreateVideoPlay(VideoPlayType type)
        {
            switch (type)
            {
                case VideoPlayType.libvlc: return new libvlcPlay();
                default:
                    throw new NotImplementedException();
            }
        }

        public static MediaPlay CreateAVPlay()
        {
            throw new NotImplementedException();
        }
    }
}
