using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// D2D1_DASH_STYLE类
    /// </summary>
    public enum D2D1_DASH_STYLE : uint
    {
        D2D1_DASH_STYLE_SOLID = 0,
        D2D1_DASH_STYLE_DASH = 1,
        D2D1_DASH_STYLE_DOT = 2,
        D2D1_DASH_STYLE_DASH_DOT = 3,
        D2D1_DASH_STYLE_DASH_DOT_DOT = 4,
        D2D1_DASH_STYLE_CUSTOM = 5,
        D2D1_DASH_STYLE_FORCE_DWORD = 0xffffffff
    }
}