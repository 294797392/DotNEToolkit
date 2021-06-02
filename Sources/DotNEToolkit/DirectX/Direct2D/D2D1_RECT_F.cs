using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Represents a rectangle defined by the coordinates of the upper-left corner
    /// (left, top) and the coordinates of the lower-right corner (right, bottom).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_RECT_F
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }
}