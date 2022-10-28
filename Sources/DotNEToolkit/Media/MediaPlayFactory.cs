using DotNEToolkit.Media.Audio;
using DotNEToolkit.Media.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    public static class MediaPlayFactory
    {
        public static MediaPlay CreateAudioPlay(AudioPlayType type)
        {
            switch (type)
            {
                case AudioPlayType.DirectSound: return new DirectSoundPlay();
            }
        }

        public static MediaPlay CreateVideoPlay(VideoPlayType type)
        {

        }

        public static MediaPlay CreateAVPlay()
        {

        }
    }
}
