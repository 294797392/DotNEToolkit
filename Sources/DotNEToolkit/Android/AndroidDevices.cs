using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.Android
{
    /// <summary>
    /// 提供一些静态函数来管理安卓设备
    /// </summary>
    public static class AndroidDevices
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("AndroidDevices");

        private static readonly Win32API.GUID ANDROID_USB_CLASS_ID = new Win32API.GUID()
        {
            Data1 = 0xf72fe0d4,
            Data2 = 0xcbcb,
            Data3 = 0x407d,
            Data4 = new byte[]
            {
                0x88, 0x14, 0x9e, 0xd6, 0x73, 0xd0, 0xdd, 0x6b
            }
        };

        /// <summary>
        /// 枚举当前系统下的所有ADB设备
        /// </summary>
        /// <returns></returns>
        public static List<AndroidDevice> EnumDevices()
        {
            List<AndroidDevice> result = new List<AndroidDevice>();

            //List<AdbWinApi.AdbInterfaceInfo> result = new List<AdbWinApi.AdbInterfaceInfo>();
            IntPtr enumrationHandle = AdbWinApi.AdbEnumInterfaces(ANDROID_USB_CLASS_ID, true, true, true);

            while (true)
            {
                try
                {
                    AdbWinApi.AdbInterfaceInfo info;
                    int size = Marshal.SizeOf(typeof(AdbWinApi.AdbInterfaceInfo));
                    bool rc = AdbWinApi.AdbNextInterface(enumrationHandle, out info, ref size);
                    if (!rc)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        if (lastError == Win32API.ERROR_NO_MORE_ITEMS)
                        {
                            // 没有多余的接口来枚举了
                            break;
                        }
                        else if (lastError == Win32API.ERROR_INSUFFICIENT_BUFFER)
                        {
                            logger.Debug("AdbNextInterface ERROR_INSUFFICIENT_BUFFER");
                            continue;
                        }
                    }

                    // 增加设备
                }
                catch (Exception ex)
                {
                    logger.Error("AdbNextInterface异常", ex);
                }
                finally
                {
                }
            }

            AdbWinApi.AdbCloseHandle(enumrationHandle);

            return result;
        }
    }
}
