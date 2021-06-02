using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DotNEToolkit.Win32API;

namespace DotNEToolkit.DirectX.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSCEFFECTDESC
    {
        public int dwSize;
        public int dwFlags;
        public GUID guidDSCFXClass;
        public GUID guidDSCFXInstance;
        public int dwReserved1;
        public int dwReserved2;
    }
}