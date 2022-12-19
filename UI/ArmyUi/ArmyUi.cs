using Godot;
using System;

public class ArmyUi : Control
{
    public Army SelectedArmy { get; private set; }
    public int NumToSpawn => Mathf.CeilToInt((float)_numUnitsToSpawn.Value);
    private SpinBox _numUnitsToSpawn;
    private Label _selectedArmy;
    public override void _Ready()
    {
        _numUnitsToSpawn = (SpinBox) FindNode("NumUnitsToSpawn");
        _selectedArmy = (Label) FindNode("SelectedArmy");
    }

    public void SelectArmy(Army army)
    {
        SelectedArmy = army;
        if (army != null) _selectedArmy.Text = "Selected Army " + army.Id;
        else _selectedArmy.Text = "";
    }
}
