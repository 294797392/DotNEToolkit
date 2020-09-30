using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Describes whether a window is occluded.
    /// </summary>
    public enum D2D1_WINDOW_STATE : uint
    {
        D2D1_WINDOW_STATE_NONE = 0x0000000,
        D2D1_WINDOW_STATE_OCCLUDED = 0x0000001,
        D2D1_WINDOW_STATE_FORCE_DWORD = 0xffffffff
    }
}