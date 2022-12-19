using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DrawMultiLineMouseBehavior : MouseDragBehavior
{
    private WorldManager _world;
    private List<Vector2> _points;
    private static float _minDist = 25f;
    
    public DrawMultiLineMouseBehavior(WorldManager world, int buttonIndex) 
        : base(buttonIndex)
    {
        _world = world;
    }

    protected abstract void LineAction(List<Vector2> points);
    protected override void ClickDown(InputEventMouseButton m)
    {
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        if (_world.Geometry.PointOutOfBounds(mousePos)) return;
        _points = new List<Vector2>();
        _points.Add(mousePos);
    }

    protected override void ClickUp(InputEventMouseButton m)
    {
        if (_points == null) return;
        if (_world.Geometry.PointOutOfBounds(_points[0]))
            return;
        if (_points.Count > 1 && _world.Geometry.PointOutOfBounds(_points[1]))
            return;
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        if (mousePos.DistanceTo(_points.Last()) > _minDist
            || _points.Count == 1)
        {
            _points.Add(mousePos);
        }


        if (_points.Count > 1) LineAction(_points);

    }

    protected override void ClickHeld(Vector2 pos)
    {
        if (_points == null) return;
        if (pos.DistanceTo(_points.Last()) > _minDist)
        {
            _points.Add(pos);
        }
    }
}
