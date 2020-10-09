using DotNEToolkit.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Direct2D中的接口ID
    /// </summary>
    public static class D2DInterfaceID
    {
        public const string ID2D1Factory = "{06152247-6F50-465A-9245-118BFD3B6007}";
    }

    /// <summary>
    /// D2D类
    /// </summary>
    public static class Direct2DNatives
    {
        private const string Direct2DllName = "D2d1.dll";

        [DllImport(Direct2DllName, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
        public static extern int D2D1CreateFactory(D2D1_FACTORY_TYPE factoryType, GUID riid, IntPtr pFactoryOptions, out IntPtr factory);
    }
}