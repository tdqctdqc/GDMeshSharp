using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Plate 
{
    public Vector2 Drift { get; private set; }
    public int Id { get; private set; }
    public List<Polygon> Cells { get; private set; }
    public bool IsLand { get; private set; }

    public Plate(int id, List<Polygon> cells)
    {
        Id = id;
        Cells = cells;
        var driftAngle = Game.Random.RandfRange(0f, Mathf.Pi * 2f);
        var driftStrength = Game.Random.RandfRange(0f, 1f);
        Drift = Vector2.Up.Rotated(driftAngle) * driftStrength;
    }
    public void AddCell(Polygon cell)
    {
        Cells.Add(cell);
    }
    public void AddCells(List<Polygon> cells)
    {
        Cells.AddRange(cells);
    }
    public void SetIsLand(bool isLand)
    {
        IsLand = isLand;
    }

    public Vector2 GetCenter(GeometryManager geometry)
    {
        return Cells.Select(c => c.GetCenter(geometry)).Aggregate((v, w) => v + w) / Cells.Count;
    }

    public List<int> GetBorderEdges(WorldManager world)
    {
        var geometry = world.Geometry;
        var polygons = world.GeologyPolygons;
        bool edgeIsBorder(int edge)
        {
            if (geometry.HalfEdgePairs[edge] is int o)
            {
                var oTri = geometry.TrianglesByEdgeIds[o];
                var poly = polygons.TrianglePolygons[oTri];
                return Cells.Contains(poly) == false;
            }
            else
            {
                return true;
            }
        }

        var borderEdges = GetBorderCells(geometry, polygons)
            .SelectMany(c => c.BorderEdges)
            .Where(e => edgeIsBorder(e))
            .ToList();
        return borderEdges;
    }

    public List<int> GetBorderEdgesWithPlate(Plate oPlate, WorldManager world)
    {
        var plates = world.Plates;
        var edges = new List<int>();
        var borderCells = GetBorderCells(world.Geometry, world.GeologyPolygons);
        foreach (var cell in borderCells)
        {
            var nPolysInPlate = cell.GetAdjacentPolygons(world.Geometry)
                .Where(nP => plates.CellPlates[nP] == oPlate);
            foreach (var nPoly in nPolysInPlate)
            {
                edges.AddRange(cell.GetEdgesBorderingPolygon(nPoly, world.Geometry));
            }
        }
        return edges;
    }
    public List<Polygon> GetBorderCells(GeometryManager geometry, PolygonManager polygons)
    {
        return Cells
            .Where(c => c.GetAdjacentPolygons(geometry)
                .Where(n => Cells.Contains(n) == false).Count() > 0)
            .ToList();
    }
    public List<Plate> GetAdjacentPlates(WorldManager world)
    {
        return GetBorderCells(world.Geometry, world.GeologyPolygons)
            .SelectMany(world.GeologyPolygons.Edges.GetNeighbors)
            .Distinct()
            .Where(nP => world.Plates.CellPlates[nP] != this)
            .Select(nP => world.Plates.CellPlates[nP])
            .Distinct()
            .ToList();
    }
}
