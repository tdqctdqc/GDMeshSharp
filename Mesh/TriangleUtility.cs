using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class TriangleUtility 
{
    public static bool BadTri(float minLength, Vector2 a, Vector2 b, Vector2 c)
    {
        if (GetMinAltitude(a,b,c) < minLength
            || GetMinEdgeLength(a,b,c) < minLength)
        {
            return true;
        }
        return false;
    }
    public static float GetTriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return .5f * Mathf.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
    }

    
    
    public static float GetMinEdgeLength(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var dist1 = p0.DistanceTo(p1);
        var dist2 = p0.DistanceTo(p2);
        var dist3 = p1.DistanceTo(p2);
        float min = Mathf.Min(dist1, dist2);
        return Mathf.Min(min, dist3);
    }

    public static float GetMinAltitude(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var area = GetArea(p0, p1, p2);
        var minAlt = Mathf.Inf;
        float altitude(Vector2 po0, Vector2 po1)
        {
            var baseLength = po1.DistanceTo(po0);
            return area * 2f / baseLength;
        }

        minAlt = Mathf.Min(minAlt, altitude(p0, p1));
        minAlt = Mathf.Min(minAlt, altitude(p1, p2));
        minAlt = Mathf.Min(minAlt, altitude(p2, p0));
        return minAlt;
    }
    public static float GetMinAltitude(List<Vector2> points)
    {
        return GetMinAltitude(points[0], points[1], points[2]);
    }

    public static float GetArea(TrianglePointPositions points)
    {
        return GetArea(points.a, points.b, points.c);
    }

    public static float GetArea(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var l0 = p0.DistanceTo(p1);
        var l1 = p1.DistanceTo(p2);
        var l2 = p2.DistanceTo(p0);
        var semiPerim = (l0 + l1 + l2) / 2f;
        return Mathf.Sqrt( semiPerim * (semiPerim - l0) * (semiPerim - l1) * (semiPerim - l2) );
    }
}