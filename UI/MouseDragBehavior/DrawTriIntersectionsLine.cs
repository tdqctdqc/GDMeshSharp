using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class DrawTriIntersectionsLine : DrawTestLineMouseBehavior
{
    private static WorldManager _world;
    public DrawTriIntersectionsLine(int buttonIndex, 
        WorldManager world) : base(buttonIndex, Action)
    {
        _world = world;
    }

    private static void Action(Vector2 v, Vector2 w)
    {
        var result = _world.Geometry.GetLineSegmentTriIntersections(v, w);
        
        var trisOnLine = result.Item1;
        var points = result.Item2;
        Game.I.Graphics.DebugGraphics.DrawTris(trisOnLine, 
            Enumerable.Range(0 ,trisOnLine.Count)
                .Select(ColorsExt.GetRainbowColor).ToList());
        Game.I.Graphics.DebugGraphics.DrawZebraLine(points);
    }
}
