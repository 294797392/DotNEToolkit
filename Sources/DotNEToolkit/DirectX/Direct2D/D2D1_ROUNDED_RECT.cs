using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Contains the dimensions and corner radii of a rounded rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_ROUNDED_RECT
    {
        public D2D1_RECT_F rect;
        public float radiusX;
        public float radiusY;
    }
}