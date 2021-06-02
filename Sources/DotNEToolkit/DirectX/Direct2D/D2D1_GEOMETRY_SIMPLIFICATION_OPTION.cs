using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_GEOMETRY_SIMPLIFICATION_OPTION类
    /// </summary>
    public enum D2D1_GEOMETRY_SIMPLIFICATION_OPTION : uint
    {
        D2D1_GEOMETRY_SIMPLIFICATION_OPTION_CUBICS_AND_LINES = 0,
        D2D1_GEOMETRY_SIMPLIFICATION_OPTION_LINES = 1,
        D2D1_GEOMETRY_SIMPLIFICATION_OPTION_FORCE_DWORD = 0xffffffff
    }
}