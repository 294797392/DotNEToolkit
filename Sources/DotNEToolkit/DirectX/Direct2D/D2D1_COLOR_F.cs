using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D_COLOR_F类
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_COLOR_F
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }
}