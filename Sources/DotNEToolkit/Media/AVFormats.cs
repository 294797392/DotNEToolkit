using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 定义音视频格式
    /// </summary>
    public enum AVFormats
    {
        /// <summary>
        /// 未知格式
        /// 播放器会自动检测
        /// </summary>
        Unkown,

        /// <summary>
        /// H264视频格式
        /// </summary>
        H264,

        /// <summary>
        /// PCM音频原始数据格式
        /// </summary>
        PCM,

        /// <summary>
        /// G711音频编码方式
        /// </summary>
        G711_ALAW,
        G711_ULAW
    }
}
