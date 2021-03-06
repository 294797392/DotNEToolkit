using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Qualifies how alpha is to be treated in a bitmap or render target containing
    /// alpha.
    /// </summary>
    public enum D2D1_ALPHA_MODE : uint
    {
        /// <summary>
        /// Alpha mode should be determined implicitly. Some target surfaces do not supply
        /// or imply this information in which case alpha must be specified.
        /// </summary>
        D2D1_ALPHA_MODE_UNKNOWN = 0,

        /// <summary>
        /// Treat the alpha as premultipled.
        /// </summary>
        D2D1_ALPHA_MODE_PREMULTIPLIED = 1,

        /// <summary>
        /// Opacity is in the 'A' component only.
        /// </summary>
        D2D1_ALPHA_MODE_STRAIGHT = 2,

        /// <summary>
        /// Ignore any alpha channel information.
        /// </summary>
        D2D1_ALPHA_MODE_IGNORE = 3,

        D2D1_ALPHA_MODE_FORCE_DWORD = 0xffffffff
    }
}