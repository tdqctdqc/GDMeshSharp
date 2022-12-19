using Godot;
using System;

public class FieldUnitGraphic : Node2D
{
    private static PackedScene _scene = (PackedScene) GD.Load("res://Client/ArmyGraphics/FieldUnitGraphic/FieldUnitGraphic.tscn");
    private MeshInstance2D _center, _color, _left, _right;
    private Line2D _line;
    private FieldUnit _unit;
    private Label _label;
    public override void _Ready()
    {
        
    }

    public static FieldUnitGraphic GetGraphic()
    {
        return (FieldUnitGraphic) _scene.Instance();
    }
    public void Setup(FieldUnit unit)
    {
        _center = (MeshInstance2D) FindNode("Center");
        _color = (MeshInstance2D) FindNode("Color");
        _left = (MeshInstance2D) FindNode("Left");
        _right = (MeshInstance2D) FindNode("Right");
        _label = (Label) FindNode("Label");
        _line = (Line2D) FindNode("Line2D");
        _line.Width = 2f;
        _line.DefaultColor = Colors.White;
        
        
        
        _unit = unit;
        _label.Text = _unit.Army.Id.ToString();
        _color.SelfModulate = ColorsExt.GetRainbowColor(
            Mathf.Abs(_unit.Army.FieldUnits.IndexOf(_unit)));
        _center.SelfModulate = _unit.Army.Faction.Color;
    }
    public void Update(ArmyManager armies)
    {
        _center.Position = _unit.CenterPosition;
        _color.Position = _unit.CenterPosition;
        _left.Position = _unit.LeftFlankPosition;
        _right.Position = _unit.RightFlankPosition;
        _line.ClearPoints();
        _line.AddPoint(_left.Position);
        _line.AddPoint(_center.Position);
        _line.AddPoint(_right.Position);
    }

}
