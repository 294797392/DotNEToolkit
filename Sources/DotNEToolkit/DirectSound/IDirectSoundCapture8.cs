using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    [ComImport]
    [Guid(InterfaceID.IID_IDirectSoundCapture8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundCapture8
    {
        [PreserveSig]
        uint CreateCaptureBuffer(ref DSCBUFFERDESC pcDSCBufferDesc, out IntPtr ppDSCBuffer, IntPtr pUnkOuter);

        /// <summary>
        /// 获取捕获音频设备的信息
        /// </summary>
        /// <param name="pDSCCaps">DSCCAPS结构体指针, 必须指定dwSize字段</param>
        /// <returns></returns>
        [PreserveSig]
        uint GetCaps([In, Out] ref DSCCAPS pDSCCaps);

        [PreserveSig]
        uint Initialize(IntPtr pcGuidDevice);
    }

    [ComImport]
    [Guid(CLSID.CLSID_DirectSoundCapture8)]
    public class DirectSoundCapture8
    {

    }
}
