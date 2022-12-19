using Godot;
using System;

public class ArmyHQGraphic : Node2D
{
    private Army _army;
    private Label _number;
    public Control Handle { get; private set; }
    private static PackedScene _scene = (PackedScene) GD.Load("res://Client/ArmyGraphics/ArmyHQGraphic.tscn");
    public override void _Ready()
    {
    }

    public void Setup(Army army)
    {
        _army = army;
        _number = (Label) FindNode("Label");
        _number.Text = army.Id.ToString();
        SelfModulate = army.Faction.Color;
        Handle = (Control) FindNode("Handle");
        Handle.Connect("gui_input", this, nameof(Clicked));
        
        Position = _army.HqPosition;
    }

    public void Update()
    {
        Position = _army.HqPosition;
    }
    public static ArmyHQGraphic GetGraphic(Army army)
    {
        var graphic = (ArmyHQGraphic) _scene.Instance();
        graphic.Setup(army);
        return graphic;
    }

    private void Clicked(InputEvent @event)
    {
        if (@event is InputEventMouseButton m)
        {
            if (m.ButtonIndex == 1 && m.Pressed == false)
            {
                Game.I.Ui.ArmyUi.SelectArmy(_army);
            }
        }
    }
}
