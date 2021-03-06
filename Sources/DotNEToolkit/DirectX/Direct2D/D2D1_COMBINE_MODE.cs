using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// This enumeration describes the type of combine operation to be performed.
    /// </summary>
    public enum D2D1_COMBINE_MODE : uint
    {
        /// <summary>
        /// Produce a geometry representing the set of points contained in either the first
        /// or the second geometry.
        /// </summary>
        D2D1_COMBINE_MODE_UNION = 0,

        /// <summary>
        /// Produce a geometry representing the set of points common to the first and the
        /// second geometries.
        /// </summary>
        D2D1_COMBINE_MODE_INTERSECT = 1,

        /// <summary>
        /// Produce a geometry representing the set of points contained in the first
        /// geometry or the second geometry, but not both.
        /// </summary>
        D2D1_COMBINE_MODE_XOR = 2,

        /// <summary>
        /// Produce a geometry representing the set of points contained in the first
        /// geometry but not the second geometry.
        /// </summary>
        D2D1_COMBINE_MODE_EXCLUDE = 3,
        D2D1_COMBINE_MODE_FORCE_DWORD = 0xffffffff
    }
}