using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class BuildRoads : MapAction
{
    private Stopwatch _sw;
    private int _maxSeconds = 3;
    private static float _roadBaseCost = 1f;
    public override void DoAction(WorldManager world)
    {
        _sw = new Stopwatch();
        var swLocal = new Stopwatch();
        var segments = BuildRoadSegments(world);
        world.Roads.AddRoads(segments);
        _sw.Stop();
    }
    private void CheckTime()
    {
        if (_sw.Elapsed.Seconds > _maxSeconds) throw new Exception();
    }
    private List<Vector2> BuildRoadSegments(WorldManager world)
    {
        var geometry = world.Geometry;
        var roadSegmentKeys = new ConcurrentDictionary<Vector2, byte>();
        float getPointsDist(int point1, int point2)
        {
            var pos1 = geometry.PointsById[point1];
            var pos2 = geometry.PointsById[point2];
            return pos1.GridDistanceTo(pos2);
        }

        bool pointHasOnlyOneLandTri(int point)
        {
            var edges = geometry.FromEdgesForPoint[point];
            bool foundFirst = false;
            for (var i = 0; i < edges.Count; i++)
            {
                var tri = geometry.TrianglesByEdgeIds[edges[i]];
                if (tri.Info.IsLand)
                {
                    if (foundFirst) return false;
                    foundFirst = true;
                }
            }
            return false;
        }
        float getRoadCost(int from, int to)
        {           
            var key = geometry.GetEdgeKey(from, to);
            if (roadSegmentKeys.ContainsKey(key))
            {
                return 0f;
            }
            if (world.Rivers.RiverWidths.ContainsKey(key))
            {
                return Mathf.Inf;
            }
            if (geometry.PointsAreOnOutsideEdge(from, to))
            {
                return Mathf.Inf;
            }

            var edge = geometry.GetFirstEdgeBetweenPointsDirectionless(from, to);
            
            if (geometry.HalfEdgePairs[edge] is int oEdge)
            {
                var tri = geometry.TrianglesByEdgeIds[edge];
                var oTri = geometry.TrianglesByEdgeIds[oEdge];
                if (tri.Info.IsWater || oTri.Info.IsWater)
                {
                    return Mathf.Inf;
                }
                return _roadBaseCost + (tri.Info.Roughness.OutValue + oTri.Info.Roughness.OutValue);
            }
            return Mathf.Inf;
        }
        List<int> getPath(int from, int to)
        {
            var path = PathFinder<int>.FindPath(
                from, to, 
                p => geometry.AdjacentPointsToPoint[p],
                getRoadCost,
                getPointsDist,
                1000
            );
            return path;
        }
        float maxRoadDist = 500f;
        int maxRoadSegmentLength = 100;
        void drawRoadsForUrbanGraph(Graph<Triangle, float> urbanGraph)
        {
            var nodes = urbanGraph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                
                var neighbors = node.Neighbors;
                var count = node.Neighbors.Count;
                for (int j = 0; j < count; j++)
                {
                    var n = neighbors[j];;
                    var nNode = urbanGraph[n];
                    
                    if (node.Element.Id < nNode.Element.Id) continue;
                    var fromPoint = node.Element.Points[0];
                    var toPoint = nNode.Element.Points[0];

                    var dist = getPointsDist(fromPoint, toPoint);
                    if (dist > maxRoadDist) continue;
                    if (pointHasOnlyOneLandTri(fromPoint))
                    {
                        fromPoint = node.Element.Points[1];
                    }
                    
                    if (pointHasOnlyOneLandTri(toPoint))
                    {
                        //bc otherwise wont build road
                        toPoint = nNode.Element.Points[1];
                    }
                    
                    var path = getPath(fromPoint, toPoint);
                    if (path == null) continue;
                    if (path.Count > maxRoadSegmentLength) continue;
                    var pathKeys = new List<Vector2>();
                    for (int k = 0; k < path.Count - 1; k++)
                    {
                        var key = geometry.GetEdgeKey(path[k], path[k + 1]);
                        pathKeys.Add(key);
                        roadSegmentKeys.TryAdd(key, new byte());
                    }

                    var loc = world.Locations.TriangleLocations[node.Element];
                    var nLoc = world.Locations.TriangleLocations[nNode.Element];
                    if (world.Locations.LocationGraph[loc].Neighbors.Contains(nLoc))
                    {
                        var edge = world.Locations.LocationGraph.GetEdge(loc, nLoc);
                        edge.SetRoadPath(pathKeys);
                        edge.LandEdge = true;
                    }
                }
            }
        }
        world.Landmasses.LandmassUrbanGraphs.TryParallel(drawRoadsForUrbanGraph);
        return roadSegmentKeys.Keys.ToList();
    }
    
    
}
