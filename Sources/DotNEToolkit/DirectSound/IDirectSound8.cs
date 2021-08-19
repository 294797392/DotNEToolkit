using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [ComImport]
    [Guid(InterfaceID.IID_IDirectSound8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSound8
    {
        [PreserveSig]
        uint CreateSoundBuffer(ref DSBUFFERDESC pcDSBufferDesc, out IntPtr ppDSBuffer, IntPtr pUnkOuter);

        [PreserveSig]
        uint GetCaps(out DSCAPS pDSCaps);

        [PreserveSig]
        uint DuplicateSoundBuffer(IntPtr pDSBufferOriginal, out IntPtr ppDSBufferDuplicate);

        [PreserveSig]
        uint SetCooperativeLevel(IntPtr hwnd, uint dwLevel);

        [PreserveSig]
        uint Compact();

        [PreserveSig]
        uint GetSpeakerConfig(out uint pdwSpeakerConfig);

        [PreserveSig]
        uint SetSpeakerConfig(uint dwSpeakerConfig);

        [PreserveSig]
        uint Initialize(IntPtr pcGuidDevice);

        [PreserveSig]
        uint VerifyCertification(out uint pdwCertified);
    }
}
