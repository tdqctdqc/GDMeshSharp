using Godot;
using System;
using System.Collections.Generic;

public class DrawPolygonMouseBehavior : MouseDragBehavior
{
    private Polygon _poly;
    private TerrainType _terrain;
    private WorldManager _world;
    private List<Triangle> _painted; 

    public DrawPolygonMouseBehavior(int buttonIndex, WorldManager world)
        : base(buttonIndex)
    {
        _world = world;
    }

    protected override void ClickDown(InputEventMouseButton m)
    {
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        var tri = _world.Geometry.TriLookup.GetTriAtPosition(mousePos, _world.Geometry);
        if (tri == null) return;
        _painted = new List<Triangle> {tri};

        _terrain = Game.I.PolyBrush.PaintingTerrain;
        if (_world.GeologyPolygons.TrianglePolygons.ContainsKey(tri))
        {
            _poly = _world.GeologyPolygons.TrianglePolygons[tri];
        }
        else
        {
            _poly = _world.GeologyPolygons.AddNewPolygonWithTris(_painted);
        }
    }

    protected override void ClickUp(InputEventMouseButton m)
    {
        _poly = null;
        _terrain = null;
        _painted = null;
    }

    protected override void ClickHeld(Vector2 pos)
    {
        if (_poly == null) return;
        var tri = _world.Geometry.TriLookup.GetTriAtPosition(pos, _world.Geometry);
        if (tri == null || _painted.Contains(tri)) return;
        _world.GeologyPolygons.TransferTriToPolygon(_poly, tri);
        _painted.Add(tri);
    }
}
