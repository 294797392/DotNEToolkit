using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_RENDER_TARGET_USAGE类
    /// </summary>
    public enum D2D1_RENDER_TARGET_USAGE : uint
    {
        D2D1_RENDER_TARGET_USAGE_NONE = 0x00000000,

        /// <summary>
        /// Rendering will occur locally, if a terminal-services session is established, the
        /// bitmap updates will be sent to the terminal services client.
        /// </summary>
        D2D1_RENDER_TARGET_USAGE_FORCE_BITMAP_REMOTING = 0x00000001,

        /// <summary>
        /// The render target will allow a call to GetDC on the ID2D1GdiInteropRenderTarget
        /// interface. Rendering will also occur locally.
        /// </summary>
        D2D1_RENDER_TARGET_USAGE_GDI_COMPATIBLE = 0x00000002,
        D2D1_RENDER_TARGET_USAGE_FORCE_DWORD = 0xffffffff
    }
}