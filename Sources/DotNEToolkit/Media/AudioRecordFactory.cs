using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 录音机类型
    /// </summary>
    public enum AudioRecordType
    {
        /// <summary>
        /// 使用DirectSound录音
        /// </summary>
        DirectSound,

        /// <summary>
        /// 使用waveIn API录音
        /// </summary>
        WaveAPI
    }

    /// <summary>
    /// 录音机工厂
    /// </summary>
    public static class AudioRecordFactory
    {
        public static AudioRecord Create(AudioRecordType engine)
        {
            switch (engine)
            {
                case AudioRecordType.DirectSound: return new DirectSoundRecord();
                case AudioRecordType.WaveAPI: return new WaveAPIAudioRecord();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
