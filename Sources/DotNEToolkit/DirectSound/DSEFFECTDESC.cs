using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static DotNEToolkit.Win32API;

namespace DotNEToolkit.DirectSound
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
