using Godot;
using System;
using System.Collections.Generic;

public class RoadGraphics : Node2D
{
    private WorldManager _world;
    private MeshInstance2D _roadMesh;
    public void Setup(WorldManager world)
    {
        _world = world;
    }

    public void Start()
    {
        _roadMesh?.Free();
        var geometry = _world.Geometry;
        var roads = _world.Roads;
        if (roads.RoadEdgeKeys.Count == 0) return;
        
        var froms = new List<Vector2>();
        var tos = new List<Vector2>();
        foreach (var key in roads.RoadEdgeKeys)
        {
            var from = (int) key.x;
            var to = (int) key.y;
            froms.Add(new Vector2(geometry.PointsById[from]));
            tos.Add(new Vector2(geometry.PointsById[to]));
        }

        _roadMesh = MeshGenerator.GetLinesMesh(froms, tos, 2f, geometry);
        _roadMesh.Modulate = Colors.Gray;
        AddChild(_roadMesh);
    }

    public void Stop()
    {
    }
    
}
