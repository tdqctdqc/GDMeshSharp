using Godot;
using System;
using System.Collections.Generic;

public class LocationEdge 
{
    public Location Location1 {get; private set;}
    public Location Location2 {get; private set;}
    public List<Vector2> RoadPathKeys {get; private set;}
    public bool LandEdge { get; set; }

    public LocationEdge(Location loc1, Location loc2)
    {
        Location1 = loc1;
        Location2 = loc2;
    }

    public void SetRoadPath(List<Vector2> roadKeys)
    {
        RoadPathKeys = roadKeys;
    }
}
