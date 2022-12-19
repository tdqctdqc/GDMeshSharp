using Godot;
using System;

public abstract class MouseDragBehavior
{
    private bool _pressed;
    private int _buttonIndex;

    public MouseDragBehavior(int buttonIndex)
    {
        _buttonIndex = buttonIndex;
    }
    public void HandleInput(InputEventMouseButton m)
    {
        if (m.ButtonIndex != _buttonIndex) return;
        if (_pressed == false && m.Pressed == true)
        {
            ClickDown(m);
            _pressed = true;
        }
        else if (_pressed == true && m.Pressed == false)
        {
            ClickUp(m);
            _pressed = false;
        }
    }

    public void Process(Vector2 mousePos)
    {
        if (_pressed)
        {
            ClickHeld(mousePos);
        }
    }

    protected abstract void ClickDown(InputEventMouseButton m);
    protected abstract void ClickUp(InputEventMouseButton m);
    protected abstract void ClickHeld(Vector2 pos);
}
