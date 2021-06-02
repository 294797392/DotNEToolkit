using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNEToolkit.DirectX.DirectSound
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