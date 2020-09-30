using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_DRAWING_STATE_DESCRIPTION类
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_DRAWING_STATE_DESCRIPTION
    {
        public D2D1_ANTIALIAS_MODE antialiasMode;
        public D2D1_TEXT_ANTIALIAS_MODE textAntialiasMode;
        public ulong tag1;
        public ulong tag2;
        public D2D1_MATRIX_3X2_F transform;
    }
}