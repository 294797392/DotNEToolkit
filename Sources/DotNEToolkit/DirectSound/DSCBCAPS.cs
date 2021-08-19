using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSCBCAPS
    {
        public int dwSize;
        public int dwFlags;
        public int dwBufferBytes;
        public int dwReserved;
    }
}
