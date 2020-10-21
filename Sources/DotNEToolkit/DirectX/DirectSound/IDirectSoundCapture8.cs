using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNEToolkit.DirectX.DirectSound
{
    [ComImport]
    [Guid(IID.IID_IDirectSoundCapture8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundCapture8
    {
        [PreserveSig]
        int CreateCaptureBuffer(ref DSCBUFFERDESC pcDSCBufferDesc, out IntPtr ppDSCBuffer, IntPtr pUnkOuter);

        /// <summary>
        /// 获取捕获音频设备的信息
        /// </summary>
        /// <param name="pDSCCaps">DSCCAPS结构体指针, 必须指定dwSize字段</param>
        /// <returns></returns>
        [PreserveSig]
        int GetCaps([In, Out]ref DSCCAPS pDSCCaps);

        [PreserveSig]
        int Initialize(IntPtr pcGuidDevice);
    }
}