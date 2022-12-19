using Godot;
using System;
using System.Collections.Generic;

public class RiverGraphics : Node2D
{
    private WorldManager _world;
    private MeshInstance2D _riverMesh;
    public void Setup(WorldManager world)
    {
        _world = world;
    }

    public void Start()
    {
        _riverMesh?.Free();
        var geometry = _world.Geometry;
        var rivers = _world.Rivers;
        if (rivers.RiverEdgeKeys.Count == 0) return;
        
        var froms = new List<Vector2>();
        var tos = new List<Vector2>();
        var widths = new List<float>();
        foreach (var entry in rivers.RiverWidths)
        {
            var key = entry.Key;
            var from = (int) key.x;
            var to = (int) key.y;
            froms.Add(new Vector2(geometry.PointsById[from]));
            tos.Add(new Vector2(geometry.PointsById[to]));
            var width = entry.Value;
            widths.Add(5f);
        }

        _riverMesh = MeshGenerator.GetLinesMeshCustomWidths(froms, tos, widths, geometry);
        _riverMesh.Modulate = Colors.Blue;
        AddChild(_riverMesh);
    }

    public void Stop()
    {
    }
}
