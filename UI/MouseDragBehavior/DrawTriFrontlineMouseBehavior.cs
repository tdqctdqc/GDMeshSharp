using Godot;
using System;
using System.Collections.Generic;

public class DrawTriFrontlineMouseBehavior : MouseDragBehavior
{
    private WorldManager _world;
    private List<Triangle> _frontlineTris;
    public DrawTriFrontlineMouseBehavior(WorldManager world, int buttonIndex) 
        : base(buttonIndex)
    {
        _world = world;
        _frontlineTris = new List<Triangle>();
        
    }

    protected override void ClickDown(InputEventMouseButton m)
    {
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        var tri = _world.Geometry.TriLookup.GetTriAtPosition(mousePos, _world.Geometry);
        if (tri == null) return;
        _frontlineTris = new List<Triangle>();
    }

    protected override void ClickUp(InputEventMouseButton m)
    {
        if (Game.I.Ui.ArmyUi.SelectedArmy == null) return;
        var army = Game.I.Ui.ArmyUi.SelectedArmy;
        // if(_frontlineTris.Count > 1) _world.Armies.SetArmyFrontline(_world, army, _frontlineTris);
    }

    protected override void ClickHeld(Vector2 pos)
    {
        var tri = _world.Geometry.TriLookup.GetTriAtPosition(pos, _world.Geometry);
        if (tri == null || _frontlineTris.Contains(tri)) return;
        if (_frontlineTris.Count > 0
            && _frontlineTris[_frontlineTris.Count - 1]
                .GetTrisSharingPoint(_world.Geometry)
                .Contains(tri) == false)
        {
            return;
        }
        _frontlineTris.Add(tri);

        Game.I.Graphics.ArmyGraphics.FrontlineDrawer.DrawFrontline(_world, _frontlineTris);
    }
}
