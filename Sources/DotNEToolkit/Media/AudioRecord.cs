using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    public abstract class AudioRecord
    {
        #region 事件处理器

        /// <summary>
        /// 录到了数据会触发
        /// </summary>
        public event Action<AudioRecord, byte[]> DataReceived;

        /// <summary>
        /// 当录音过程中遇到错误的时候触发
        /// </summary>
        public event Action<AudioRecord, int> Failed;

        #endregion

        #region 实例变量

        #endregion

        #region 属性

        /// <summary>
        /// 采样通道数
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public int SamplesPerSec { get; set; }

        /// <summary>
        /// 每个采样大小是16位
        /// </summary>
        public int BitsPerSample { get; set; }

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

        public AudioRecord()
        {
            this.Channel = 2;
            this.SamplesPerSec = 44100;
            this.BitsPerSample = 16;
        }

        #endregion

        #region 实例方法

        public virtual int Initialize()
        {
            return DotNETCode.SUCCESS;
        }

        public virtual void Release()
        { }

        protected void NotifyDataReceived(byte[] audioData)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(this, audioData);
            }
        }

        protected void NotifyFailed(int code)
        {
            if (this.Failed != null)
            {
                this.Failed(this, code);
            }
        }

        #endregion

        #region 抽象方法

        public abstract int Start();

        public abstract void Stop();

        #endregion
    }
}
