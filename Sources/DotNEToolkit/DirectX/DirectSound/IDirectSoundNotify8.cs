using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNEToolkit.DirectX.DirectSound
{
    [Guid(IID.IID_IDirectSoundNotify8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundNotify8
    {
        [PreserveSig]
        int SetNotificationPositions(int dwPositionNotifies, IntPtr pcPositionNotifies);
    }
}