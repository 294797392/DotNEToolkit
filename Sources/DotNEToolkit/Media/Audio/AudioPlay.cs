using DotNEToolkit.Bindings;
using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media.Audio
{
    /// <summary>
    /// 一个简单的PCM播放器
    /// </summary>
    public abstract class AudioPlay : MediaPlay
    {
        #region 属性

        /// <summary>
        /// 采样通道数
        /// </summary>
        [BindableProperty(2)]
        public short Channel { get; set; }

        /// <summary>
        /// 采样率
        /// 每秒钟的采样次数
        /// </summary>
        [BindableProperty(44100)]
        public int SamplesPerSec { get; set; }

        /// <summary>
        /// 每个采样大小是16位
        /// </summary>
        [BindableProperty(16)]
        public short BitsPerSample { get; set; }

        /// <summary>
        /// 块对齐, 每个采样的字节数
        /// </summary>
        public short BlockAlign { get { return (short)(Channel * BitsPerSample / 8); } }

        /// <summary>
        /// 一秒钟音频所占空间大小
        /// </summary>
        public int BytesPerSec { get { return BlockAlign * SamplesPerSec; } }

        /// <summary>
        /// 捕获缓冲区大小和播放缓冲区大小
        /// 缓冲区里的数据是一秒钟的音频数据
        /// </summary>
        public int BufferSize { get { return BlockAlign * SamplesPerSec; } }

        #endregion

        #region 构造方法

        public AudioPlay()
        {
        }

        #endregion

        #region MediaPlay

        protected override int OnInitialize()
        {
            base.OnInitialize();

            this.Channel = this.GetParameter<short>("channel", 2);
            this.SamplesPerSec = this.GetParameter<int>("sampleRate", 44100);
            this.BitsPerSample = this.GetParameter<short>("sampleSize", 16);

            return DotNETCode.SUCCESS;
        }

        #endregion
    }
}
