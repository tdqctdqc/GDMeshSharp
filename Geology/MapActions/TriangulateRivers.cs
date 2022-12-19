using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RiverTriangulation;

public class TriangulateRivers : MapAction
{
    public override void DoAction(WorldManager world)
    {
        var segments = GetSegments(world);
        InjectNewPoints(world, segments);
    }

    private Dictionary<Vector2, TriangulationSegment> GetSegments(WorldManager world)
    {
        var segments = new Dictionary<Vector2, TriangulationSegment>();
        var rivers = world.Rivers;
        var geometry = world.Geometry;
        var count = rivers.RiverEdgeKeys.Count;
        for (var i = 0; i < count; i++)
        {
            var edgeKey = rivers.RiverEdgeKeys[i];
            var fromPoint = (int) edgeKey.x;
            var toPoint = (int) edgeKey.y;
            var edges = geometry.EdgesBetweenPoints.GetEdge(fromPoint, toPoint);
            
            var lowerEdge = edges[0] < edges[1]
                ? edges[0]
                : edges[1];
            var upperEdge = lowerEdge == edges[0]
                ? edges[1]
                : edges[0];
            
            var segment = new TriangulationSegment();
            segment.LowerEdge = lowerEdge;
            segment.UpperEdge = upperEdge;
            segment.OldLowerFrom = geometry.From[lowerEdge];
            segment.OldLowerTo = geometry.To[lowerEdge];
            segments.Add(new Vector2(fromPoint, toPoint), segment);
        }
        return segments;
    }

    private void InjectNewPoints(WorldManager world, Dictionary<Vector2, TriangulationSegment> segments)
    {
        var geometry = world.Geometry;
        var rivers = world.Rivers;
        var joinPointsEnum = segments.Keys.Select(k => (int)k.x);
        joinPointsEnum = joinPointsEnum.Union(segments.Keys.Select(k => (int)k.y));
        var joinPoints = joinPointsEnum.Distinct().ToList();
        bool isARiverEdge(int point1, int point2)
        {
            return segments.ContainsKey(geometry.GetEdgeKey(point1, point2));
        }

        int iter = 0;
        int count = joinPoints.Count;
        void doJoin(int joinPoint)
        {
            iter++;
            var pos = geometry.PointsById[joinPoint];
            var adjPoints = geometry.AdjacentPointsToPoint[joinPoint];
            int firstRiverAdjPoint = -1;
            for (var i = 0; i < adjPoints.Count; i++)
            {
                var adjPoint = adjPoints[i];
                if (isARiverEdge(adjPoint, joinPoint) == false) continue;
                firstRiverAdjPoint = adjPoint;
                break;
            }
            if (firstRiverAdjPoint == -1) return;
            var firstRiverEdgeTo = geometry.GetEdgeBetweenPoints(firstRiverAdjPoint, joinPoint);
            
            var adjRiverToEdges = new List<int>{firstRiverEdgeTo};
            
            geometry.DoFan(firstRiverEdgeTo, joinPoint, true, 
                edge =>
                {
                    if (geometry.To[edge] != joinPoint) throw new Exception();
                    if (isARiverEdge(geometry.From[edge], joinPoint))
                    {
                        adjRiverToEdges.Add(edge);
                    }
                });
            geometry.DoFan(firstRiverEdgeTo, joinPoint, false,
                edge =>
                {
                    if (geometry.To[edge] != joinPoint) throw new Exception();

                    if (isARiverEdge(geometry.From[edge], joinPoint))
                    {
                        adjRiverToEdges.Insert(0, edge);
                    }
                });

            var riverEdgeCount = adjRiverToEdges.Count;
            var edgeJoinPoints = new List<int>();

            for (var i = 0; i < adjRiverToEdges.Count; i++)
            {
                edgeJoinPoints.Add(geometry.AddPoint(pos));
            }

            for (var i = 0; i < adjRiverToEdges.Count; i++)
            {
                var edge = adjRiverToEdges[i];
                var oEdge = geometry.HalfEdgePairs[edge];
                var index = adjRiverToEdges.IndexOf(edge);
                var prevJoinPoint = edgeJoinPoints[(index + riverEdgeCount - 1) % riverEdgeCount];
                var nextJoinPoint = edgeJoinPoints[index];
                var key = geometry.GetEdgeKey(geometry.To[edge], geometry.From[edge]);
                var segment = segments[key];
                
                if (joinPoint == segment.OldLowerTo)
                {
                    //then edge is lowerEdge
                    segment.LowerTo = nextJoinPoint;
                    segment.UpperFrom = prevJoinPoint;
                }
                else
                {
                    segment.UpperTo = nextJoinPoint;
                    segment.LowerFrom = prevJoinPoint;
                }
            }
        }
        
        foreach (var joinPoint in joinPoints)
        {
            doJoin(joinPoint);
        }
    }
}

namespace RiverTriangulation
{
    public class TriangulationSegment
    {
        public int OldLowerFrom,
            OldLowerTo,
            LowerEdge, 
            UpperEdge,
            
            NewLowerEdge,
            NewUpperEdge,
            LowerFrom,
            LowerTo,
            UpperFrom,
            UpperTo,
            
            LowerCrossEdge,
            UpperCrossEdge;
    }
}