using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Enum which describes the drawing of the corners on the line.
    /// </summary>
    public enum D2D1_LINE_JOIN : uint
    {
        /// <summary>
        /// Miter join.
        /// </summary>
        D2D1_LINE_JOIN_MITER = 0,

        /// <summary>
        /// Bevel join.
        /// </summary>
        D2D1_LINE_JOIN_BEVEL = 1,

        /// <summary>
        /// Round join.
        /// </summary>
        D2D1_LINE_JOIN_ROUND = 2,

        /// <summary>
        /// Miter/Bevel join.
        /// </summary>
        D2D1_LINE_JOIN_MITER_OR_BEVEL = 3,
        D2D1_LINE_JOIN_FORCE_DWORD = 0xffffffff
    }
}