using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Represents a 3-by-2 matrix.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_MATRIX_3X2_F
    {
        /// <summary>
        /// Horizontal scaling / cosine of rotation
        /// </summary>
        public float m11;

        /// <summary>
        /// Vertical shear / sine of rotation
        /// </summary>
        public float m12;

        /// <summary>
        /// Horizontal shear / negative sine of rotation
        /// </summary>
        public float m21;

        /// <summary>
        /// Vertical scaling / cosine of rotation
        /// </summary>
        public float m22;

        /// <summary>
        /// Horizontal shift (always orthogonal regardless of rotation)
        /// </summary>
        public float dx;

        /// <summary>
        /// Vertical shift (always orthogonal regardless of rotation)
        /// </summary>
        public float dy;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public float[][] m;
    }
}