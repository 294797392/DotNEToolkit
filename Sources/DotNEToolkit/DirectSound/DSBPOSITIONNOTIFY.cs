using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSBPOSITIONNOTIFY
    {
        public uint dwOffset;
        public IntPtr hEventNotify;
    }
}
