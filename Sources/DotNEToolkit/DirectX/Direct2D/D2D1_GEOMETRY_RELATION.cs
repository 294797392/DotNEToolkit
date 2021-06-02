using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_GEOMETRY_RELATION类
    /// </summary>
    public enum D2D1_GEOMETRY_RELATION : uint
    {
        /// <summary>
        /// The relation between the geometries couldn't be determined. This value is never
        /// returned by any D2D method.
        /// </summary>
        D2D1_GEOMETRY_RELATION_UNKNOWN = 0,

        /// <summary>
        /// The two geometries do not intersect at all.
        /// </summary>
        D2D1_GEOMETRY_RELATION_DISJOINT = 1,

        /// <summary>
        /// The passed in geometry is entirely contained by the object.
        /// </summary>
        D2D1_GEOMETRY_RELATION_IS_CONTAINED = 2,

        /// <summary>
        /// The object entirely contains the passed in geometry.
        /// </summary>
        D2D1_GEOMETRY_RELATION_CONTAINS = 3,

        /// <summary>
        /// The two geometries overlap but neither completely contains the other.
        /// </summary>
        D2D1_GEOMETRY_RELATION_OVERLAP = 4,
        D2D1_GEOMETRY_RELATION_FORCE_DWORD = 0xffffffff
    }
}