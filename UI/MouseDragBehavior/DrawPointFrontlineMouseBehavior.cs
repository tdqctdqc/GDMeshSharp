using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class DrawPointFrontlineMouseBehavior : DrawMultiLineMouseBehavior
{
    private WorldManager _world;
    
    public DrawPointFrontlineMouseBehavior(WorldManager world, int buttonIndex) 
        : base(world, buttonIndex)
    {
        _world = world;
    }

    protected override void LineAction(List<Vector2> points)
    {
        if (Game.I.Ui.ArmyUi.SelectedArmy == null) return;
        var army = Game.I.Ui.ArmyUi.SelectedArmy;
        // if(points.Count > 1) 
            // _world.Armies.SetArmyFrontline(_world, army, points);
    }
}
