using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArmyManager 
{
    public List<Army> Armies { get; private set; }
    public List<FieldUnit> FieldUnits { get; private set; }
    private int _armyIdCounter;
    private int _fieldUnitIdCounter;
    public RegularGrid<FieldUnit> UnitGrid { get; private set; }
    public Action<FieldUnit> AddedFieldUnit { get; set; }
    public Action<Army> AddedArmy { get; set; }
    public Action<Army> UpdatedFrontline { get; set; }
    private WorldManager _world;
    public ArmyManager(WorldManager world)
    {
        _world = world;
        Armies = new List<Army>();
        FieldUnits = new List<FieldUnit>();
        UnitGrid = new RegularGrid<FieldUnit>(u => u.CenterPosition, 200f);
    }
    public void Process(float delta)
    {
        UnitGrid.Update();
        FieldUnits.ForEach(f => f.Process(_world, delta));
    }
    public Army AddArmyWithUnits(Faction faction, int numUnits, Vector2 pos)
    {
        var army = AddArmy(faction);
        for (int i = 0; i < numUnits; i++)
        {
            var unit = AddFieldUnit(army);
            UnitGrid.AddElement(unit, unit.CenterPosition);
            // unit.SetLinePosition(pos, pos);
            army.FieldUnits.Add(unit);
        }
        
        return army;
    }

    private Army AddArmy(Faction faction)
    {
        _armyIdCounter++;
        var army = new Army(_armyIdCounter, faction);
        Armies.Add(army);
        AddedArmy?.Invoke(army);
        return army;
    }

    private FieldUnit AddFieldUnit(Army army)
    {
        _fieldUnitIdCounter++;
        var unit = new FieldUnit(_fieldUnitIdCounter, army);
        FieldUnits.Add(unit);
        AddedFieldUnit?.Invoke(unit);
        return unit;
    }
}
