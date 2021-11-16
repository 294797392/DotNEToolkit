using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DotNEToolkit.Android
{
    /// <summary>
    /// 表示设备通知事件的处理器
    /// </summary>
    internal interface IDeviceNotificationHandler
    {
        int HandleNotification(System.Windows.Forms.Message m);
    }

    /// <summary>
    /// 当连接了一个新的安卓设备的时候会触发一个回调
    /// </summary>
    public class AndroidDeviceWatcher : ModuleBase, IDeviceNotificationHandler
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("AndroidDeviceWatcher");

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

        #region 公开事件

        /// <summary>
        /// 当一个新的安卓设备连接的时候会触发回调
        /// </summary>
        public event Action<AndroidDeviceWatcher, AndroidDevice> DeviceAttached;

        /// <summary>
        /// 当一个安卓设备被拔掉之后触发
        /// </summary>
        public event Action<AndroidDeviceWatcher, AndroidDevice> DeviceDetached;

        #endregion

        #region 实例变量

        private IntPtr handle;
        private IntPtr filterPtr;

        private WatcherForm watcherForm;

        #endregion

        #region 公开接口

        public override int Initialize(IDictionary parameters)public int HandleNotification(Message m)
        {
            throw new NotImplementedException();
        }

        OK
        {
            base.Initialize(parameters);

            // 接收消息的窗体
            this.watcherForm = new WatcherForm();
            this.watcherForm.Handler = this;
            this.watcherForm.Opacity = 0;
            this.watcherForm.ShowInTaskbar = false;
            this.watcherForm.Show();

            WinUser.DEV_BROADCAST_DEVICEINTERFACE filter = new WinUser.DEV_BROADCAST_DEVICEINTERFACE()
            {
                dbcc_size = Marshal.SizeOf(typeof(WinUser.DEV_BROADCAST_DEVICEINTERFACE)),
                dbcc_devicetype = Dbt.DBT_DEVTYP_DEVICEINTERFACE,
                dbcc_classguid = ANDROID_USB_CLASS_ID
            };

            this.filterPtr = PInvoke.StructureToPtr(filter);
            this.handle = WinUser.RegisterDeviceNotification(this.watcherForm.Handle, this.filterPtr, WinUser.DEVICE_NOTIFY_WINDOW_HANDLE);
            if (this.handle == IntPtr.Zero)
            {
                logger.ErrorFormat("RegisterDeviceNotification失败, Win32Error = {0}", Marshal.GetLastWin32Error());
                PInvoke.FreeStructurePtr(this.filterPtr);
                this.watcherForm.Close();
                return DotNETCode.SYS_ERROR;
            }

            logger.InfoFormat("RegisterDeviceNotification成功");

            return DotNETCode.SUCCESS;
        }

        public override void Release()
        {
            if (this.filterPtr != IntPtr.Zero)
            {
                PInvoke.FreeStructurePtr(this.filterPtr);
                this.filterPtr = IntPtr.Zero;
            }

            if (this.handle != IntPtr.Zero)
            {
                WinUser.UnregisterDeviceNotification(this.handle);
                this.handle = IntPtr.Zero;
            }

            base.Release();
        }

        #endregion

        #region IDeviceNotificationHandler

        public int HandleNotification(System.Windows.Forms.Message m)
        {
            return 0;
        }

        #endregion

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
