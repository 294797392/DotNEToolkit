using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Renders drawing instructions to a window.
    /// </summary>
    [Guid("2cd90698-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1HwndRenderTarget
    {
        D2D1_WINDOW_STATE CheckWindowState();

        /// <summary>
        /// Resize the buffer underlying the render target. This operation might fail if
        /// there is insufficient video memory or system memory, or if the render target is
        /// resized beyond the maximum bitmap size. If the method fails, the render target
        /// will be placed in a zombie state and D2DERR_RECREATE_TARGET will be returned
        /// from it when EndDraw is called. In addition an appropriate failure result will
        /// be returned from Resize.
        /// </summary>
        uint Resize(IntPtr pixelSize);

        IntPtr GetHwnd();

        uint Resize(ref D2D1_SIZE_U pixelSize);
    }
}