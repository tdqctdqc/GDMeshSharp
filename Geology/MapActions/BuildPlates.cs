using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
public class BuildPlates : MapAction
{
    private int _numPlates, _cellsPerPlate;
    private float _percentLand;
    private Vector2 _dimensions;

    public BuildPlates(int numPlates, int cellsPerPlate, Vector2 dimensions,
        float percentLand)
    {
        _numPlates = numPlates;
        _cellsPerPlate = cellsPerPlate;
        _dimensions = dimensions;
        _percentLand = percentLand;
    }
    public override void DoAction(WorldManager world)
    {
        RunAndTime(world);
    }

    private void Run(WorldManager world)
    {
        var numCells = _numPlates * _cellsPerPlate;
        world.GeologyPolygons.Clear();

        var cellTris = GetCells(world, numCells);
        
        BuildPolygonsFromCellTris(world, cellTris);
        world.GeologyPolygons.BuildGraph(world);
        
        PlatesPickCells(world);
        world.Plates.BuildGraph(world);

        SetPlatesLandOrWater(world);
    }
    private void RunAndTime(WorldManager world)
    {
        var numCells = _numPlates * _cellsPerPlate;
        world.GeologyPolygons.Clear();

        var cellTris = GetCells(world, numCells);

        BuildPolygonsFromCellTris(world, cellTris);
        
        world.GeologyPolygons.BuildGraph(world);

        PlatesPickCells(world);
        
        SetPlatesLandOrWater(world);
        
        world.Plates.BuildGraph(world);
    }
    
    private List<List<Triangle>> GetCells(WorldManager world, int numCells)
    {
        var geometry = world.Geometry;
        var margin = .1f;
        var cells = new ConcurrentDictionary<Vector2, ConcurrentBag<Triangle>>();
        var cellPoses = new List<Vector2>();
        var cellTriHash = new HashSet<Triangle>();
        
        for (int i = 0; i < numCells; i++)
        {
            var point = new Vector2(
                Game.Random.RandfRange(margin, _dimensions.x - margin),
                Game.Random.RandfRange(margin, _dimensions.y - margin)
            );
            if (cells.ContainsKey(point) == false)
            {
                cellPoses.Add(point);
                cells.TryAdd(point, new ConcurrentBag<Triangle>());
            }
        }
        
        
        void registerTri(Triangle tri)
        {
            var pos = tri.GetCentroid(geometry);
            var closeCellSeedTri = cellPoses
                    .OrderBy(p => p.DistanceSquaredTo(pos))
                    .First();
            cells[closeCellSeedTri].Add(tri);
        }
        Parallel.ForEach(geometry.Triangles, registerTri);
        return cells.Values.Select(b => b.ToList()).ToList();
    }

    private void BuildPolygonsFromCellTris(WorldManager world,
        List<List<Triangle>> cellTris)
    {
        var polygons = world.GeologyPolygons;
        foreach (var entry in cellTris)
        {
            if (entry.Count == 0)
            {
                continue;
            }
            var poly = polygons.AddNewPolygonWithTris(entry);
        }
    }

    private void PlatesPickCells(WorldManager world)
    {
        var polygons = world.GeologyPolygons;
        var plates = world.Plates;
        var geometry = world.Geometry;

        List<Polygon> getAdjacentPolys(Polygon poly)
        {
            return polygons.Edges[poly].Neighbors;
        }
        
        var cellsToPick = polygons.Polygons.ToHashSet();
                
        for (int i = 0; i < _numPlates; i++)
        {
            var rand = cellsToPick.GetRandomElement();
            cellsToPick.Remove(rand);
            plates.AddPlateWithCells(new List<Polygon> {rand});
        }
        var openPlates = plates.Plates.ToList();

        while (cellsToPick.Count > 0)
        {
            var plate = openPlates[0];
            var borderPolys = plate.Cells
                .SelectMany(getAdjacentPolys)
                .Where(nP => cellsToPick.Contains(nP));
            var borderPolyCount = borderPolys.Count();
            if (borderPolyCount == 0)
            {
                openPlates.RemoveAt(0);
            }
            else
            {
                var rand = Game.Random.RandiRange(0, borderPolyCount - 1);
                var pick = borderPolys.ElementAt(rand);
                if (plates.CellPlates.ContainsKey(pick)) continue;
                plates.AddCellToPlate(pick, plate);
                cellsToPick.Remove(pick);
                openPlates.RemoveAt(0);
                openPlates.Add(plate);
            }
        }
    }


    private void SetPlatesLandOrWater(WorldManager world)
    {
        var polygons = world.GeologyPolygons;
        var plates = world.Plates;
        var plateIsLand = Enumerable.Range(0, plates.Plates.Count)
            .Select(i =>
                Game.Random.RandfRange(0f, 1f) < _percentLand
                    ? true
                    : false
            )
            .ToList();
        foreach (var plate in plates.Plates)
        {
            var index = plates.Plates.IndexOf(plate);
            plate.SetIsLand(plateIsLand[index]);
            foreach (var cell in plate.Cells)
            {
                polygons.ChangePolygonIsLand(cell, plateIsLand[index]);
            }
        }
    }
}

