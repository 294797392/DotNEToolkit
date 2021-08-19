using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    public class DSBCAPS
    {
        public static readonly uint DSBCAPS_PRIMARYBUFFER = 0x00000001;

        public static readonly uint DSBCAPS_STATIC = 0x00000002;

        /// <summary>
        /// 缓冲区存储在声卡里, 混音是在声卡里做的
        /// </summary>
        public static readonly uint DSBCAPS_LOCHARDWARE = 0x00000004;

        /// <summary>
        /// 缓冲区存储在内存里, 混音是CPU做的
        /// </summary>
        public static readonly uint DSBCAPS_LOCSOFTWARE = 0x00000008;

        /// <summary>
        /// The sound source can be moved in 3D space. 
        /// </summary>
        public static readonly uint DSBCAPS_CTRL3D = 0x00000010;

        /// <summary>
        /// 可以控制声音的频率
        /// </summary>
        public static readonly uint DSBCAPS_CTRLFREQUENCY = 0x00000020;

        /// <summary>
        /// The sound source can be moved from left to right. 
        /// </summary>
        public static readonly uint DSBCAPS_CTRLPAN = 0x00000040;

        /// <summary>
        /// 可获取或设置音量大小
        /// </summary>
        public static readonly uint DSBCAPS_CTRLVOLUME = 0x00000080;

        /// <summary>
        /// 缓冲区通知功能
        /// </summary>
        public static readonly uint DSBCAPS_CTRLPOSITIONNOTIFY = 0x00000100;

        /// <summary>
        /// Effects can be added to the buffer. 
        /// </summary>
        public static readonly uint DSBCAPS_CTRLFX = 0x00000200;

        public static readonly uint DSBCAPS_STICKYFOCUS = 0x00004000;

        /// <summary>
        /// 失去焦点继续播放功能
        /// </summary>
        public static readonly uint DSBCAPS_GLOBALFOCUS = 0x00008000;

        public static readonly uint DSBCAPS_GETCURRENTPOSITION2 = 0x00010000;

        public static readonly uint DSBCAPS_MUTE3DATMAXDISTANCE = 0x00020000;

        public static readonly uint DSBCAPS_LOCDEFER = 0x00040000;

        public static readonly uint DSBCAPS_TRUEPLAYPOSITION = 0x00080000;
    }
}
