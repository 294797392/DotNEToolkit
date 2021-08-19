using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [Guid(InterfaceID.IID_IDirectSoundNotify8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundNotify8
    {
        [PreserveSig]
        uint SetNotificationPositions(int dwPositionNotifies, IntPtr pcPositionNotifies);
    }
}
