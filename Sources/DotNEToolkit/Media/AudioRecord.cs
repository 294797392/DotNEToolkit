using DotNEToolkit.Bindings;
using DotNEToolkit.Modular;
using DotNEToolkit.Modular.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 表示一个声卡设备
    /// </summary>
    public abstract class AudioDevice
    {
        /// <summary>
        /// 声卡设备的显示名字
        /// </summary>
        public string Name { get; internal set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// 一个录制PCM音频的录音机
    /// </summary>
    public abstract class AudioRecord : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("AudioRecord");

        #endregion

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

        /// <summary>
        /// 要保存的音频文件路径
        /// </summary>
        private string filePath;

        /// <summary>
        /// 保存的音频流文件
        /// </summary>
        private FileStream fileStream;

        /// <summary>
        /// 当前正在使用的声卡设备
        /// </summary>
        protected AudioDevice usedDevice;

        #endregion

        #region 属性

        /// <summary>
        /// 采样通道数
        /// </summary>
        [BindableProperty(2)]
        public short Channel { get; set; }

        /// <summary>
        /// 采样率
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

        public AudioRecord()
        {
        }

        #endregion

        #region ModuleBase

        public override int Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            return DotNETCode.SUCCESS;
        }

        public override void Release()
        {
            base.Release();
        }

        protected void NotifyDataReceived(byte[] audioData)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(this, audioData);
            }

            if (this.fileStream != null)
            {
                //logger.DebugFormat("写入PCM数据, 大小 = {0}", audioData.Length);
                this.fileStream.Write(audioData, 0, audioData.Length);
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

        /// <summary>
        /// 开始录音
        /// </summary>
        /// <returns></returns>
        [ModuleAction]
        public virtual int Start()
        {
            if (!string.IsNullOrEmpty(this.filePath))
            {
                if (File.Exists(this.filePath))
                {
                    File.Delete(this.filePath);
                }

                try
                {
                    this.fileStream = File.Open(this.filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                }
                catch (Exception ex)
                {
                    logger.Error("创建录音文件异常", ex);
                    return DotNETCode.OPEN_FILE_FAILED;
                }
            }

            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        [ModuleAction]
        public virtual void Stop()
        {
            if (this.fileStream != null)
            {
                this.fileStream.Flush();
                this.fileStream.Close();
                this.fileStream.Dispose();
            }
        }

        /// <summary>
        /// 枚举系统里所有的录音设备（可以录音的声卡）
        /// </summary>
        /// <returns></returns>
        [ModuleAction]
        public abstract List<AudioDevice> GetAudioDevices();

        #endregion

        #region 公开接口

        /// <summary>
        /// 设置录音文件的保存路径
        /// </summary>
        /// <param name="filePath"></param>
        [ModuleAction]
        public void SetRecordFile(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// 设置录音程序使用的声卡
        /// </summary>
        /// <param name="device"></param>
        public void SetAudioDevice(AudioDevice device)
        {
            this.usedDevice = device;
        }

        #endregion
    }
}
