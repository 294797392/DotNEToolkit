using DotNEToolkit.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectX.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSEFFECTDESC
    {
        public int dwSize;
        public int dwFlags;
        public GUID guidDSFXClass;
        public int dwReserved1;
        public int dwReserved2;
    }
}