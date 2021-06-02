using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Enum which describes the manner in which we render edges of non-text primitives.
    /// </summary>
    public enum D2D1_ANTIALIAS_MODE : uint
    {
        /// <summary>
        /// The edges of each primitive are antialiased sequentially.
        /// </summary>
        D2D1_ANTIALIAS_MODE_PER_PRIMITIVE = 0,

        /// <summary>
        /// Each pixel is rendered if its pixel center is contained by the geometry.
        /// </summary>
        D2D1_ANTIALIAS_MODE_ALIASED = 1,
        D2D1_ANTIALIAS_MODE_FORCE_DWORD = 0xffffffff
    }
}