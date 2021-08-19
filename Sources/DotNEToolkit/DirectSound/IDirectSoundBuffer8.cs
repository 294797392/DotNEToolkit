using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static DotNEToolkit.Win32API;

namespace DotNEToolkit.DirectSound
{
    [ComImport]
    [Guid(InterfaceID.IID_IDirectSoundBuffer8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundBuffer8
    {
        [PreserveSig]
        uint GetCaps(out DSCBCAPS pDSBufferCaps);

        [PreserveSig]
        uint GetCurrentPosition(out uint pdwCurrentPlayCursor, out uint pdwCurrentWriteCursor);

        [PreserveSig]
        uint GetFormat(out WAVEFORMATEX pwfxFormat, uint dwSizeAllocated, out uint pdwSizeWritten);

        [PreserveSig]
        uint GetVolume(out int plVolume);

        [PreserveSig]
        uint GetPan(out int plPan);

        [PreserveSig]
        uint GetFrequency(out uint pdwFrequency);

        [PreserveSig]
        uint GetStatus(out uint pdwStatus);

        [PreserveSig]
        uint Initialize(IntPtr pDirectSound, ref DSBUFFERDESC pcDSBufferDesc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwOffset">Offset, in bytes, from the start of the buffer to the point where the lock begins. This parameter is ignored if DSBLOCK_FROMWRITECURSOR is specified in the dwFlags parameter. </param>
        /// <param name="dwBytes"></param>
        /// <param name="ppvAudioPtr1"></param>
        /// <param name="pdwAudioBytes1"></param>
        /// <param name="ppvAudioPtr2"></param>
        /// <param name="pdwAudioBytes2"></param>
        /// <param name="dwFlags">
        /// DSBLOCK_FROMWRITECURSOR : Start the lock at the write cursor. The dwOffset parameter is ignored.
        /// DSBLOCK_ENTIREBUFFER : Lock the entire buffer. The dwBytes parameter is ignored.
        /// </param>
        /// <returns></returns>
        [PreserveSig]
        uint Lock(uint dwOffset, uint dwBytes, out IntPtr ppvAudioPtr1, out uint pdwAudioBytes1, out IntPtr ppvAudioPtr2, out uint pdwAudioBytes2, uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwReserved1">必须为0</param>
        /// <param name="dwPriority">声音优先级, 当分配硬件混合资源的时候用来管理声音, 最低级别为0, 最高级别0xFFFFFFFF, 如果缓冲区创建的时候没有设置DSBCAPS_LOCDEFER标志, 那么取值必须为0</param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        uint Play(uint dwReserved1, uint dwPriority, uint dwFlags);

        [PreserveSig]
        uint SetCurrentPosition(uint dwNewPosition);

        [PreserveSig]
        uint SetFormat(ref WAVEFORMATEX pcfxFormat);

        [PreserveSig]
        uint SetVolume(int lVolume);

        [PreserveSig]
        uint SetPan(int lPan);

        [PreserveSig]
        uint SetFrequency(uint dwFrequency);

        [PreserveSig]
        uint Stop();

        [PreserveSig]
        uint Unlock(IntPtr pvAudioPtr1, uint dwAudioBytes1, IntPtr pvAudioPtr2, uint dwAudioBytes2);

        [PreserveSig]
        uint Restore();

        [PreserveSig]
        uint SetFX(uint dwEffectsCount, DSEFFECTDESC pDSFXDesc, out uint pdwResultCodes);

        [PreserveSig]
        uint AcquireResources(uint dwFlags, uint dwEffectsCount, out uint pdwResultCodes);

        [PreserveSig]
        uint GetObjectInPath(GUID rguidObject, int dwIndex, GUID rguidInterface, out IntPtr ppObject);
    }
}
