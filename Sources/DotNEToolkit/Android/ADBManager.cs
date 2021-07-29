using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DotNEToolkit.Android
{
    public class ADBManager : SingletonObject<ADBManager>
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ADBManager");

        private const int MonitorInterval = 500;

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

        #endregion

        #region 实例变量

        private bool isRunning;
        private Thread monitorThread;

        private IntPtr enumrationHandle;

        #endregion

        #region 公开接口

        public void Start()
        {
            if (this.monitorThread != null)
            {
                return;
            }

            this.isRunning = true;
            this.monitorThread = new Thread(this.DeviceMonitorThreadProc);
            this.monitorThread.IsBackground = true;
            this.monitorThread.Start();
        }

        public void Stop()
        {
            if (this.monitorThread == null)
            {
                return;
            }

            this.isRunning = false;
            this.monitorThread.Abort();
            this.monitorThread.Join();
            this.monitorThread = null;
        }

        public List<AdbWinApi.AdbInterfaceInfo> EnumDevices()
        {
            List<AdbWinApi.AdbInterfaceInfo> result = new List<AdbWinApi.AdbInterfaceInfo>();
            this.enumrationHandle = AdbWinApi.AdbEnumInterfaces(ANDROID_USB_CLASS_ID, true, true, true);

            while (this.isRunning)
            {
                try
                {
                    AdbWinApi.AdbInterfaceInfo info;
                    int size = Marshal.SizeOf(typeof(AdbWinApi.AdbInterfaceInfo));
                    bool rc = AdbWinApi.AdbNextInterface(this.enumrationHandle, out info, ref size);
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
                            logger.Error("AdbNextInterface ERROR_INSUFFICIENT_BUFFER");
                            continue;
                        }
                    }

                    result.Add(info);
                }
                catch (Exception ex)
                {
                    logger.Error("AdbNextInterface异常", ex);
                }
                finally
                {
                }
            }

            AdbWinApi.AdbCloseHandle(this.enumrationHandle);

            return result;
        }

        #endregion

        #region 实例方法

        private void DeviceMonitorThreadProc()
        {
        }

        #endregion
    }
}




