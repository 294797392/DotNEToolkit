using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Represents a geometry resource and defines a set of helper methods for
    /// manipulating and measuring geometric shapes. Interfaces that inherit from
    /// ID2D1Geometry define specific shapes.
    /// </summary>
    [Guid("2cd906a1-12e2-11dc-9fed-001143a055f9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID2D1Geometry
    {
        /// <summary>
        /// Retrieve the bounds of the geometry, with an optional applied transform.
        /// </summary>
        uint GetBounds(IntPtr worldTransform, out D2D1_RECT_F bounds);

        /// <summary>
        /// Get the bounds of the corresponding geometry after it has been widened or have
        /// an optional pen style applied.
        /// </summary>
        uint GetWidenedBounds(float strokeWidth, IntPtr strokeStyle, IntPtr worldTransform, float flatteningTolerance, out D2D1_RECT_F bounds);

        /// <summary>
        /// Checks to see whether the corresponding penned and widened geometry contains the
        /// given point.
        /// </summary>
        uint StrokeContainsPoint(D2D1_POINT_2F point, float strokeWidth, IntPtr strokeStyle, IntPtr worldTransform, float flatteningTolerance, out bool contains);

        /// <summary>
        /// Test whether the given fill of this geometry would contain this point.
        /// </summary>
        uint FillContainsPoint(D2D1_POINT_2F point, IntPtr worldTransform, float flatteningTolerance, out bool contains);

        /// <summary>
        /// Compare how one geometry intersects or contains another geometry.
        /// </summary>
        uint CompareWithGeometry(IntPtr inputGeometry, IntPtr inputGeometryTransform, float flatteningTolerance, out D2D1_GEOMETRY_RELATION relation);

        /// <summary>
        /// Converts a geometry to a simplified geometry that has arcs and quadratic beziers
        /// removed.
        /// </summary>
        uint Simplify(D2D1_GEOMETRY_SIMPLIFICATION_OPTION simplificationOption, IntPtr worldTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Tessellates a geometry into triangles.
        /// </summary>
        uint Tessellate(IntPtr worldTransform, float flatteningTolerance, IntPtr tessellationSink);

        /// <summary>
        /// Performs a combine operation between the two geometries to produce a resulting
        /// geometry.
        /// </summary>
        uint CombineWithGeometry(IntPtr inputGeometry, D2D1_COMBINE_MODE combineMode, IntPtr inputGeometryTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Computes the outline of the geometry. The result is written back into a
        /// simplified geometry sink.
        /// </summary>
        uint Outline(IntPtr worldTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Computes the area of the geometry
        /// </summary>
        uint ComputeArea(IntPtr worldTransform, float flatteningTolerance, out float area);

        /// <summary>
        /// Computes the length of the geometry.
        /// </summary>
        uint ComputeLength(IntPtr worldTransform, float flatteningTolerance, out float length);

        /// <summary>
        /// Computes the point and tangent a given distance along the path.
        /// </summary>
        uint ComputePointAtLength(float length, IntPtr worldTransform, float flatteningTolerance, out D2D1_POINT_2F point, out D2D1_POINT_2F unitTangentVector);

        /// <summary>
        /// Get the geometry and widen it as well as apply an optional pen style.
        /// </summary>
        uint Widen(float strokeWidth, IntPtr strokeStyle, IntPtr worldTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Get the bounds of the corresponding geometry after it has been widened or have
        /// an optional pen style applied.
        /// </summary>
        uint GetWidenedBounds(float strokeWidth, IntPtr strokeStyle, out D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, out D2D1_RECT_F bounds);

        /// <summary>
        /// Get the bounds of the corresponding geometry after it has been widened or have
        /// an optional pen style applied.
        /// </summary>
        uint GetWidenedBounds(float strokeWidth, IntPtr strokeStyle, IntPtr worldTransform, out D2D1_RECT_F bounds);

        /// <summary>
        /// Get the bounds of the corresponding geometry after it has been widened or have
        /// an optional pen style applied.
        /// </summary>
        uint GetWidenedBounds(float strokeWidth, IntPtr strokeStyle, out D2D1_MATRIX_3X2_F worldTransform, out D2D1_RECT_F bounds);

        uint StrokeContainsPoint(D2D1_POINT_2F point, float strokeWidth, IntPtr strokeStyle, out D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, out bool contains);

        /// <summary>
        /// Checks to see whether the corresponding penned and widened geometry contains the
        /// given point.
        /// </summary>
        uint StrokeContainsPoint(D2D1_POINT_2F point, float strokeWidth, IntPtr strokeStyle, IntPtr worldTransform, out bool contains);

        uint StrokeContainsPoint(D2D1_POINT_2F point, float strokeWidth, IntPtr strokeStyle, out D2D1_MATRIX_3X2_F worldTransform, out bool contains);

        uint FillContainsPoint(D2D1_POINT_2F point, out D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, out bool contains);

        /// <summary>
        /// Test whether the given fill of this geometry would contain this point.
        /// </summary>
        uint FillContainsPoint(D2D1_POINT_2F point, IntPtr worldTransform, out bool contains);

        uint FillContainsPoint(D2D1_POINT_2F point, out D2D1_MATRIX_3X2_F worldTransform, out bool contains);

        /// <summary>
        /// Compare how one geometry intersects or contains another geometry.
        /// </summary>
        uint CompareWithGeometry(IntPtr inputGeometry, out D2D1_MATRIX_3X2_F inputGeometryTransform, float flatteningTolerance, out D2D1_GEOMETRY_RELATION relation);

        /// <summary>
        /// Compare how one geometry intersects or contains another geometry.
        /// </summary>
        uint CompareWithGeometry(IntPtr inputGeometry, IntPtr inputGeometryTransform, out D2D1_GEOMETRY_RELATION relation);

        /// <summary>
        /// Compare how one geometry intersects or contains another geometry.
        /// </summary>
        uint CompareWithGeometry(IntPtr inputGeometry, ref D2D1_MATRIX_3X2_F inputGeometryTransform, out D2D1_GEOMETRY_RELATION relation);

        /// <summary>
        /// Converts a geometry to a simplified geometry that has arcs and quadratic beziers
        /// removed.
        /// </summary>
        uint Simplify(D2D1_GEOMETRY_SIMPLIFICATION_OPTION simplificationOption, ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Converts a geometry to a simplified geometry that has arcs and quadratic beziers
        /// removed.
        /// </summary>
        uint Simplify(D2D1_GEOMETRY_SIMPLIFICATION_OPTION simplificationOption, IntPtr worldTransform, IntPtr geometrySink);

        /// <summary>
        /// Converts a geometry to a simplified geometry that has arcs and quadratic beziers
        /// removed.
        /// </summary>
        uint Simplify(D2D1_GEOMETRY_SIMPLIFICATION_OPTION simplificationOption, ref D2D1_MATRIX_3X2_F worldTransform, IntPtr geometrySink);

        /// <summary>
        /// Tessellates a geometry into triangles.
        /// </summary>
        uint Tessellate(ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, IntPtr tessellationSink);

        /// <summary>
        /// Tessellates a geometry into triangles.
        /// </summary>
        uint Tessellate(IntPtr worldTransform, IntPtr tessellationSink);

        /// <summary>
        /// Tessellates a geometry into triangles.
        /// </summary>
        uint Tessellate(ref D2D1_MATRIX_3X2_F worldTransform, IntPtr tessellationSink);

        /// <summary>
        /// Performs a combine operation between the two geometries to produce a resulting
        /// geometry.
        /// </summary>
        uint CombineWithGeometry(IntPtr inputGeometry, D2D1_COMBINE_MODE combineMode, ref D2D1_MATRIX_3X2_F inputGeometryTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Performs a combine operation between the two geometries to produce a resulting
        /// geometry.
        /// </summary>
        uint CombineWithGeometry(IntPtr inputGeometry, D2D1_COMBINE_MODE combineMode, IntPtr inputGeometryTransform, IntPtr geometrySink);

        /// <summary>
        /// Performs a combine operation between the two geometries to produce a resulting
        /// geometry.
        /// </summary>
        uint CombineWithGeometry(IntPtr inputGeometry, D2D1_COMBINE_MODE combineMode, ref D2D1_MATRIX_3X2_F inputGeometryTransform, IntPtr geometrySink);

        /// <summary>
        /// Computes the outline of the geometry. The result is written back into a
        /// simplified geometry sink.
        /// </summary>
        uint Outline(ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Computes the outline of the geometry. The result is written back into a
        /// simplified geometry sink.
        /// </summary>
        uint Outline(IntPtr worldTransform, IntPtr geometrySink);

        /// <summary>
        /// Computes the outline of the geometry. The result is written back into a
        /// simplified geometry sink.
        /// </summary>
        uint Outline(ref D2D1_MATRIX_3X2_F worldTransform, IntPtr geometrySink);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        uint ComputeArea(ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, out float area);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        uint ComputeArea(IntPtr worldTransform, out float area);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        uint ComputeArea(ref D2D1_MATRIX_3X2_F worldTransform, out float area);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        uint ComputeLength(ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, out float length);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        uint ComputeLength(IntPtr worldTransform, out float length);

        /// <summary>
        /// Computes the area of the geometry.
        /// </summary>
        uint ComputeLength(ref D2D1_MATRIX_3X2_F worldTransform, out float length);

        /// <summary>
        /// Computes the point and tangent a given distance along the path.
        /// </summary>
        uint ComputePointAtLength(float length, ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, out D2D1_POINT_2F point, out D2D1_POINT_2F unitTangentVector);

        /// <summary>
        /// Computes the point and tangent a given distance along the path.
        /// </summary>
        uint ComputePointAtLength(float length, IntPtr worldTransform, out D2D1_POINT_2F point, out D2D1_POINT_2F unitTangentVector);

        /// <summary>
        /// Computes the point and tangent a given distance along the path.
        /// </summary>
        uint ComputePointAtLength(float length, ref D2D1_MATRIX_3X2_F worldTransform, out D2D1_POINT_2F point, ref D2D1_POINT_2F unitTangentVector);

        /// <summary>
        /// Get the geometry and widen it as well as apply an optional pen style.
        /// </summary>
        uint Widen(float strokeWidth, IntPtr strokeStyle, ref D2D1_MATRIX_3X2_F worldTransform, float flatteningTolerance, IntPtr geometrySink);

        /// <summary>
        /// Get the geometry and widen it as well as apply an optional pen style.
        /// </summary>
        uint Widen(float strokeWidth, IntPtr strokeStyle, IntPtr worldTransform, IntPtr geometrySink);

        /// <summary>
        /// Get the geometry and widen it as well as apply an optional pen style.
        /// </summary>
        uint Widen(float strokeWidth, IntPtr strokeStyle, ref D2D1_MATRIX_3X2_F worldTransform, IntPtr geometrySink);
    }
}