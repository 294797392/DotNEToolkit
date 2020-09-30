using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// ID2D1Brush类
    /// </summary>
    [Guid("2cd906a8-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1Brush
    {
        /// <summary>
        /// Sets the opacity for when the brush is drawn over the entire fill of the brush.
        /// </summary>
        void SetOpacity(float opacity);

        /// <summary>
        /// Sets the transform that applies to everything drawn by the brush.
        /// </summary>
        void SetTransform(IntPtr transform);

        float GetOpacity();

        void GetTransform(out D2D1_MATRIX_3X2_F transform);

        void SetTransform(ref D2D1_MATRIX_3X2_F transfor);
    }
}