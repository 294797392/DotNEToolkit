using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Describes a geometric path that does not contain quadratic bezier curves or arcs.
    /// </summary>
    [Guid("2cd9069e-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1SimplifiedGeometrySink
    {
        void SetFillMode(D2D1_FILL_MODE fillMode);

        void SetSegmentFlags(D2D1_PATH_SEGMENT vertexFlags);

        void BeginFigure(D2D1_POINT_2F startPoint, D2D1_FIGURE_BEGIN figureBegin);

        void AddLines(IntPtr points, int pointsCount);

        void AddBeziers(IntPtr beziers, int beziersCount);

        void EndFigure(D2D1_FIGURE_END figureEnd);

        uint Close();
    }
}