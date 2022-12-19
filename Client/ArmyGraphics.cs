using Godot;
using System;
using System.Collections.Generic;

public class ArmyGraphics : Node
{
    public FrontlineDrawer FrontlineDrawer { get; private set; }
    private WorldManager _world;
    private ArmyManager _armies;
    private Node2D _unitHolderNode, _armyHolderNode;
    private Dictionary<FieldUnit, FieldUnitGraphic> _unitGraphics;
    private Dictionary<Army, ArmyGraphic> _armyGraphics; 
    public void Setup(WorldManager world)
    {
        _world = world;
        _armies = world.Armies;
        _unitHolderNode = new Node2D();
        AddChild(_unitHolderNode);
        _unitGraphics = new Dictionary<FieldUnit, FieldUnitGraphic>();
        FrontlineDrawer = new FrontlineDrawer();
        AddChild(FrontlineDrawer);
        _armyGraphics = new Dictionary<Army, ArmyGraphic>();
    }

    public void Start()
    {
        _armyHolderNode?.Free();
        _armyHolderNode = new Node2D();
        AddChild(_armyHolderNode);
        
        _unitHolderNode?.Free();
        _unitHolderNode = new Node2D();
        AddChild(_unitHolderNode);
        
        _armies.Armies.ForEach(AddArmyGraphic);
        _armies.FieldUnits.ForEach(AddFieldUnitGraphic);
        _armies.AddedFieldUnit += AddFieldUnitGraphic;
        _armies.AddedArmy += AddArmyGraphic;
        _armies.UpdatedFrontline += UpdateFrontline;
    }

    public void Stop()
    {
        _armies.AddedFieldUnit -= AddFieldUnitGraphic;
        _armies.AddedArmy -= AddArmyGraphic;
        _armies.UpdatedFrontline -= UpdateFrontline;

        _armyHolderNode?.Free();
        _armyHolderNode = null;
        _unitHolderNode?.Free();
        _unitHolderNode = null;

        _armyGraphics.Clear();
        _unitGraphics.Clear();
        FrontlineDrawer.ClearPoints();
    }

    private void AddFieldUnitGraphic(FieldUnit unit)
    {
        var graphic = FieldUnitGraphic.GetGraphic();
        graphic.Setup(unit);
        _unitHolderNode.AddChild(graphic);
        _unitGraphics.Add(unit, graphic);
    }

    private void AddArmyGraphic(Army army)
    {
        var graphic = new ArmyGraphic();
        graphic.Setup(army);
        _armyHolderNode.AddChild(graphic);
        _armyGraphics.Add(army, graphic);
    }

    private void UpdateFrontline(Army army)
    {
        _armyGraphics[army].UpdateFrontline(_world);
    }
    
    public void Update(WorldManager world)
    {
        foreach (var fieldUnitGraphic in _unitGraphics.Values)
        {
            fieldUnitGraphic.Update(_armies);
        }
        foreach (var armyGraphic in _armyGraphics.Values)
        {
            armyGraphic.Update(world);
        }
    }
}