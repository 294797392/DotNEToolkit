using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Indicates whether the given segment should be stroked, or, if the join between
    /// this segment and the previous one should be smooth.
    /// </summary>
    public enum D2D1_PATH_SEGMENT : uint
    {
        D2D1_PATH_SEGMENT_NONE = 0x00000000,
        D2D1_PATH_SEGMENT_FORCE_UNSTROKED = 0x00000001,
        D2D1_PATH_SEGMENT_FORCE_ROUND_LINE_JOIN = 0x00000002,
        D2D1_PATH_SEGMENT_FORCE_DWORD = 0xffffffff
    }
}