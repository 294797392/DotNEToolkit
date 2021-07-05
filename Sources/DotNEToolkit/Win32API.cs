using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
    }

    public static class Win32API
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }

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

        public delegate bool WNDENUMPROC(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, WNDENUMPROC lpEnumProc, int lParam);

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

        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

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
}
