using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_POINT_2F类
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_POINT_2F
    {
        public float x;
        public float y;
    }
}