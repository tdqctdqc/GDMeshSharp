using Godot;
using System;
using System.Collections.Generic;

public class RiverManager
{
    public Dictionary<Vector2, float> RiverWidths { get; private set; }
    public List<Vector2> RiverEdgeKeys { get; private set; }
    private WorldManager _world;
    public RiverManager(WorldManager world)
    {
        _world = world;
        RiverWidths = new Dictionary<Vector2, float>();
        RiverEdgeKeys = new List<Vector2>();
    }

    public void AddRivers(List<List<int>> riversByPoint)
    {
        foreach (var river in riversByPoint)
        {
            for (int i = 0; i < river.Count - 1; i++)
            {
                var from = river[i];
                var to = river[i + 1];
                var key = _world.Geometry.GetEdgeKey(from, to);
                if (RiverEdgeKeys.Contains(key) == false)
                {
                    RiverEdgeKeys.Add(key);
                    RiverWidths.Add(key, 0f);
                }

                RiverWidths[key] += 1f;
            }
        }
    }
}
