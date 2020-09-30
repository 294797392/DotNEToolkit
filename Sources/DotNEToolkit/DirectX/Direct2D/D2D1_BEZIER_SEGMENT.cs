using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Describes a cubic bezier in a path.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_BEZIER_SEGMENT
    {
        public D2D1_POINT_2F point1;
        public D2D1_POINT_2F point2;
        public D2D1_POINT_2F point3;
    }
}