using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Indicates whether the given figure is filled or hollow.
    /// </summary>
    public enum D2D1_FIGURE_BEGIN : uint
    {
        D2D1_FIGURE_BEGIN_FILLED = 0,
        D2D1_FIGURE_BEGIN_HOLLOW = 1,
        D2D1_FIGURE_BEGIN_FORCE_DWORD = 0xffffffff
    }
}