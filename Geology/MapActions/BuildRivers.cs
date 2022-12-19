using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class BuildRivers : MapAction
{
    public override void DoAction(WorldManager world)
    {
        var sourcePoints = PickSourcePoints(world);
        var shorePoints = GetAllShorePoints(world);
        var rivers = DrawRivers(world, shorePoints, sourcePoints);
        world.Rivers.AddRivers(rivers);
    }

    private List<int> PickSourcePoints(WorldManager world)
    {
        var plates = world.Plates;
        var sourcePoints = new ConcurrentBag<int>();
        var geometry = world.Geometry;
        
        void registerPlateSourcePoints(Plate plate)
        {
            var landTris = plate.Cells
                .SelectMany(c => c.Triangles)
                .Where(t => t.Info.IsLand)
                .ToList();
            var sourcePointCount = Mathf.FloorToInt(landTris.Count / 20f);
            int thisPlateSourcePointCount = 0;
            while(landTris.Count > 0 && thisPlateSourcePointCount < sourcePointCount)
            {
                var tri = landTris.GetRandomElement();
                landTris.Remove(tri);
                for (int j = 0; j < 3; j++)
                {
                    var point = tri.Points[j];
                    bool noWaterNeighbors = true;
                    foreach (var edge in geometry.FromEdgesForPoint[point])
                    {
                        var t = geometry.TrianglesByEdgeIds[edge];
                        if (t.Info.IsWater)
                        {
                            noWaterNeighbors = false;
                            break;
                        }
                    }

                    if (noWaterNeighbors)
                    {
                        sourcePoints.Add(point);
                        thisPlateSourcePointCount++;
                    }
                }
            }
        }
        plates.Plates.TryParallel(registerPlateSourcePoints);
        return sourcePoints.ToList();
    }

    private List<int> GetAllShorePoints(WorldManager world)
    {
        var geometry = world.Geometry;
        var shorePoints = new ConcurrentBag<int>();

        var points = world.Geometry.PointsById.Keys.ToList();
        
        void checkPointIsShore(int point)
        {
            bool hasLand = false;
            bool hasWater = false;
            foreach (var edge in geometry.FromEdgesForPoint[point])
            {
                var t = geometry.TrianglesByEdgeIds[edge];
                if (t.Info.IsLand)
                    hasLand = true;
                else hasWater = true;
                if (hasLand && hasWater)
                {
                    shorePoints.Add(point);
                    return;
                }
            }
        }
        points.TryParallel(checkPointIsShore);
        return shorePoints.Distinct().ToList();
    }

    private List<List<int>> DrawRivers(WorldManager world,
        List<int> shorePoints, List<int> sourcePoints)
    {
        var rivers = new ConcurrentBag<List<int>>();
        var geometry = world.Geometry;

        float getDrainCost(int from, int to)
        {
            var edges = geometry.EdgesBetweenPoints.GetEdge(from, to);
            if (edges.Count < 2) return Mathf.Inf;
            var tri0 = geometry.TrianglesByEdgeIds[edges[0]];
            var tri1 = geometry.TrianglesByEdgeIds[edges[1]];
            if (tri0.Info.IsWater || tri1.Info.IsWater) return Mathf.Inf;
            var length = geometry.GetEdgeLength(edges[0]);
            
            return length * (tri0.Info.Roughness.OutValue + tri1.Info.Roughness.OutValue);
        }
        void drawRiver(int sourcePoint)
        {
            var newPath = PathFinder<int>.FindPathMultipleEndsNew(
                sourcePoint, shorePoints, 
                point => geometry.AdjacentPointsToPoint[point],
                getDrainCost);
            rivers.Add(newPath);
        }
        sourcePoints.TryParallel(drawRiver);
        return rivers.ToList();
    }
}
