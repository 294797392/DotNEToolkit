using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Populates an ID2D1Mesh object with triangles.
    /// </summary>
    [Guid("2cd906c1-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1TessellationSink
    {
        void AddTriangles(IntPtr triangles, int trianglesCount);

        uint Close();
    }
}