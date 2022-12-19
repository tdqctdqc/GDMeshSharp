using Godot;
using System;

public class TriangleInfo 
{
    public TriRoughnessStat Roughness { get; private set; }
    public Faction Faction { get; private set; }
    
    public TerrainType TerrainType { get; private set; }
    public bool IsLand { get; private set; }
    public bool IsWater => IsLand == false;

    public Color GetColor()
    {
        return TerrainType != null
            ? TerrainType.Color
            : Colors.Gray;
    }
    public TriangleInfo()
    {
        Faction = null;
        TerrainType = Terrain.Proto;
        Roughness = new TriRoughnessStat();
        IsLand = false;
    }
    public void IncrementRoughness(float roughToAdd)
    {
        Roughness.AddIn(roughToAdd);
    }

    public void SetTerrain(TerrainType terrain)
    {
        TerrainType = terrain;
        IsLand = terrain.IsLand;
    }

    public void SetFaction(Faction fac)
    {
        Faction = fac;
    }

    public void SetIsLand(bool isLand)
    {
        IsLand = isLand;
    }
}
