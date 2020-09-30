using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D类
    /// </summary>
    public static class D2D
    {
        private const string Direct2DllName = "D2d1.dll";

        [DllImport(Direct2DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern uint D2D1CreateFactory(D2D1_FACTORY_TYPE factoryType, out IntPtr factory);
    }
}