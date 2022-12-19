using Godot;
using System;
using System.Collections.Generic;

public class PlateManager 
{
    public List<Plate> Plates { get; private set; }
    public Graph<Plate, float> Graph { get; private set; }
    public Dictionary<Polygon, Plate> CellPlates { get; private set; }
    private int _idCounter;
    public Graph<Plate, PlateEdgeInfo> Edges { get; private set; }
    public PlateManager()
    {
        _idCounter = 0;
        CellPlates = new Dictionary<Polygon, Plate>();
        Plates = new List<Plate>();
    }

    public Plate AddPlateWithCells(List<Polygon> cells)
    {
        var plate = new Plate(TakeId(), cells);
        Plates.Add(plate);
        foreach (var c in cells)
        {
            CellPlates.Add(c, plate);
        }

        return plate; 
    }
    public void AddCellToPlate(Polygon cell, Plate plate)
    {
        plate.AddCell(cell);
        CellPlates.Add(cell, plate);
    }
    public void AddCellsToPlate(List<Polygon> cells, Plate plate)
    {
        plate.AddCells(cells);
        foreach (var c in cells)
        {
            CellPlates.Add(c, plate);
        }
    }

    public void BuildGraph(WorldManager world)
    {
        Edges = new Graph<Plate, PlateEdgeInfo>();
        Graph = new Graph<Plate, float>();
        for (var i = 0; i < Plates.Count; i++)
        {
            Graph.AddNode(Plates[i]);
        }

        for (var i = 0; i < Plates.Count; i++)
        {
            var plate = Plates[i];
            var neighbors = plate.GetAdjacentPlates(world);
            for (var j = 0; j < neighbors.Count; j++)
            {
                var n = neighbors[j];
                if (Edges.HasEdge(plate, n) == false)
                {
                    Edges.AddEdge(plate, n, new PlateEdgeInfo());
                    Graph.AddUndirectedEdge(plate, n, 1f);
                }
            }
        }
    }
    private int TakeId()
    {
        _idCounter++;
        return _idCounter;
    }
}
