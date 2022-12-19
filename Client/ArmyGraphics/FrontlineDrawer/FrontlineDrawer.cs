using Godot;
using System;
using System.Collections.Generic;

public class FrontlineDrawer : Line2D
{
    public override void _Ready()
    {
        Width = 10f;
        DefaultColor = Colors.Black;
    }

    public void DrawFrontline(WorldManager world, List<Triangle> tris)
    {
        ClearPoints();
        for (var i = 0; i < tris.Count; i++)
        {
            AddPoint(tris[i].GetCentroid(world.Geometry));
        }
    }
    public void DrawFrontline(WorldManager world, Frontline frontline)
    {
        ClearPoints();
        for (var i = 0; i < frontline.Points.Count; i++)
        {
            AddPoint(frontline.Points[i]);
        }
    }
}
