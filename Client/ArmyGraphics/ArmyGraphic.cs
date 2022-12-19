using Godot;
using System;
using System.Linq;

public class ArmyGraphic : Node2D
{
    public ArmyHQGraphic HQGraphic { get; private set; }
    private Line2D _frontLine, _unitLine, _homeLocLine, _attackLocLine;
    private Army _army;
    public override void _Ready()
    {
        
    }

    public void Setup(Army army)
    {
        _frontLine = new Line2D();
        AddChild(_frontLine);
        _frontLine.Width = 5f;
        _frontLine.DefaultColor = army.Faction.Color;
        _frontLine.Visible = false;
        
        _unitLine = new Line2D();
        AddChild(_unitLine);
        _unitLine.Width = 2f;
        _unitLine.DefaultColor = army.Faction.Color;
        _unitLine.Visible = false;
        
        _homeLocLine = new Line2D();
        AddChild(_homeLocLine);
        _homeLocLine.Width = 5f;
        _homeLocLine.DefaultColor = Colors.Blue;
        _homeLocLine.Visible = false;

        
        _attackLocLine = new Line2D();
        AddChild(_attackLocLine);
        _attackLocLine.Width = 1f;
        _attackLocLine.DefaultColor = Colors.Red;
        _attackLocLine.Visible = false;
        
        
        _army = army; 
        HQGraphic = ArmyHQGraphic.GetGraphic(army);
        AddChild(HQGraphic);

        HQGraphic.Handle.Connect("mouse_entered", this, nameof(MouseEntered));
        HQGraphic.Handle.Connect("mouse_exited", this, nameof(MouseExited));
    }
    
    public void UpdateFrontline(WorldManager world)
    {
    }

    public void Update(WorldManager world)
    {
        var hqPos = _army.HqPosition;
        HQGraphic.Update();
        _frontLine.ClearPoints();
        for (var i = 0; i < _army.FieldUnits.Count; i++)
        {
            _frontLine.AddPoint(_army.FieldUnits[i].CenterPosition);
        }
        _unitLine.ClearPoints();
        for (var i = 0; i < _army.FieldUnits.Count; i++)
        {
            _unitLine.AddPoint(hqPos);
            _unitLine.AddPoint(_army.FieldUnits[i].CenterPosition);
        }
        
        _homeLocLine.ClearPoints();
        if (_army.Defend is Location def)
        {
            _homeLocLine.AddPoint(hqPos);
            _homeLocLine.AddPoint(def.Position);
        }
        
        _attackLocLine.ClearPoints();
        if (_army.Attack is Location atk)
        {
            _attackLocLine.AddPoint(hqPos);
            _attackLocLine.AddPoint(atk.Position);
        }
    }

    private void MouseEntered()
    {
        _attackLocLine.Visible = true;
        _homeLocLine.Visible = true;
        _unitLine.Visible = true;
        _frontLine.Visible = true;
    }

    private void MouseExited()
    {
        _attackLocLine.Visible = false;
        _homeLocLine.Visible = false;
        _unitLine.Visible = false;
        _frontLine.Visible = false;
    }
}
