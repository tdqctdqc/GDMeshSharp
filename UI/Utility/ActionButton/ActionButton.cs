using Godot;
using System;

public class ActionButton : Button
{
    private Action _action;
    public void Setup(string name, Action action)
    {
        Text = name;
        _action = action;
        Connect("button_up", this, nameof(DoAction));
    }

    private void DoAction()
    {
        _action.Invoke();
    }
}
