using Godot;
using System;
using System.Collections.Generic;

public class PolygonTerrainInfo 
{
    public bool IsLand { get; private set; }
    public PolygonTerrainInfo()
    {
        IsLand = false;
    }
    public void SetIsLand(bool isLand)
    {
        IsLand = isLand;
    }
}
