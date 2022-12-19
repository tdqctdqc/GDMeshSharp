using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class LineSegmentLookup
{
    public Dictionary<int, Vector2> From { get; private set; }
    public Dictionary<int, Vector2> To { get; private set; }
    private Dictionary<Vector2, List<int>> _segmentsByCellCoords;
    private Dictionary<int, List<Vector2>> _segmentCellCoords;
    private float _cellSize;

    public LineSegmentLookup(float cellSize)
    {
        _cellSize = cellSize;
        From = new Dictionary<int, Vector2>();
        To = new Dictionary<int, Vector2>();
        _segmentCellCoords = new Dictionary<int, List<Vector2>>();
        _segmentsByCellCoords = new Dictionary<Vector2, List<int>>();
    }

    public float GetDistToClosestEdge(Vector2 point)
    {
        if (_segmentCellCoords.Count == 0) return Mathf.Inf;

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
                    if (_segmentsByCellCoords.ContainsKey(coord))
                    {
                        _segmentsByCellCoords[coord].ForEach(e => edges.Add(e));
                    }
                }
            }
        }
        return edges
            .Max(e => point.DistFromLineSegmentToPoint(From[e], To[e]));
    }
    public void AddSegment(int id, Vector2 from, Vector2 to)
    {
        From.Add(id, from);
        To.Add(id, to);
        var maxX = Mathf.Max(from.x, to.x);
        var minX = Mathf.Min(from.x, to.x);
        var maxY = Mathf.Max(from.y, to.y);
        var minY = Mathf.Min(from.y, to.y);
        var maxCorner = GetCellCoordsFromPosition(new Vector2(maxX, maxY));
        var minCorner = GetCellCoordsFromPosition(new Vector2(minX, minY));
        _segmentCellCoords.Add(id, new List<Vector2>());

        for (int i = Mathf.FloorToInt(minCorner.x); i < Mathf.CeilToInt(maxCorner.x + 1); i++)
        {
            for (int j = Mathf.FloorToInt(minCorner.y); j < Mathf.CeilToInt(maxCorner.y + 1); j++)
            {
                var coord = new Vector2(i, j);
                if (_segmentsByCellCoords.ContainsKey(coord) == false)
                {
                    _segmentsByCellCoords.Add(coord, new List<int>());
                }
                _segmentCellCoords[id].Add(coord);
                _segmentsByCellCoords[coord].Add(id);
            }
        }
    }

    public void RemoveSegment(int id)
    {
        From.Remove(id);
        To.Remove(id);
        if (_segmentCellCoords.ContainsKey(id) == false) return;
        var coords = _segmentCellCoords[id];
        foreach (var coord in coords)
        {
            _segmentsByCellCoords[coord].Remove(id);
        }

        _segmentCellCoords.Remove(id);
    }
    
    private Vector2 GetCellCoordsFromPosition(Vector2 pos)
    {
        var x = Mathf.FloorToInt(pos.x / _cellSize);
        var y = Mathf.FloorToInt(pos.y / _cellSize);
        return new Vector2(x, y);
    }
}
