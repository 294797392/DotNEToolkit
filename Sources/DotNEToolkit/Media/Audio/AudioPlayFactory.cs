using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media.Audio
{
    /// <summary>
    /// 录音机类型
    /// </summary>
    public enum AudioPlayType
    {
        /// <summary>
        /// 使用DirectSound播放音频你
        /// </summary>
        DirectSound,

        /// <summary>
        /// 使用libvlc播放音频
        /// </summary>
        libvlc,

        /// <summary>
        /// 使用WaveAPI
        /// </summary>
        WaveAPI
    }

    public static class AudioPlayFactory
    {
        public static AudioPlay Create(AudioPlayType playType)
        {
            switch (playType)
            {
                case AudioPlayType.DirectSound: return new DirectSoundPlay();
                case AudioPlayType.WaveAPI: return new WaveAPIPlay();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
