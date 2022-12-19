using Godot;
using System;
using System.Collections.Generic;

public class RoadManager
{
    public List<Vector2> RoadEdgeKeys { get; private set; }
    private WorldManager _world;

    public RoadManager(WorldManager world)
    {
        _world = world;
        RoadEdgeKeys = new List<Vector2>();
    }

    public void AddRoads(List<Vector2> roadEdgeKeys)
    {
        RoadEdgeKeys.AddRange(roadEdgeKeys);
    }
}
