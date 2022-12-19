using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TriangleLookup
{
    private int minXCoord, minYCoord, maxXCoord, maxYCoord;
    private Dictionary<Vector2, List<Triangle>> _trisByCellCoords;
    private Dictionary<Triangle, List<Vector2>> _triCellCoords;
    private float _cellSize;

    public TriangleLookup(float cellSize, GeometryManager geometry)
    {
        _cellSize = cellSize;
        _trisByCellCoords = new Dictionary<Vector2, List<Triangle>>();
        _triCellCoords = new Dictionary<Triangle, List<Vector2>>();
        foreach (var tri in geometry.Triangles)
        {
            AddTriangle(tri, geometry);
        }
    }

    public void UpdateTriangle(Triangle tri, GeometryManager geometry)
    {
        OverwriteCoords(tri, geometry);
    }
    public void AddTriangle(Triangle tri, GeometryManager geometry)
    {
        WriteCoords(tri, geometry);
    }

    private void OverwriteCoords(Triangle tri, GeometryManager geometry)
    {
        var oldTriCellCoords = _triCellCoords[tri];
        foreach (var triCellCoord in oldTriCellCoords)
        {
            _trisByCellCoords[triCellCoord].Remove(tri);
        }
        _triCellCoords.Remove(tri);
        
        WriteCoords(tri, geometry);
    }

    private void WriteCoords(Triangle tri, GeometryManager geometry)
    {
        var centroid = tri.GetCentroid(geometry);
        var maxExtentFromCentroid =
            tri.GetMaxOfPoints(p => geometry.PointsById[p].DistanceTo(centroid));  
            
        var maxExtentInCells = Mathf.CeilToInt(maxExtentFromCentroid / _cellSize);
        var homeCoord = GetCellCoordsFromPosition(centroid);
        var triCellCoords = new List<Vector2>();
        for (int i = -maxExtentInCells; i < maxExtentInCells + 1; i++)
        {
            for (int j = -maxExtentInCells; j < maxExtentInCells + 1; j++)
            {
                var coord = homeCoord + new Vector2(i, j);
                SetMaxAndMin(coord);
                if (_trisByCellCoords.ContainsKey(coord) == false)
                {
                    _trisByCellCoords.Add(coord, new List<Triangle>());
                }
                _trisByCellCoords[coord].Add(tri);
                triCellCoords.Add(coord);
            }
        }
        _triCellCoords.Add(tri, triCellCoords);
    }

    private void SetMaxAndMin(Vector2 coord)
    {
        minXCoord = Mathf.FloorToInt(Mathf.Min(coord.x, minXCoord));
        minYCoord = Mathf.FloorToInt(Mathf.Min(coord.y, minYCoord));
        maxXCoord = Mathf.CeilToInt(Mathf.Max(coord.x, maxXCoord));
        maxYCoord = Mathf.CeilToInt(Mathf.Max(coord.y, maxYCoord));
    }
    public void RemoveTriangle(Triangle tri, GeometryManager geometry)
    {
        if (_triCellCoords.ContainsKey(tri) == false) return;

        var coords = _triCellCoords[tri];
        foreach (var coord in coords)
        {
            _trisByCellCoords[coord].Remove(tri);
        }

        _triCellCoords.Remove(tri);
    }
    private Vector2 GetCellCoordsFromPosition(Vector2 pos)
    {
        var x = Mathf.FloorToInt(pos.x / _cellSize);
        var y = Mathf.FloorToInt(pos.y / _cellSize);
        return new Vector2(x, y);
    }
    public Triangle GetTriAtPosition(Vector2 position, GeometryManager geometry)
    {
        var cellCoords = GetCellCoordsFromPosition(position);
        if (_trisByCellCoords.ContainsKey(cellCoords))
        {
            var tris = _trisByCellCoords[cellCoords];
            foreach (var tri in tris)
            {
                if (tri.PointInsideTriangle(position, geometry)) return tri;
            }
        }
        return null;
    }
}
