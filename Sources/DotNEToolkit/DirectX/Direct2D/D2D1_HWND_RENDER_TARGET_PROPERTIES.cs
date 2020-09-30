using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Contains the HWND, pixel size, and presentation options for an
    /// ID2D1HwndRenderTarget.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_HWND_RENDER_TARGET_PROPERTIES
    {
        public IntPtr hwnd;
        public D2D1_SIZE_U pixelSize;
        public D2D1_PRESENT_OPTIONS presentOptions;
    }
}