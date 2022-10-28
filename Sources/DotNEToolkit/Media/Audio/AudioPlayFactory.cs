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
        /// 使用DirectSound录音
        /// </summary>
        DirectSound
    }

    public static class AudioPlayFactory
    {
        public static AudioPlay Create(AudioPlayType playType)
        {
            switch (playType)
            {
                case AudioPlayType.DirectSound: return new DirectSoundPlay();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
