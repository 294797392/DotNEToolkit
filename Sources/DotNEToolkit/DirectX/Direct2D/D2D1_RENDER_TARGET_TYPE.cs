using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Describes whether a render target uses hardware or software rendering, or if
    /// Direct2D should select the rendering mode.
    /// </summary>
    public enum D2D1_RENDER_TARGET_TYPE : uint
    {
        /// <summary>
        /// D2D is free to choose the render target type for the caller.
        /// </summary>
        D2D1_RENDER_TARGET_TYPE_DEFAULT = 0,

        /// <summary>
        /// The render target will render using the CPU.
        /// </summary>
        D2D1_RENDER_TARGET_TYPE_SOFTWARE = 1,

        /// <summary>
        /// The render target will render using the GPU.
        /// </summary>
        D2D1_RENDER_TARGET_TYPE_HARDWARE = 2,
        D2D1_RENDER_TARGET_TYPE_FORCE_DWORD = 0xffffffff
    }
}