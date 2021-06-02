using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Paints an area with a solid color.
    /// </summary>
    [Guid("2cd906a9-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1SolidColorBrush
    {
        void SetColor(IntPtr color);

        D2D1_COLOR_F GetColor();

        void SetColor(ref D2D1_COLOR_F color);
    }
}