using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    public class DSBSTATUS
    {
        public static readonly uint DSBSTATUS_PLAYING = 0x00000001;

        public static readonly uint DSBSTATUS_BUFFERLOST = 0x00000002;

        public static readonly uint DSBSTATUS_LOOPING = 0x00000004;

        public static readonly uint DSBSTATUS_LOCHARDWARE = 0x00000008;

        public static readonly uint DSBSTATUS_LOCSOFTWARE = 0x00000010;

        public static readonly uint DSBSTATUS_TERMINATED = 0x00000020;
    }
}
