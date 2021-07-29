using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit.Android
{
    /// <summary>
    /// 包装ADB API
    /// </summary>
    public static class AdbWinApi
    {
        /// <summary>
        /// Provides information about an interface.
        /// </summary>
        public struct AdbInterfaceInfo
        {
            /// <summary>
            /// Inteface's class id (see SP_DEVICE_INTERFACE_DATA for details)
            /// </summary>
            public Win32API.GUID class_id;

            /// <summary>
            /// Interface flags (see SP_DEVICE_INTERFACE_DATA for details)
            /// </summary>
            public int flags;

            /// <summary>
            /// Device name for the interface (see SP_DEVICE_INTERFACE_DETAIL_DATA for details)
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16384)]
            public byte[] device_name;
        }

        private const string DLLNAME = "AdbWinApi.dll";

        [DllImport(DLLNAME)]
        public static extern IntPtr AdbEnumInterfaces(Win32API.GUID class_id, bool exclude_not_present, bool exclude_removed, bool active_only);

        [DllImport(DLLNAME)]
        public static extern bool AdbNextInterface(IntPtr adb_handle, out AdbInterfaceInfo info, ref int size);

        [DllImport(DLLNAME)]
        public static extern bool AdbResetInterfaceEnum(IntPtr adb_handle);

        [DllImport(DLLNAME)]
        public static extern IntPtr AdbCreateInterfaceByName(string interface_name);

        [DllImport(DLLNAME)]
        public static extern bool AdbGetSerialNumber(IntPtr adb_interface, IntPtr buffer, int buffer_char_size, bool ansi);

        [DllImport(DLLNAME)]
        public static extern bool AdbCloseHandle(IntPtr adb_handle);
    }
}
