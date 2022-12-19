using Godot;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

public class EdgeLookup 
{
    private Dictionary<Vector2, List<int>> _edgesByCellCoords;
    private Dictionary<int, List<Vector2>> _edgeCellCoords;
    private float _cellSize;

    public EdgeLookup(float cellSize)
    {
        _cellSize = cellSize;
        _edgeCellCoords = new Dictionary<int, List<Vector2>>();
        _edgesByCellCoords = new Dictionary<Vector2, List<int>>();
    }

    public void LoadEdges(GeometryManager geometry)
    {
        foreach (var entry in geometry.From)
        {
            var edge = entry.Key;
            AddEdge(edge, geometry);
        }
    }

    public void UpdateEdge(int edgeId, GeometryManager geometry)
    {
        if(_edgeCellCoords.ContainsKey(edgeId)) RemoveEdge(edgeId);
        AddEdge(edgeId, geometry);
    }
    public void AddEdge(int edgeId, GeometryManager geometry)
    {
        var points = geometry.GetEdgeFromAndToPoints(edgeId);
        var maxX = Mathf.Max(points.from.x, points.to.x);
        var minX = Mathf.Min(points.from.x, points.to.x);
        var maxY = Mathf.Max(points.from.y, points.to.y);
        var minY = Mathf.Min(points.from.y, points.to.y);
        var maxCorner = GetCellCoordsFromPosition(new Vector2(maxX, maxY));
        var minCorner = GetCellCoordsFromPosition(new Vector2(minX, minY));
        _edgeCellCoords.Add(edgeId, new List<Vector2>());

        for (int i = Mathf.FloorToInt(minCorner.x); i < Mathf.CeilToInt(maxCorner.x + 1); i++)
        {
            for (int j = Mathf.FloorToInt(minCorner.y); j < Mathf.CeilToInt(maxCorner.y + 1); j++)
            {
                var coord = new Vector2(i, j);
                if (_edgesByCellCoords.ContainsKey(coord) == false)
                {
                    _edgesByCellCoords.Add(coord, new List<int>());
                }
                _edgeCellCoords[edgeId].Add(coord);
                _edgesByCellCoords[coord].Add(edgeId);
            }
        }
    }

    public void RemoveEdge(int edgeId)
    {
        if (_edgeCellCoords.ContainsKey(edgeId) == false) return;
        var coords = _edgeCellCoords[edgeId];
        foreach (var coord in coords)
        {
            _edgesByCellCoords[coord].Remove(edgeId);
        }

        _edgeCellCoords.Remove(edgeId);
    }
    private Vector2 GetCellCoordsFromPosition(Vector2 pos)
    {
        var x = Mathf.FloorToInt(pos.x / _cellSize);
        var y = Mathf.FloorToInt(pos.y / _cellSize);
        return new Vector2(x, y);
    }
    public int? GetClosestEdge(Vector2 point, GeometryManager geometry)
    {
        if (_edgeCellCoords.Count == 0) return null;

        if (geometry.TriLookup.GetTriAtPosition(point, geometry) is Triangle tri)
        {
            return tri.HalfEdges.OrderBy(e => GetDistFromEdge(point, e, geometry)).First();
        }
        //not really closest for now
        var edges = new HashSet<int>();
        var startCoord = GetCellCoordsFromPosition(point);
        int iter = 0;
        
        while (edges.Count == 0)
        {
            iter++;
            for (int i = -iter; i < iter + 1; i++)
            {
                for (int j = -iter; j < iter + 1; j++)
                {
                    var coord = startCoord + new Vector2(i, j);
                    if (_edgesByCellCoords.ContainsKey(coord))
                    {
                        _edgesByCellCoords[coord].ForEach(e => edges.Add(e));
                    }
                }
            }
        }

        var first = edges.OrderBy(e => GetDistFromEdge(point, e, geometry)).First();
        if (geometry.HalfEdgePairs[first] is int o)
        {
            var fromTo = geometry.GetEdgeFromAndToPoints(o);
            if (point.PointIsLeftOfLine(fromTo.from, fromTo.to) == false)
            {
                return o;
            }
        }
        return first;
    }

    public bool Contains(int edge)
    {
        return _edgeCellCoords.ContainsKey(edge);
    }
    private float GetDistFromEdge(Vector2 point, int edgeId, GeometryManager geometry)
    {
        var fromTo = geometry.GetEdgeFromAndToPoints(edgeId);
        return point.DistFromLineSegmentToPoint(fromTo.from, fromTo.to);
    }
}
