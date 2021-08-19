using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    public class DSBPLAY
    {
        /// <summary>
        /// 缓冲区播放完毕之后从缓冲区开始的位置继续播放, 当播放主缓冲区的时候必须设置DSBPLAY_LOOPING
        /// </summary>
        public static readonly uint DSBPLAY_LOOPING = 0x00000001;

        public static readonly uint DSBPLAY_LOCHARDWARE = 0x00000002;

        public static readonly uint DSBPLAY_LOCSOFTWARE = 0x00000004;

        public static readonly uint DSBPLAY_TERMINATEBY_TIME = 0x00000008;

        public static readonly uint DSBPLAY_TERMINATEBY_DISTANCE = 0x000000010;

        public static readonly uint DSBPLAY_TERMINATEBY_PRIORITY = 0x000000020;
    }
}
