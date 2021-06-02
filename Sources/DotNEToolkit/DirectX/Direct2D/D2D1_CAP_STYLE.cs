using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_CAP_STYLE类
    /// </summary>
    public enum D2D1_CAP_STYLE : uint
    {
        /// <summary>
        /// Flat line cap.
        /// </summary>
        D2D1_CAP_STYLE_FLAT = 0,

        /// <summary>
        /// Square line cap.
        /// </summary>
        D2D1_CAP_STYLE_SQUARE = 1,

        /// <summary>
        /// Round line cap.
        /// </summary>
        D2D1_CAP_STYLE_ROUND = 2,

        /// <summary>
        /// Triangle line cap.
        /// </summary>
        D2D1_CAP_STYLE_TRIANGLE = 3,

        D2D1_CAP_STYLE_FORCE_DWORD = 0xffffffff
    }
}