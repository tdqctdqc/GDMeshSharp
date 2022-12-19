using Godot;
using System;

public class CheckBoxEntry : Control
{
    private static PackedScene _scene = (PackedScene) GD.Load("res://UI/Utility/CheckBoxEntry/CheckBoxEntry.tscn");
    private Label _label;
    private CheckBox _checkBox;
    private Action<bool> _toggleAction;
    public override void _Ready()
    {
        
    }

    public static CheckBoxEntry GetCheckBoxEntry(string name, Action<bool> toggleAction)
    {
        var checkBox = (CheckBoxEntry) _scene.Instance();
        checkBox.Setup(name, toggleAction);
        return (CheckBoxEntry) _scene.Instance();
    }

    public void Setup(string name, Action<bool> toggleAction)
    {
        _toggleAction = toggleAction;
        _label = GetNode<Label>("Label");
        _label.Text = name;
        _checkBox = GetNode<CheckBox>("CheckBox");
        _checkBox.Connect("toggled", this, nameof(Toggle));
    }

    private void Toggle(bool toggle)
    {
        _toggleAction.Invoke(toggle);
    }
}
