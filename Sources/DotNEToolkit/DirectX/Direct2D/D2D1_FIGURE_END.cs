using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Indicates whether the figure is open or closed on its end point.
    /// </summary>
    public enum D2D1_FIGURE_END : uint
    {
        D2D1_FIGURE_END_OPEN = 0,
        D2D1_FIGURE_END_CLOSED = 1,
        D2D1_FIGURE_END_FORCE_DWORD = 0xffffffff
    }
}