using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Frontline
{
    public List<Triangle> TrisInOrderLeftToRight { get; private set; }
    public List<Vector2> Points { get; private set; }
    public Vector2 GoalPoint { get; private set; }
    public List<float> FrontageNeeds { get; private set; } // 1 less than points
    public List<float> FrontageRatioNeeds { get; private set; }
    public List<float> CumulativeFrontageRatio { get; private set; }

    public Frontline(WorldManager world, 
        List<Vector2> frontlinePoints, Func<Triangle, float> triFrontageCost)
    {
        TrisInOrderLeftToRight = new List<Triangle>();
        Points = new List<Vector2>();
        Points.Add(frontlinePoints[0]);
        for (var i = 0; i < frontlinePoints.Count - 1; i++)
        {
            var segment = world.Geometry
                .GetLineSegmentTriIntersections(frontlinePoints[i], frontlinePoints[i + 1]);
            if (segment == null) continue;
            var segmentTris = segment.Item1;
            var segmentPoints = segment.Item2;
            TrisInOrderLeftToRight.AddRange(segment.Item1);
            for (var i1 = 1; i1 < segment.Item2.Count; i1 += 2)
            {
                Points.Add(segmentPoints[i1]);
            }
        }

        FrontageNeeds = new List<float>();
        FrontageRatioNeeds = new List<float>();
        CumulativeFrontageRatio = new List<float>();
        for (var i = 0; i < Points.Count - 1; i++)
        {
            var tri = TrisInOrderLeftToRight[i];
            var length = Points[i + 1].DistanceTo(Points[i]);
            var frontageNeed = triFrontageCost(tri) * length;
            FrontageNeeds.Add(frontageNeed);
        }

        var accum = 0f;
        var frontageTotal = FrontageNeeds.Sum();
        for (var i = 0; i < FrontageNeeds.Count; i++)
        {
            var value = FrontageNeeds[i];
            accum += value;
            FrontageRatioNeeds.Add(value / frontageTotal);
            CumulativeFrontageRatio.Add(accum / frontageTotal);
        }
        for (var i = 0; i < Points.Count; i++)
        {
            GoalPoint += Points[i];
        }

        GoalPoint /= Points.Count;
    }
    public Frontline(WorldManager world, 
        List<Triangle> tris, Func<Triangle, float> triFrontageCost)
    {
        TrisInOrderLeftToRight = tris;
        Points = new List<Vector2>();
        FrontageNeeds = new List<float>();
        for (var i = 0; i < tris.Count - 1; i++)
        {
            var fromTri = tris[i];
            var toTri = tris[i + 1];
            var fromPoint = fromTri.GetCentroid(world.Geometry);
            var toPoint = toTri.GetCentroid(world.Geometry);
            var edgeBetweenTris = world.Geometry.GetEdgeBetweenTris(fromTri, toTri);
            var midPoint = GeometryExt.GetLineIntersection(fromPoint, toPoint, 
                world.Geometry.PointsById[world.Geometry.From[edgeBetweenTris]],
                world.Geometry.PointsById[world.Geometry.To[edgeBetweenTris]]);
            if(i == 0) Points.Add(fromPoint);
            Points.Add(midPoint);
            Points.Add(toPoint);
            FrontageNeeds.Add(triFrontageCost(fromTri) * fromPoint.DistanceTo(midPoint));
            FrontageNeeds.Add(triFrontageCost(toTri) * toPoint.DistanceTo(midPoint));
        }

        float totalFrontageNeed = FrontageNeeds.Sum();
        FrontageRatioNeeds = FrontageNeeds.Select(f => f / totalFrontageNeed).ToList();
        CumulativeFrontageRatio = new List<float>();
        
        float cumul = 0f;
        for (var i = 0; i < FrontageRatioNeeds.Count; i++)
        { 
            cumul += FrontageRatioNeeds[i];
            CumulativeFrontageRatio.Add(cumul);
        }
        
        for (var i = 0; i < Points.Count; i++)
        {
            GoalPoint += Points[i];
        }

        GoalPoint /= Points.Count;
    }
    public List<Vector2> GetFrontStartEndPoints<T>(WorldManager world, List<T> units, 
        Func<T, float> unitForceFunc)
    {
        var unitForcesTotal = units.Select(unitForceFunc).Sum();
        var unitCumulForcesRatio = new List<float>();
        float accum = 0f;
        for (var i = 0; i < units.Count; i++)
        {
            accum += unitForceFunc(units[i]) / unitForcesTotal;
            unitCumulForcesRatio.Add(accum);
        }

        var frontPositions = new List<Vector2>{Points[0]};
        int bookmark = 0;
        for (var i = 0; i < units.Count; i++)
        {
            var unitCumulForce = unitCumulForcesRatio[i];
            bool found = false;
            for (int j = bookmark; j < CumulativeFrontageRatio.Count; j++)
            {
                if (unitCumulForce <= CumulativeFrontageRatio[j])
                {
                    bookmark = j;
                    var cumulFrontage = CumulativeFrontageRatio[j];
                    var from = Points[j];
                    var to = Points[j + 1];
                    var segmentLength = FrontageRatioNeeds[j];
                    var progressAlongSegment =
                        1f - 
                        (cumulFrontage - unitCumulForce)
                        / segmentLength;

                    var pos = from.LinearInterpolate(to, progressAlongSegment);
                        
                    frontPositions.Add(pos);
                    found = true;
                    break;
                }
            }
            
            if(found == false)
            {
                frontPositions.Add(Points[Points.Count - 1]);
            }
        }
        return frontPositions;
    }

    
}
