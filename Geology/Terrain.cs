using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Terrain
{
    public static TerrainType Proto { get; private set; }
    public static TerrainType ProtoWater { get; private set; }
    public static TerrainType ProtoLand { get; private set; }
    public static TerrainType Plains { get; private set; }
    public static TerrainType LowMountains { get; private set; }
    public static TerrainType HighMountains { get; private set; }
    public static TerrainType Hills { get; private set; }
    public static TerrainType LightUrban { get; private set; }
    public static TerrainType DenseUrban { get; private set; }
    public static List<TerrainType> Terrains { get; private set; }
    
    static Terrain()
    {
        SetupTerrains();
    }

    private static void SetupTerrains()
    {
        Proto = new TerrainType("Proto", Colors.Purple, true);
        ProtoWater = new TerrainType("Proto Water", Colors.Cyan, false);
        ProtoLand = new TerrainType("Proto Land", Colors.Orange, true);
        HighMountains = new TerrainType("High Mountains", Colors.Gray, true);
        LowMountains = new TerrainType("Low Mountains", Colors.Brown, true);
        Hills = new TerrainType("Hills", Colors.OliveDrab, true);
        Plains = new TerrainType("Plains", Colors.Limegreen, true);
        LightUrban = new TerrainType("Light Urban", Colors.Red, true);
        DenseUrban = new TerrainType("Plains", Colors.Black, true);
            
        Terrains = new List<TerrainType> { ProtoWater, ProtoLand };
    }

    public static TerrainType GetTerrainByRoughness(float roughness)
    {
        if (roughness > .65f) return HighMountains;
        if (roughness > .35f) return LowMountains;
        if (roughness > .10f) return Hills;
        return Plains;
    }
}
