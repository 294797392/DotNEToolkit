using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    [Guid("06152247-6f50-465a-9245-118bfd3b6007")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1Factory
    {
        /// <summary>
        /// Cause the factory to refresh any system metrics that it might have been snapped
        /// on factory creation.
        /// </summary>
        /// <returns></returns>
        uint ReloadSystemMetrics();

        /// <summary>
        /// Retrieves the current desktop DPI. To refresh this, call ReloadSystemMetrics.
        /// </summary>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        void GetDesktopDpi(out float dpiX, out float dpiY);

        uint CreateRectangleGeometry(IntPtr rectangle, out IntPtr rectangleGeometry);

        uint CreateRoundedRectangleGeometry(IntPtr roundedRectangle, out IntPtr roundedRectangleGeometry);

        uint CreateEllipseGeometry(IntPtr ellipse, out IntPtr ellipseGeometry);

        /// <summary>
        /// Create a geometry which holds other geometries.
        /// </summary>
        uint CreateGeometryGroup(D2D1_FILL_MODE fillMode, IntPtr geometries, int geometriesCount, out IntPtr geometryGroup);

        uint CreateTransformedGeometry(IntPtr sourceGeometry, IntPtr transform, out IntPtr transformedGeometry);

        /// <summary>
        /// Returns an initially empty path geometry interface. A geometry sink is created
        /// off the interface to populate it.
        /// </summary>
        uint CreatePathGeometry(out IntPtr pathGeometry);

        /// <summary>
        /// Allows a non-default stroke style to be specified for a given geometry at draw
        /// time.
        /// </summary>
        uint CreateStrokeStyle(IntPtr strokeStyleProperties, IntPtr dashes, int dashesCount, out IntPtr strokeStyle);

        /// <summary>
        /// Creates a new drawing state block, this can be used in subsequent
        /// SaveDrawingState and RestoreDrawingState operations on the render target.
        /// </summary>
        uint CreateDrawingStateBlock(IntPtr drawingStateDescription, IntPtr textRenderingParams, out IntPtr drawingStateBlock);

        /// <summary>
        /// Creates a render target which is a source of bitmaps.
        /// </summary>
        uint CreateWicBitmapRenderTarget(IntPtr target, IntPtr renderTargetProperties, out IntPtr renderTarget);

        /// <summary>
        /// Creates a render target that appears on the display.
        /// </summary>
        uint CreateHwndRenderTarget(IntPtr renderTargetProperties, IntPtr hwndRenderTargetProperties, out IntPtr hwndRenderTarget);

        /// <summary>
        /// Creates a render target that draws to a DXGI Surface. The device that owns the
        /// surface is used for rendering.
        /// </summary>
        uint CreateDxgiSurfaceRenderTarget(IntPtr dxgiSurface, IntPtr renderTargetProperties, out IntPtr renderTarget);

        /// <summary>
        /// Creates a render target that draws to a GDI device context.
        /// </summary>
        uint CreateDCRenderTarget(IntPtr renderTargetProperties, out IntPtr dcRenderTarget);

        uint CreateRectangleGeometry(ref D2D1_RECT_F rectangle, out IntPtr rectangleGeometry);

        uint CreateRoundedRectangleGeometry(ref D2D1_ROUNDED_RECT roundedRectangle, out IntPtr roundedRectangleGeometry);

        uint CreateEllipseGeometry(ref D2D1_ELLIPSE ellipse, out IntPtr ellipseGeometry);

        uint CreateTransformedGeometry(IntPtr sourceGeometry, ref D2D1_MATRIX_3X2_F transform, out IntPtr transformedGeometry);

        uint CreateStrokeStyle(ref D2D1_STROKE_STYLE_PROPERTIES strokeStyleProperties, IntPtr dashes, int dashesCount, out IntPtr strokeStyle);

        uint CreateDrawingStateBlock(ref D2D1_DRAWING_STATE_DESCRIPTION drawingStateDescription, out IntPtr drawingStateBlock);

        uint CreateDrawingStateBlock(out IntPtr drawingStateBlock);

        uint CreateWicBitmapRenderTarget(IntPtr target, ref D2D1_RENDER_TARGET_PROPERTIES renderTargetProperties, out IntPtr renderTarget);

        uint CreateHwndRenderTarget(ref D2D1_RENDER_TARGET_PROPERTIES renderTargetProperties, ref D2D1_HWND_RENDER_TARGET_PROPERTIES hwndRenderTargetProperties, out IntPtr hwndRenderTarget);

        uint CreateDxgiSurfaceRenderTarget(IntPtr dxgiSurface, ref D2D1_RENDER_TARGET_PROPERTIES renderTargetProperties, out IntPtr enderTarget);
    }
}