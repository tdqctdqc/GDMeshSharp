using Godot;
using System;

public class FragmentPolygon : PolygonSelfAction
{
    public override void PerformAction(Polygon poly, WorldManager world)
    {
        GeologyUtility.FragmentPolygon(poly, world, 10f);
    }
}
