using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSCCAPS
    {
        public int dwSize;
        public int dwFlags;
        public int dwFormats;
        public int dwChannels;
    }
}
