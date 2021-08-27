using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 一个简单的PCM播放器
    /// </summary>
    public abstract class AudioPlay
    {
        #region 属性

        /// <summary>
        /// 采样通道数
        /// </summary>
        public short Channel { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public int SamplesPerSec { get; set; }

        /// <summary>
        /// 每个采样大小是16位
        /// </summary>
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

        public AudioPlay()
        {
            this.Channel = 2;
            this.SamplesPerSec = 44100;
            this.BitsPerSample = 16;
        }

        /// <summary>
        /// 打开播放器
        /// </summary>
        /// <returns></returns>
        public abstract int Open();

        /// <summary>
        /// 关闭播放器
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 播放一个音频文件
        /// 该方法是同步方法，会阻塞当前线程直到音频文件播放完了为止
        /// </summary>
        /// <param name="fileURI">要播放的音频文件的地址</param>
        public abstract int PlayFile(string fileURI);
    }
}
