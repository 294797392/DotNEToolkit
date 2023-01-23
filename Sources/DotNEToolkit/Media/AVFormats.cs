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
        /// H264视频格式
        /// </summary>
        AV_FORMAT_H264,

        /// <summary>
        /// PCM音频原始数据格式
        /// </summary>
        AV_FORMAT_PCM,

        /// <summary>
        /// G711音频编码方式
        /// </summary>
        AV_FORMAT_G711_ALAW,
        AV_FORMAT_G711_ULAW
    }
}
