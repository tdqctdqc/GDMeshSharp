using Godot;
using System;

public class DrawTestLineMouseBehavior : MouseDragBehavior
{
    private Vector2 _p1, _p2;
    private Action<Vector2, Vector2> _action;
    public DrawTestLineMouseBehavior(int buttonIndex, 
        Action<Vector2, Vector2> action)
        : base(buttonIndex)
    {
        _action = action;
    }

    protected override void ClickDown(InputEventMouseButton m)
    {
        _p1 = Game.I.Graphics.GetGlobalMousePosition();
    }

    protected override void ClickUp(InputEventMouseButton m)
    {
        _p2 = Game.I.Graphics.GetGlobalMousePosition();
        _action(_p1, _p2);
    }

    protected override void ClickHeld(Vector2 pos)
    {
    }
}
