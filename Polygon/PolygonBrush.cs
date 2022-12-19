using Godot;
using System;
using System.Collections.Generic;

public class PolygonBrush
{
    public TerrainType PaintingTerrain { get; private set; }

    public PolygonBrush()
    {
        PaintingTerrain = Terrain.ProtoLand; 
    }
    public void SetTerrain(TerrainType terrain)
    {
        PaintingTerrain = terrain;
    }
}
