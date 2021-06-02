using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNEToolkit.DirectX.DirectSound
{
    [ComImport]
    [Guid(IID.IID_IDirectSound8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSound8
    {
        [PreserveSig]
        int CreateSoundBuffer(ref DSBUFFERDESC pcDSBufferDesc, out IntPtr ppDSBuffer, IntPtr pUnkOuter);

        [PreserveSig]
        int GetCaps(out DSCAPS pDSCaps);

        [PreserveSig]
        int DuplicateSoundBuffer(IntPtr pDSBufferOriginal, out IntPtr ppDSBufferDuplicate);

        [PreserveSig]
        int SetCooperativeLevel(IntPtr hwnd, uint dwLevel);

        [PreserveSig]
        int Compact();

        [PreserveSig]
        int GetSpeakerConfig(out uint pdwSpeakerConfig);

        [PreserveSig]
        int SetSpeakerConfig(uint dwSpeakerConfig);

        [PreserveSig]
        int Initialize(IntPtr pcGuidDevice);

        [PreserveSig]
        int VerifyCertification(out uint pdwCertified);
    }
}