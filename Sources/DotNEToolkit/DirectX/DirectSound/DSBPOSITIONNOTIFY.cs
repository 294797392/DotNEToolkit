using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNEToolkit.DirectX.DirectSound
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DSBPOSITIONNOTIFY
    {
        public uint dwOffset;
        public IntPtr hEventNotify;
    }
}