using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Stores an ordered pair of integers, typically the width and height of a
    /// rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_SIZE_U
    {
        public int width;
        public int height;
    }
}