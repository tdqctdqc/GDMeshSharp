using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Triangle
{
    public int Id { get; private set; }
    public List<int> HalfEdges { get; private set; }
    public List<int> Points { get; private set; }
    public TriangleInfo Info { get; private set; }
    
    public Triangle(int id, int edgeA, int edgeAFrom, int edgeB, int edgeBFrom, int edgeC, int edgeCFrom)
    {
        Id = id;
        HalfEdges = new List<int> { edgeA, edgeB, edgeC };
        Points = new List<int> { edgeAFrom, edgeBFrom, edgeCFrom };
        Info = new TriangleInfo();
    }
    public void SortEdges(GeometryManager geometry)
    {
        var first = HalfEdges[0];
        var firstFrom = Points[0];
        var lastTo = geometry.To[HalfEdges[2]];
        
        if (firstFrom != lastTo)
        {
            var edge = HalfEdges[1];
            HalfEdges[1] = HalfEdges[2];
            HalfEdges[2] = edge;
            var point = Points[1];
            Points[1] = Points[2];
            Points[2] = point;
        }
    }
    public TrianglePointIds GetPoints(GeometryManager geometry)
    {
        return new TrianglePointIds( 
            geometry.From[HalfEdges[0]], 
            geometry.From[HalfEdges[1]], 
            geometry.From[HalfEdges[2]] 
        );
    }
    public TrianglePointPositions GetPointPositions(GeometryManager geometry)
    {
        return new TrianglePointPositions(this, geometry);
    }
    public Vector2 GetCentroid(GeometryManager geometry)
    {
        return (geometry.PointsById[Points[0]]
                + geometry.PointsById[Points[1]]
                + geometry.PointsById[Points[2]])
               / 3f;
    }
    public float GetArea(GeometryManager geometry)
    {
        return TriangleUtility.GetArea(GetPointPositions(geometry));
    }
}

public struct TrianglePointIds
{
    public int a;
    public int b;
    public int c;
    
    public TrianglePointIds(int a, int b, int c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
}
public struct TrianglePointPositions
{
    public Vector2 a;
    public Vector2 b;
    public Vector2 c;
    
    public TrianglePointPositions(Vector2 a, Vector2 b, Vector2 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public TrianglePointPositions(Triangle tri, GeometryManager geometry)
    {
        a = geometry.PointsById[geometry.From[tri.HalfEdges[0]]];
        b = geometry.PointsById[geometry.From[tri.HalfEdges[1]]];
        c = geometry.PointsById[geometry.From[tri.HalfEdges[2]]];
    }
}

public struct TriangleRays
{
    public Vector2 forwardRay;
    public Vector2 backRay;

    public TriangleRays(Vector2 backRay, Vector2 forwardRay)
    {
        this.forwardRay = forwardRay;
        this.backRay = backRay;
    }
}
public static class TriangleExt
{
    public static float GetFrontageCost(this Triangle tri)
    {
        if (tri == null) return 0f;
        return 2f - tri.Info.Roughness.OutValue;
    }
    public static int GetMaxPointBy<T>(this Triangle tri, Func<int, T> func) where T: IComparable
    {
        var aVal = func(tri.Points[0]);
        var bVal = func(tri.Points[1]);
        var cVal = func(tri.Points[2]);
        int max = tri.Points[0];
        if (bVal.CompareTo(aVal) >= 0)
        {
            aVal = bVal;
            max = tri.Points[1];
        }

        if (cVal.CompareTo(aVal) >= 0)
        {
            max = tri.Points[2];
        }

        return max;
    }
    public static int GetMinPointBy<T>(this Triangle tri, Func<int, T> func) where T: IComparable
    {
        var aVal = func(tri.Points[0]);
        var bVal = func(tri.Points[1]);
        var cVal = func(tri.Points[2]);
        int max = tri.Points[0];
        if (bVal.CompareTo(aVal) <= 0)
        {
            aVal = bVal;
            max = tri.Points[1];
        }

        if (cVal.CompareTo(aVal) <= 0)
        {
            max = tri.Points[2];
        }

        return max;
    }
    public static T GetMaxOfPoints<T>(this Triangle tri, Func<int, T> func) where T : IComparable
    {
        var aVal = func(tri.Points[0]);
        var bVal = func(tri.Points[1]);
        var cVal = func(tri.Points[2]);

        if (bVal.CompareTo(bVal) >= 0)
        { aVal = bVal; }

        if (cVal.CompareTo(aVal) >= 0)
        { aVal = cVal; }

        return aVal;
    }
    public static T GetMinOfPoints<T>(this Triangle tri, Func<int, T> func) where T : IComparable
    {
        var aVal = func(tri.Points[0]);
        var bVal = func(tri.Points[1]);
        var cVal = func(tri.Points[2]);

        if (bVal.CompareTo(bVal) <= 0)
        { aVal = bVal; }

        if (cVal.CompareTo(aVal) <= 0)
        { aVal = cVal; }

        return aVal;
    }
    public static int GetMaxEdgeBy<T>(this Triangle tri, Func<int, T> func) where T: IComparable
    {
        var aVal = func(tri.HalfEdges[0]);
        var bVal = func(tri.HalfEdges[1]);
        var cVal = func(tri.HalfEdges[2]);
        int max = tri.HalfEdges[0];
        if (bVal.CompareTo(aVal) >= 0)
        {
            aVal = bVal;
            max = tri.HalfEdges[1];
        }

        if (cVal.CompareTo(aVal) >= 0)
        {
            max = tri.HalfEdges[2];
        }

        return max;
    }
    public static int GetMinEdgeBy<T>(this Triangle tri, Func<int, T> func) where T: IComparable
    {
        var aVal = func(tri.HalfEdges[0]);
        var bVal = func(tri.HalfEdges[1]);
        var cVal = func(tri.HalfEdges[2]);
        int max = tri.HalfEdges[0];
        if (bVal.CompareTo(aVal) <= 0)
        {
            aVal = bVal;
            max = tri.HalfEdges[1];
        }

        if (cVal.CompareTo(aVal) <= 0)
        {
            max = tri.HalfEdges[2];
        }

        return max;
    }
    public static T GetMaxOfEdges<T>(this Triangle tri, Func<int, T> func) where T : IComparable
    {
        var aVal = func(tri.HalfEdges[0]);
        var bVal = func(tri.HalfEdges[1]);
        var cVal = func(tri.HalfEdges[2]);

        if (bVal.CompareTo(bVal) >= 0)
            { aVal = bVal; }

        if (cVal.CompareTo(aVal) >= 0)
            { aVal = cVal; }

        return aVal;
    }
    public static T GetMinOfEdges<T>(this Triangle tri, Func<int, T> func) where T : IComparable
    {
        var aVal = func(tri.HalfEdges[0]);
        var bVal = func(tri.HalfEdges[1]);
        var cVal = func(tri.HalfEdges[2]);

        if (bVal.CompareTo(bVal) <= 0)
            { aVal = bVal; }

        if (cVal.CompareTo(aVal) <= 0)
            { aVal = cVal; }

        return aVal;
    }

    public static List<Triangle> GetTrisSharingEdge(this Triangle tri, GeometryManager geometry)
    {
        return tri.HalfEdges
            .Where(e => geometry.HalfEdgePairs.Contains(e))
            .Select(e => geometry.TrianglesByEdgeIds[(int) geometry.HalfEdgePairs[e]])
            .ToList();
    }
    public static List<Triangle> GetTrisSharingPoint(this Triangle tri, GeometryManager geometry)
    {
        IEnumerable<int> edgesSharingPoint = geometry.ToEdgesForPoint[tri.Points[0]];
        edgesSharingPoint = edgesSharingPoint.Union(geometry.ToEdgesForPoint[tri.Points[1]]);
        edgesSharingPoint = edgesSharingPoint.Union(geometry.ToEdgesForPoint[tri.Points[2]]);
        return edgesSharingPoint
            .Select(e => geometry.TrianglesByEdgeIds[e])
            .Distinct()
            .Where(t => t != tri)
            .ToList();
    }

    public static void DoForEachTriSharingAnyPoint(this Triangle tri, 
        Action<Triangle> action,
        GeometryManager geometry)
    {
        void doForPoint(int index)
        {
            var point = tri.Points[index];
            var edge = tri.HalfEdges[index];
            var prevEdge = tri.HalfEdges[(index + 2) % 3];
            Triangle ignore = null;
            if (geometry.HalfEdgePairs[prevEdge] is int oPrevEdge)
            {
                ignore = geometry.TrianglesByEdgeIds[oPrevEdge];
            }
            foreach (var e in geometry.ToEdgesForPoint[point])
            {
                var aTri = geometry.TrianglesByEdgeIds[e];
                if (tri == aTri || aTri == ignore) continue;
                action(aTri);
            }
        }
        doForPoint(0);
        doForPoint(1);
        doForPoint(2);
    }
    public static int GetEdgeOppositePoint(this Triangle tri, GeometryManager geometry, int point)
    {
        var index = tri.Points.IndexOf(point);
        return tri.HalfEdges[(index + 2) % 3];
    }
    
    public static TrianglePointPositions GetSplitPoints(this Triangle tri, GeometryManager geometry)
    {
        var points0 = geometry.GetEdgeFromAndToPoints(tri.HalfEdges[0]);
        var points1 = geometry.GetEdgeFromAndToPoints(tri.HalfEdges[1]);
        var points2 = geometry.GetEdgeFromAndToPoints(tri.HalfEdges[2]);

        return new TrianglePointPositions(
            (points0.from + points0.to) / 2f,
            (points1.from + points1.to) / 2f,
            (points2.from + points2.to) / 2f);
    }
    public static TriangleRays 
        GetRaysFromPoint(this Triangle tri, GeometryManager geometry, int point)
    {
        var oppEdge = tri.GetEdgeOppositePoint(geometry, point);
        var pointPos = geometry.PointsById[point];
        var fromTo = geometry.GetEdgeFromAndToPoints(oppEdge);

        return new TriangleRays(fromTo.to - pointPos, fromTo.from - pointPos);
    }
    public static Vector2 GetRandomPointInCone(this Triangle tri, 
        GeometryManager geometry, int point, float coneRatio)
    {
        var conePointPos = geometry.PointsById[point];
        var rays = tri.GetRaysFromPoint(geometry, point);
        var rand = Game.Random.RandfRange(0f, coneRatio);
        return conePointPos + rays.backRay * rand + rays.forwardRay * (coneRatio - rand);
    }
    public static Vector2 GetRandomPointInsideMargin(this Triangle tri, 
        GeometryManager geometry, float marginRatio)
    {
        var points = tri.GetPointPositions(geometry);
        var ray1 = points.a - points.b;
        var norm1 = ray1.Normalized();
        var ray2 = points.c - points.b;
        var norm2 = ray2.Normalized();
        var hinge = points.b;

        var share1 = Game.Random.RandfRange(0f, 1f);
        var share2 = 1f - share1;
        var shareToHave = 1f - marginRatio;
        var length1 = share1 * shareToHave * ray1.Length();
        var length2 = share2 * shareToHave * ray2.Length();
        var min = (norm1 + norm2) * (marginRatio / 2f);
        return hinge + norm1 * length1 + norm2 * length2 + min;
    }
    public static bool PointInsideTriangle(this Triangle tri, Vector2 pt, GeometryManager geometry)
    {
        bool hasNeg, hasPos;
        var points = tri.GetPointPositions(geometry);
        var v1 = points.a;
        var v2 = points.b;
        var v3 = points.c;
        float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
        var d1 = sign(pt, points.a, points.b);
        var d2 = sign(pt, points.b, points.c);
        var d3 = sign(pt, points.c, points.a);

        hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    public static Tuple<Vector2, Vector2> 
        GetIntersectionOfLineSegment(this Triangle tri, GeometryManager geometry, Vector2 from, Vector2 to)
    {
        Vector2 i1 = Vector2.Zero, i2 = Vector2.Zero;
        bool hasI1 = false, hasI2 = false;
        if (tri.PointInsideTriangle(from, geometry))
        {
            i1 = from;
            hasI1 = true;
        }
        if (tri.PointInsideTriangle(to, geometry))
        {
            if (hasI1 == false)
            {
                i1 = to;
                hasI1 = true;
            }
            else
            {
                i2 = to;
                hasI2 = true;
            }
        }

        if (hasI1 && hasI2)
        {
            return new Tuple<Vector2, Vector2>(i1, i2);
        }

        int bookmark = -1;
            //one point inside tri
        for (int i = 0; i < 3; i++)
        {
            var edge = tri.HalfEdges[i];
            var edgeFrom = geometry.PointsById[geometry.From[edge]];
            var edgeTo = geometry.PointsById[geometry.To[edge]];
            var intersection = GeometryExt.GetLineIntersection(edgeFrom, edgeTo, from, to);
            if (intersection.PointOnLineIsInLineSegment(from, to))
            {
                i2 = intersection;
                hasI2 = true;
                bookmark = i + 1;
                break;
            }
        }

        if (hasI2 == false) return null;
        if (hasI1 == false)
        {
            for (int i = bookmark; i < 3; i++)
            {
                var edge = tri.HalfEdges[i];
                var edgeFrom = geometry.PointsById[geometry.From[edge]];
                var edgeTo = geometry.PointsById[geometry.To[edge]];
                var intersection = GeometryExt.GetLineIntersection(edgeFrom, edgeTo, from, to);
                if (intersection.PointOnLineIsInLineSegment(from, to))
                {
                    i1 = intersection;
                    break;
                }
            }

            return null;
        }

        return new Tuple<Vector2, Vector2>(i1, i2);
    }
    public static float GetLongestEdgeLength(this Triangle tri, GeometryManager geometry)
    {
        return GetMaxOfEdges(tri, e => geometry.GetEdgeLength(e));
    }
    public static Vector2[] GetInnerTriPoints(this Triangle tri, GeometryManager geometry, float dist)
    {
        var points = tri.GetPointPositions(geometry);
        var aOuter = points.a;
        var bOuter = points.b;
        var cOuter = points.c;
        var centroid = (aOuter + bOuter + cOuter) / 3f;
        var aInner = aOuter + (centroid - aOuter).Normalized() * dist;
        var bInner = bOuter + (centroid - bOuter).Normalized() * dist;
        var cInner = cOuter + (centroid - cOuter).Normalized() * dist;
        return new Vector2[] {aInner, bInner, cInner};
    }
}
