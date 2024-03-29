﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [ComImport]
    [Guid(InterfaceID.IID_IDirectSoundCaptureBuffer)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundCaptureBuffer
    {
        int GetCaps([In, Out] ref DSCBCAPS pDSCBCaps);

        int GetCurrentPosition(out int pdwCapturePosition, out int pdwReadPosition);

        int GetFormat(out WAVEFORMATEX pwfxFormat, int dwSizeAllocated, out int pdwSizeWritten);

        int GetStatus(out int pdwStatus);

        int Initialize(IntPtr pDirectSoundCapture, ref DSCBUFFERDESC pcDSCBufferDesc);

        int Lock(int dwOffset, int dwBytes, out IntPtr ppvAudioPtr1, out int pdwAudioBytes1, out IntPtr ppvAudioPtr2, out int pdwAudioBytes2, int dwFlags);

        int Start(int dwFlags);

        int Stop();

        int Unlock(IntPtr pvAudioPtr1, int dwAudioBytes1, IntPtr pvAudioPtr2, int dwAudioBytes2);
    }
}
