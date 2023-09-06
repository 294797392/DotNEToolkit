using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static DotNEToolkit.Shell32;

// 所有的Win32API用Dll名字给类命名

namespace DotNEToolkit
{
    public static class Win32APIHelper
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("Win32APIHelper");

        /// <summary>
        /// 获取某个窗口中，某个位置的控件上的文本
        /// </summary>
        /// <param name="hWnd">要获取的窗口的句柄</param>
        /// <param name="x">要获取文本的控件的x坐标</param>
        /// <param name="y">要获取文本的控件的y坐标</param>
        /// <returns>获取到的文本</returns>
        public static string GetWindowText(IntPtr hWnd, int x, int y)
        {
            Win32API.POINT point = new Win32API.POINT()
            {
                x = x,
                y = y
            };

            IntPtr handle = Win32API.ChildWindowFromPoint(hWnd, point);
            if (handle == IntPtr.Zero)
            {
                logger.ErrorFormat("ChildWindowFromPoint失败, {0}", Marshal.GetLastWin32Error());
                return null;
            }

            int size = Win32API.SendMessage(handle, Win32API.WM_GETTEXTLENGTH, 0, 0);
            if (size == 0)
            {
                return string.Empty;
            }

            char[] title = new char[size];
            SendMessage(handle, Win32API.WM_GETTEXT, size, Marshal.UnsafeAddrOfPinnedArrayElement(title, 0));
            return new string(title);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int bufSize, IntPtr buf);

        /// <summary>
        /// 显示文件或者目录的属性对话框
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }
    }

    public static class Iphlpapi
    {
        private const string DllName = "Iphlpapi.dll";

        private const int TCPIP_OWNING_MODULE_SIZE = 16;

        public const int MIB_TCP_STATE_CLOSED = 1;
        public const int MIB_TCP_STATE_LISTEN = 2;
        public const int MIB_TCP_STATE_SYN_SENT = 3;
        public const int MIB_TCP_STATE_SYN_RCVD = 4;
        public const int MIB_TCP_STATE_ESTAB = 5;
        public const int MIB_TCP_STATE_FIN_WAIT1 = 6;
        public const int MIB_TCP_STATE_FIN_WAIT2 = 7;
        public const int MIB_TCP_STATE_CLOSE_WAIT = 8;
        public const int MIB_TCP_STATE_CLOSING = 9;
        public const int MIB_TCP_STATE_LAST_ACK = 10;
        public const int MIB_TCP_STATE_TIME_WAIT = 11;
        public const int MIB_TCP_STATE_DELETE_TCB = 12;

        public enum TCP_TABLE_CLASS
        {
            /// <summary>
            /// MIB_TCPTABLE
            /// </summary>
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_LISTENER,

            /// <summary>
            /// MIB_TCPTABLE_OWNER_MODULE
            /// </summary>
            TCP_TABLE_OWNER_MODULE_ALL,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_LISTENER,

            /// <summary>
            /// MIB_TCPTABLE_OWNER_PID
            /// </summary>
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_LISTENER
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_MODULE
        {
            public int dwState;
            public int dwLocalAddr;
            public int dwLocalPort;
            public int dwRemoteAddr;
            public int dwRemotePort;
            public int dwOwningPid;
            public long liCreateTimestamp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = TCPIP_OWNING_MODULE_SIZE)]
            public long[] OwningModuleInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_MODULE
        {
            /// <summary>
            /// The number of MIB_TCPROW_OWNER_MODULE elements in the table.
            /// </summary>
            public int dwNumEntries;

            /// <summary>
            /// Array of MIB_TCPROW_OWNER_MODULE structures returned by a call to GetExtendedTcpTable.
            /// </summary>
            public IntPtr table;
        }

        [DllImport(DllName)]
        public static extern int GetExtendedTcpTable(IntPtr pTcpTable, out int pdwSize, bool bOrder, int ulAf, TCP_TABLE_CLASS TableClass, int Reserved);

        //[DllImport(DllName)]
        //public static extern int GetExtendedUdpTable(out IntPtr pUdpTable, ref int pdwSize, bool bOrder, int ulAf, );
    }

    public static class Kernel32
    {
        private const string DllName = "kernel32.dll";

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        /// <summary>
        /// 加载动态链接库
        /// </summary>
        /// <param name="dllToLoad">dll文件名</param>
        /// <returns>dll模块指针</returns>
        [DllImport(DllName, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        /// <summary>
        /// 获取函数指针
        /// </summary>
        /// <param name="hModule">dll模块指针</param>
        /// <param name="procedureName">方法名</param>
        /// <returns>函数指针</returns>
        [DllImport(DllName, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        /// <summary>
        /// 释放动态链接库
        /// </summary>
        /// <param name="hModule">dll模块指针</param>
        /// <returns>释放释放成功</returns>
        [DllImport(DllName, SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(DllName, SetLastError = true)]
        public static extern IntPtr CreateFileMapping();

        #region 控制台函数

        [DllImport(DllName)]
        public static extern IntPtr CreateConsoleScreenBuffer(int dwDesiredAccess, int dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, int dwFlags, IntPtr lpScreenBufferData);

        //[DllImport(DllName)]
        //public static extern 

        #endregion

        [DllImport(DllName)]
        public static extern bool CloseHandle(IntPtr hObject);
    }

    /// <summary>
    /// 返回值定义
    /// </summary>
    internal static class MMSYSERR
    {
        public const int MMSYSERR_NOERROR = 0;                   /* no error */
        public const int MMSYSERR_ERROR = 1;  /* unspecified error */
        public const int MMSYSERR_BADDEVICEID = 2;  /* device ID out of range */
        public const int MMSYSERR_NOTENABLED = 3;  /* driver failed enable */
        public const int MMSYSERR_ALLOCATED = 4;  /* device already allocated */
        public const int MMSYSERR_INVALHANDLE = 5;  /* device handle is invalid */
        public const int MMSYSERR_NODRIVER = 6;  /* no device driver present */
        public const int MMSYSERR_NOMEM = 7;  /* memory allocation error */
        public const int MMSYSERR_NOTSUPPORTED = 8;  /* function isn't supported */
        public const int MMSYSERR_BADERRNUM = 9;  /* error value out of range */
        public const int MMSYSERR_INVALFLAG = 10; /* invalid flag passed */
        public const int MMSYSERR_INVALPARAM = 11; /* invalid parameter passed */
        public const int MMSYSERR_HANDLEBUSY = 12; /* handle being used */
        /* simultaneously on another */
        /* thread (eg callback; */
        public const int MMSYSERR_INVALIDALIAS = 13; /* specified alias not found */
        public const int MMSYSERR_BADDB = 14; /* bad registry database */
        public const int MMSYSERR_KEYNOTFOUND = 15; /* registry key not found */
        public const int MMSYSERR_READERROR = 16; /* registry read error */
        public const int MMSYSERR_WRITEERROR = 17; /* registry write error */
        public const int MMSYSERR_DELETEERROR = 18; /* registry delete error */
        public const int MMSYSERR_VALNOTFOUND = 19; /* registry value not found */
        public const int MMSYSERR_NODRIVERCB = 20; /* driver does not call DriverCallback */
        public const int MMSYSERR_MOREDATA = 21; /* more data to be returned */
        public const int MMSYSERR_LASTERROR = 21; /* last error in range */
    }

    public static class waveIn
    {
        private const string waveDll = "Winmm.dll";

        [StructLayout(LayoutKind.Sequential)]
        public struct wavehdr_tag
        {
            /// <summary>
            /// Pointer to the waveform buffer.
            /// </summary>
            public IntPtr lpData;               /* pointer to locked data buffer */

            /// <summary>
            /// Length, in bytes, of the buffer.
            /// </summary>
            public uint dwBufferLength;         /* length of data buffer */

            /// <summary>
            /// When the header is used in input, specifies how much data is in the buffer.
            /// </summary>
            public uint dwBytesRecorded;        /* used for input only */

            /// <summary>
            /// User data.
            /// </summary>
            public uint dwUser;                 /* for client's use */
            public uint dwFlags;                /* assorted flags (see defines) */
            public uint dwLoops;                /* loop control counter */
            public IntPtr lpNext;               /* reserved for driver */
            public uint reserved;               /* reserved for driver */
        }

        #region 枚举

        public enum uMsgEnum : uint
        {
            /// <summary>
            /// Sent when the device is closed using the waveInClose function.
            /// </summary>
            WIM_CLOSE = 0x3BF,

            /// <summary>
            /// Sent when the device driver is finished with a data block sent using the waveInAddBuffer function.
            /// </summary>
            WIM_DATA = 0x3C0,

            /// <summary>
            /// Sent when the device is opened using the waveInOpen function.
            /// </summary>
            WIM_OPEN = 0x3BE
        }

        #endregion

        #region DeviceID

        public const uint WAVE_MAPPER = 0xFFFFFFFF;

        #endregion

        #region fdwOpen选项

        /// <summary>
        /// The dwCallback parameter is an event handle.
        /// </summary>
        public const int CALLBACK_EVENT = 0x00050000;

        /// <summary>
        /// The dwCallback parameter is a callback procedure address.
        /// </summary>
        public const int CALLBACK_FUNCTION = 0x00030000;

        /// <summary>
        /// No callback mechanism. This is the default setting.
        /// </summary>
        public const int CALLBACK_NULL = 0x00000000;

        /// <summary>
        /// The dwCallback parameter is a thread identifier.
        /// </summary>
        public const int CALLBACK_THREAD = 0x00020000;

        /// <summary>
        /// The dwCallback parameter is a window handle.
        /// </summary>
        public const int CALLBACK_WINDOW = 0x00010000;

        /// <summary>
        /// If this flag is specified, the ACM driver does not perform conversions on the audio data.
        /// </summary>
        public const int WAVE_FORMAT_DIRECT = 0x0008;

        /// <summary>
        /// The function queries the device to determine whether it supports the given format, but it does not open the device.
        /// </summary>
        public const int WAVE_FORMAT_QUERY = 0x0001;

        /// <summary>
        /// The uDeviceID parameter specifies a waveform-audio device to be mapped to by the wave mapper.
        /// </summary>
        public const int WAVE_MAPPED = 0x0004;

        #endregion

        #region waveIn函数

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwi"></param>
        /// <param name="uMsg"></param>
        /// <param name="dwInstance"></param>
        /// <param name="dwParam1">wavehdr_tag指针</param>
        /// <param name="dwParam2"></param>
        public delegate void waveInProcDlg(IntPtr hwi, uint uMsg, uint dwInstance, uint dwParam1, uint dwParam2);

        /// <summary>
        /// 获取音频输入设备的数量
        /// </summary>
        /// <returns></returns>
        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInGetNumDevs();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phwi">用于返回设备句柄的指针; 如果 dwFlags=WAVE_FORMAT_QUERY, 这里应是 NULL</param>
        /// <param name="uDeviceID">设备ID; 可以指定为: WAVE_MAPPER, 这样函数会根据给定的波形格式选择合适的设备</param>
        /// <param name="pwfx">要申请的声音格式</param>
        /// <param name="dwCallback">回调函数地址或窗口句柄; 若不使用回调机制, 设为 NULL</param>
        /// <param name="dwInstance">给回调函数的实例数据; 不用于窗口</param>
        /// <param name="fdwOpen">打开选项</param>
        /// <returns></returns>
        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInOpen([Out] out IntPtr phwi, [In] uint uDeviceID, [In] IntPtr pwfx, [In] waveInProcDlg dwCallback, [In] int dwInstance, [In] int fdwOpen);

        /// <summary>
        /// The waveInClose function closes the given waveform-audio input device.
        /// </summary>
        /// <param name="phwi"></param>
        /// <remarks>
        /// If there are input buffers that have been sent with the waveInAddBuffer function and that haven't been returned to the application, the close operation will fail. Call the waveInReset function to mark all pending buffers as done.
        /// 为了确保关闭成功, 要在调用waveInClose之前先调用waveInReset
        /// </remarks>
        /// <returns></returns>
        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInClose([In] IntPtr phwi);

        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInStart([In] IntPtr hwi);

        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInStop([In] IntPtr hwi);

        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInReset([In] IntPtr hwi);

        /// <summary>
        /// The waveInPrepareHeader function prepares a buffer for waveform-audio input.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <param name="pwh">WAVEHDR（wavehdr_tag）结构体指针</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <remarks>
        /// The lpData, dwBufferLength, and dwFlags members of the WAVEHDR structure must be set before calling this function (dwFlags must be zero).
        /// </remarks>
        /// <returns></returns>
        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInPrepareHeader([In] IntPtr hwi, [In] IntPtr pwh, uint cbwh);

        /// <summary>
        /// The waveInAddBuffer function sends an input buffer to the given waveform-audio input device. When the buffer is filled, the application is notified.
        /// 把准备好的缓冲区送给硬件
        /// </summary>
        /// <param name="hwi"></param>
        /// <param name="pwh">waveHeader指针</param>
        /// <param name="cbwh"></param>
        /// <remarks>
        /// When the buffer is filled, the WHDR_DONE bit is set in the dwFlags member of the WAVEHDR structure.
        /// The buffer must be prepared with the waveInPrepareHeader function before it is passed to this function.
        /// 在调用waveInAddBuffer之前必须调用waveInPrepareHeader函数
        /// </remarks>
        /// <returns></returns>
        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInAddBuffer([In] IntPtr hwi, [In] IntPtr pwh, uint cbwh);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwi"></param>
        /// <param name="pwh">waveHeader指针</param>
        /// <param name="cbwh"></param>
        /// <returns></returns>
        [DllImport(waveDll, CallingConvention = CallingConvention.StdCall)]
        public static extern int waveInUnprepareHeader([In] IntPtr hwi, [In] IntPtr pwh, uint cbwh);

        #endregion
    }

    public static class Win32API
    {
        public static class Kernel32
        {
            private const string Kernel32Dll = "kernel32.dll";

            [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr LoadLibrary(string lpLibFileName);

            [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern bool FreeLibrary(IntPtr hModule);
        }

        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int ERROR_NO_MORE_ITEMS = 259;

        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }

        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED_0 = 0x00000080;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        /// <summary>
        /// 等待信号失败
        /// </summary>
        public const uint WAIT_FAILED = 0xFFFFFFFF;

        /// <summary>
        /// flags for wFormatTag field of WAVEFORMAT
        /// </summary>
        public const int WAVE_FORMAT_PCM = 1;

        public const int CBR_110 = 110;
        public const int CBR_300 = 300;
        public const int CBR_600 = 600;
        public const int CBR_1200 = 1200;
        public const int CBR_2400 = 2400;
        public const int CBR_4800 = 4800;
        public const int CBR_9600 = 9600;
        public const int CBR_14400 = 14400;
        public const int CBR_19200 = 19200;
        public const int CBR_38400 = 38400;
        public const int CBR_56000 = 56000;
        public const int CBR_57600 = 57600;
        public const int CBR_115200 = 115200;
        public const int CBR_128000 = 128000;
        public const int CBR_256000 = 256000;

        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("ws2_32.dll")]
        public static extern int inet_addr(string ip);

        [DllImport("ntdll.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr source, int length);

        #region User32

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        //SendMessage参数
        public const int WM_KEYDOWN = 0X100;
        public const int WM_KEYUP = 0X101;
        public const int WM_SYSCHAR = 0X106;
        public const int WM_SYSKEYUP = 0X105;
        public const int WM_SYSKEYDOWN = 0X104;
        public const int WM_CHAR = 0X102;
        public const int WM_COMMAND = 0x0111;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;

        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;

        #region WM_APPCOMMAND

        /// <summary>
        /// https://docs.microsoft.com/zh-cn/windows/win32/inputdev/wm-appcommand?redirectedfrom=MSDN
        /// 使用WM_APPCOMMAND消息可以实现键盘上的多媒体按键（比如音量控制）
        /// </summary>
        public const int WM_APPCOMMAND = 0x319;


        // 音量控制
        public const int APPCOMMAND_VOLUME_UP = 0x0A;
        public const int APPCOMMAND_VOLUME_DOWN = 0x09;
        public const int APPCOMMAND_VOLUME_MUTE = 0x08;


        // 控制浏览器
        public const int APPCOMMAND_BROWSER_BACKWARD = 1;
        public const int APPCOMMAND_BROWSER_FAVORITES = 6;
        public const int APPCOMMAND_BROWSER_FORWARD = 2;
        public const int APPCOMMAND_BROWSER_HOME = 7;
        public const int APPCOMMAND_BROWSER_REFRESH = 3;
        public const int APPCOMMAND_BROWSER_SEARCH = 5;
        public const int APPCOMMAND_BROWSER_STOP = 6;

        /// <summary>
        /// Close the window (not the application).
        /// </summary>
        public const int APPCOMMAND_CLOSE = 31;
        public const int APPCOMMAND_COPY = 32;
        public const int APPCOMMAND_CUT = 37;
        public const int APPCOMMAND_PASTE = 38;

        public const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 44;
        /// <summary>
        /// Decrease microphone volume.
        /// </summary>
        public const int APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25;
        /// <summary>
        /// Mute the microphone.
        /// </summary>
        public const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24;
        public const int APPCOMMAND_MICROPHONE_VOLUME_UP = 26;

        #endregion


        [DllImport("user32", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("User32.dll")]
        public static extern bool GetCursorPos(out POINT p);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndlnsertAfter, int X, int Y, int cx, int cy, uint Flags);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern long GetWindowThreadProcessId(long hWnd, long lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        public static extern long GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hwnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 立即刷新某个窗口
        /// 会导致窗口重绘
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UpdateWindow(IntPtr hWnd);

        /// <summary>
        /// 返回父窗口中包含了指定点的第一个子窗口的句柄
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPoint(IntPtr hWndParent, POINT point);

        /// <summary>
        /// 返回指定坐标处的窗口句柄
        /// </summary>
        /// <param name="Point"></param>
        /// <returns>如果找到了窗口，则返回窗口句柄，否则返回空句柄</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        #endregion

        #region shell32

        public const int SW_HIDE = 0; //{隐藏, 并且任务栏也没有最小化图标}
        public const int SW_SHOWNORMAL = 1; //{用最近的大小和位置显示, 激活}
        public const int SW_NORMAL = 1; //{同 SW_SHOWNORMAL}
        public const int SW_SHOWMINIMIZED = 2; //{最小化, 激活}
        public const int SW_SHOWMAXIMIZED = 3; //{最大化, 激活}
        public const int SW_MAXIMIZE = 3; //{同 SW_SHOWMAXIMIZED}
        public const int SW_SHOWNOACTIVATE = 4; //{用最近的大小和位置显示, 不激活}
        public const int SW_SHOW = 5; //{同 SW_SHOWNORMAL}
        public const int SW_MINIMIZE = 6; //{最小化, 不激活}
        public const int SW_SHOWMINNOACTIVE = 7; //{同 SW_MINIMIZE}
        public const int SW_SHOWNA = 8; //{同 SW_SHOWNOACTIVATE}
        public const int SW_RESTORE = 9; //{同 SW_SHOWNORMAL}
        public const int SW_SHOWDEFAULT = 10; //{同 SW_SHOWNORMAL}
        public const int SW_MAX = 10; //{同 SW_SHOWNORMAL}

        /// <summary>
        /// ShellExecute(IntPtr.Zero, "Open", "C:/Program Files/TTPlayer/TTPlayer.exe", "", "", 1);
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lpOperation"></param>
        /// <param name="lpFile"></param>
        /// <param name="lpParameters"></param>
        /// <param name="lpDirectory"></param>
        /// <param name="nShowCmd"></param>
        /// <returns></returns>
        [DllImport("shell32.dll", EntryPoint = "ShellExecute")]
        public static extern int ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        #endregion

        #region DirectSound

        private const string dsoundDll = "dsound.dll";

        public const uint DSBPN_OFFSETSTOP = 0xFFFFFFFF;
        public const int DSCBSTART_LOOPING = 0x00000001;

        /// <summary>
        /// 创建一个DirectSoundCapture8接口
        /// </summary>
        /// <param name="pcGuidDevice"></param>
        /// <param name="ppDSC8"></param>
        /// <param name="pUnkOuter"></param>
        /// <returns></returns>
        [DllImport(dsoundDll, CallingConvention = CallingConvention.StdCall)]
        public static extern uint DirectSoundCaptureCreate8(IntPtr pcGuidDevice, out IntPtr ppDSC8, IntPtr pUnkOuter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpcGuidDevice">
        /// Address of the GUID that identifies the sound device
        /// DSDEVID_DefaultPlayback : System-wide default audio playback device. Equivalent to NULL. 
        /// DSDEVID_DefaultVoicePlayback : Default voice playback device. 
        /// </param>
        /// <param name="ppDS8">Address of a variable to receive an IDirectSound8 interface pointer. </param>
        /// <param name="pUnkOuter"></param>
        /// <returns></returns>
        /// <remarks>
        /// 在创建IDirectSound8接口之后必须首先调用SetCooperativeLevel
        /// </remarks>
        [DllImport(dsoundDll, CallingConvention = CallingConvention.StdCall)]
        public static extern uint DirectSoundCreate8(IntPtr lpcGuidDevice, out IntPtr ppDS8, IntPtr pUnkOuter);

        /// <summary>
        /// 注意：第一个被枚举出来的设备永远都是Primary Sound Driver（主声音捕获设备），所以一般情况下都会忽略第一个设备。主声音捕获设备的意思就是当前用户选择的录音设备。
        /// The first device enumerated is always called the Primary Sound Driver, and the lpGUID parameter of the callback is NULL. This device represents the preferred playback device set by the user in Control Panel
        /// </summary>
        /// <param name="lpGuid">Address of the GUID that identifies the device being enumerated, or NULL for the primary device. This value can be passed to the DirectSoundCreate8 or DirectSoundCaptureCreate8 function to create a device object for that driver.</param>
        /// <param name="lpcstrDescription">Address of a null-terminated string that provides a textual description of the DirectSound device.</param>
        /// <param name="lpcstrModule">Address of a null-terminated string that specifies the module name of the DirectSound driver corresponding to this device.</param>
        /// <param name="lpContext">Address of application-defined data. This is the pointer passed to DirectSoundEnumerate or DirectSoundCaptureEnumerate as the lpContext parameter.</param>
        /// <returns>Returns TRUE to continue enumerating drivers, or FALSE to stop.</returns>
        public delegate bool DSEnumCallback(IntPtr lpGuid, string lpcstrDescription, string lpcstrModule, object lpContext);

        /// <summary>
        /// DirectSound枚举声音捕获设备的接口
        /// </summary>
        /// <param name="lpDSEnumCallback">枚举回调</param>
        /// <param name="lpContext">上下文信息</param>
        /// <returns></returns>
        [DllImport(dsoundDll, CallingConvention = CallingConvention.StdCall)]
        public static extern uint DirectSoundCaptureEnumerate(DSEnumCallback lpDSEnumCallback, object lpContext);

        #endregion

        #region Kernel32

        private const string Kernel32Dll = "kernel32.dll";

        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string lpName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCount">指定列表中的句柄数量  最大值为MAXIMUM_WAIT_OBJECTS（64）</param>
        /// <param name="lpHandles">柄数组的指针。lpHandles为指定对象句柄组合中的第一个元素 HANDLE类型可以为（Event，Mutex，Process，Thread，Semaphore）数组</param>
        /// <param name="bWaitAll">如果为TRUE，表示除非对象都发出信号，否则就一直等待下去；如果FALSE，表示任何对象发出信号即可</param>
        /// <param name="dwMilliseconds">指定要等候的毫秒数。如设为零，表示立即返回。如指定常数INFINITE，则可根据实际情况无限等待下去</param>
        /// <returns>
        /// WAIT_ABANDONED_0：所有对象都发出消息，而且其中有一个或多个属于互斥体（一旦拥有它们的进程中止，就会发出信号）
        /// WAIT_TIMEOUT：对象保持未发信号的状态，但规定的等待超时时间已经超过
        /// WAIT_OBJECT_0：所有对象都发出信号，WAIT_OBJECT_0是微软定义的一个宏，你就把它看成一个数字就可以了。例如，WAIT_OBJECT_0 + 5的返回结果意味着列表中的第5个对象发出了信号
        /// WAIT_IO_COMPLETION：（仅适用于WaitForMultipleObjectsEx）由于一个I/O完成操作已作好准备执行，所以造成了函数的返回
        /// 返回WAIT_FAILED则表示函数执行失败，会设置GetLastError
        /// </returns>
        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        public static extern uint WaitForMultipleObjects(int nCount, IntPtr lpHandles, [MarshalAs(UnmanagedType.Bool)] bool bWaitAll, uint dwMilliseconds);

        /// <summary>
        /// 等待信号量
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="dwMilliseconds"></param>
        /// <returns></returns>
        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        public static extern uint WaitForSingleObject(IntPtr evt, uint dwMilliseconds);

        /// <summary>
        /// 重置信号量为无信号状态
        /// </summary>
        /// <param name="hEvent"></param>
        /// <returns></returns>
        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ResetEvent(IntPtr hEvent);

        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport(Kernel32Dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        #endregion

        public const int SWP_NOOWNERZORDER = 0x200;
        public const int SWP_NOREDRAW = 0x8;
        public const int SWP_NOZORDER = 0x4;
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int WS_EX_MDICHILD = 0x40;
        public const int SWP_FRAMECHANGED = 0x20;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int SWP_NOMOVE = 0x2;
        public const int SWP_NOSIZE = 0x1;
        public const int GWL_STYLE = (-16);
        public const int WS_VISIBLE = 0x10000000;
        public const int WM_CLOSE = 0x10;
        public const int WS_CHILD = 0x40000000;

        public const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_VM_WRITE = 0x0020;

        #region 读写INI文件

        /// <summary>
        /// Ini文件读取
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="retVal"></param>
        /// <param name="size"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// Ini文件写入
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        #endregion

        public const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const int TOKEN_DUPLICATE = 0x0002;
        public const int TOKEN_IMPERSONATE = 0x0004;
        public const int TOKEN_QUERY = 0x0008;
        public const int TOKEN_QUERY_SOURCE = 0x0010;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const int TOKEN_ADJUST_GROUPS = 0x0040;
        public const int TOKEN_ADJUST_DEFAULT = 0x0080;
        public const int TOKEN_ADJUST_SESSIONID = 0x0100;

        [DllImport("Advapi32.dll")]
        public static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

        //[DllImport("Advapi32.dll")]
        //public static extern int CreateProcessAsUser(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);
    }

    /// <summary>
    /// WinUser.h
    /// </summary>
    public static class User32
    {
        private const string User32Dll = "User32.dll";

        #region 委托

        public delegate bool WNDENUMPROC(IntPtr hwnd, int lParam);

        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Win32API.GUID dbcc_classguid;
            public IntPtr dbcc_name;
        }

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004;

        public const int WM_DEVICECHANGE = 0x0219;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hRecipient"></param>
        /// <param name="NotificationFilter"></param>
        /// <param name="Flags">
        /// DEVICE_NOTIFY_ALL_INTERFACE_CLASSES：Notifies the recipient of device interface events for all device interface classes. (The dbcc_classguid member is ignored.)This value can be used only if the dbch_devicetype member is DBT_DEVTYP_DEVICEINTERFACE.
        /// </param>
        /// <returns></returns>
        [DllImport(User32Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);

        /// <summary>
        /// Closes the specified device notification handle.
        /// </summary>
        /// <param name="Handle">Device notification handle returned by the RegisterDeviceNotification function.</param>
        /// <returns></returns>
        [DllImport(User32Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UnregisterDeviceNotification(IntPtr Handle);

        [DllImport(User32Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableWindow(IntPtr hwnd, bool enable);

        [DllImport(User32Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, WNDENUMPROC lpEnumProc, int lParam);

        [DllImport(User32Dll)]
        public static extern uint SendInput();

        [DllImport(User32Dll)]
        public static extern ushort GetKeyState();

        [DllImport(User32Dll)]
        public static extern short GetAsyncKeyState();
    }

    /// <summary>
    /// Dbt.h
    /// </summary>
    public static class Dbt
    {
        public const int DBT_DEVTYP_OEM = 0x00000000;
        public const int DBT_DEVTYP_DEVNODE = 0x00000001;
        public const int DBT_DEVTYP_VOLUME = 0x00000002;
        public const int DBT_DEVTYP_PORT = 0x00000003;
        public const int DBT_DEVTYP_NET = 0x00000004;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        public const int DBT_DEVTYP_HANDLE = 0x00000006;

        public const int DBT_DEVICEARRIVAL = 0x8000;  // system detected a new device
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;  // wants to remove, may fail
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  // removal aborted
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;  // about to remove, still avail.
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  // device is gone
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;  // type specific event

        public const int DBT_DEVNODES_CHANGED = 0x0007;
    }

    /// <summary>
    /// SetupAPI.h
    /// </summary>
    public static class SetupAPI
    {
        private const string SetupAPIDll = "SetupAPI.dll";

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Win32API.GUID ClassGuid;
            public int DevInst;
            public int Reserved;
        }

        [DllImport(SetupAPIDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetupDiGetClassDevs([In] Guid ClassGuid, [In] string Enumerator, [In] IntPtr hwndParent, int Flags);
    }

    /// <summary>
    /// imm.h
    /// </summary>
    public static class imm
    {
        //private const string 

        //[DllImport(]
        //public static extern bool ImmSetCompositionWindow(IntPtr);
    }

    /// <summary>
    /// COM类型ID
    /// </summary>
    public static class CLSID
    {
        public const string CLSID_DirectSoundCapture8 = "E4BCAC13-7F99-4908-9A8E-74E3BF24B6E1";
    }

    /// <summary>
    /// COM接口ID
    /// </summary>
    public static class InterfaceID
    {
        public const string IID_IDirectSound8 = "C50A7E93-F395-4834-9EF6-7FA99DE50966";

        public const string IID_IDirectSoundCapture8 = "b0210781-89cd-11d0-af08-00a0c925cd16";

        public const string IID_IDirectSoundBuffer8 = "6825a449-7524-4d82-920f-50e36ab3ab1e";

        public const string IID_IDirectSoundCaptureBuffer8 = "00990df4-0dbb-4872-833e-6d303e80aeb6";

        public const string IID_IDirectSoundCaptureBuffer = "b0210782-89cd-11d0-af08-00a0c925cd16";

        public const string IID_IDirectSoundNotify8 = "b0210783-89cd-11d0-af08-00a0c925cd16";
    }

    public static class msvcrt
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr to, IntPtr from, long size);
    }

    public static class Ole32
    {
        public enum tagCOINIT
        {
            COINIT_APARTMENTTHREADED = 0x2,
            COINIT_MULTITHREADED,
            COINIT_DISABLE_OLE1DDE = 0x4,
            COINIT_SPEED_OVER_MEMORY = 0x8
        }

        [DllImport("Ole32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int CoInitializeEx(IntPtr pvReserved, tagCOINIT dwCoInit);
    }

    public static class Shell32
    {
        public const int SW_SHOW = 5;
        public const uint SEE_MASK_INVOKEIDLIST = 12;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
    }
}
