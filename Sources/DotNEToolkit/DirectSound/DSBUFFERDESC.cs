using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static DotNEToolkit.Win32API;

namespace DotNEToolkit.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSBUFFERDESC
    {
        public int dwSize;
        public uint dwFlags;

        /// <summary>
        /// 播放或者捕获缓冲区大小
        /// </summary>
        public int dwBufferBytes;
        public int dwReserved;
        public IntPtr lpwfxFormat;
        public GUID guid3DAlgorithm;
    }
}
