using Godot;
using System;

public class TerrainType 
{
    public Color Color { get; private set; }
    public string Name { get; private set; }
    public bool IsLand { get; private set; }
    public TerrainType(string name, Color color, bool isLand)
    {
        Color = color;
        Name = name;
        IsLand = isLand;
    }
}
