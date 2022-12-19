using Godot;
using System;

public class TheaterHQGraphic : Node2D
{
    private static PackedScene _scene = 
        (PackedScene) GD.Load("res://Client/ArmyGraphics/TheaterHQGraphic.tscn");

    private MeshInstance2D _facColor, _theaterColor;
    private Control _control;
    private Theater _theater;
    
    public void Setup(Theater theater)
    {
        _theater = theater;
        _facColor = (MeshInstance2D) FindNode("FactionColor");
        _facColor.SelfModulate = theater.Faction.Color;
        _theaterColor = (MeshInstance2D) FindNode("TheaterColor");
        _theaterColor.SelfModulate = theater.Color;
        _control = (Control) FindNode("Control");
        _control.Connect("mouse_entered", this, nameof(MouseEntered));
        _control.Connect("mouse_exited", this, nameof(MouseExited));
    }
    public static TheaterHQGraphic GetGraphic(Theater theater)
    {
        var graphic = (TheaterHQGraphic) _scene.Instance();
        graphic.Setup(theater);
        return graphic;
    }

    private void MouseEntered()
    {
        // GD.Print($"Reserves: {_theater.Reserves.Armies.Count}");
    }

    private void MouseExited()
    {
        
    }
}
