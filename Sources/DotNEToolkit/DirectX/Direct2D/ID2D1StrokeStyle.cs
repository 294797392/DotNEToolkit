using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// ID2D1StrokeStyle类
    /// </summary>
    [Guid("2cd9069d-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1StrokeStyle : ID2D1Resource
    {
        D2D1_CAP_STYLE GetStartCap();

        D2D1_CAP_STYLE GetEndCap();

        D2D1_CAP_STYLE GetDashCap();

        float GetMiterLimit();

        D2D1_LINE_JOIN GetLineJoin();

        float GetDashOffset();

        D2D1_DASH_STYLE GetDashStyle();

        uint GetDashesCount();

        /// <summary>
        /// Returns the dashes from the object into a user allocated array. The user must
        /// call GetDashesCount to retrieve the required size.
        /// </summary>
        void GetDashes(out float dashes, uint dashesCount);
    }
}