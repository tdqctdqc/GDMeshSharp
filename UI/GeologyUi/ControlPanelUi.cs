using Godot;
using System;
using System.Diagnostics;

public class ControlPanelUi : Node
{
    private WorldManager _world;
    private VBoxContainer _vBox;
    private SliderEntry _numPlatesSlider, _numCellsSlider, _percentLandSlider, _numFactionsSlider;
    public override void _Ready()
    {
        _vBox = (VBoxContainer) FindNode("VBoxContainer");
        SetupSliders();
        AddButton("Build", nameof(DoBuild));
        AddButton("Build Plates", nameof(BuildPlates));
        AddButton("Do Friction", nameof(DoFriction));
        AddButton("Build Terrains", nameof(BuildTerrains));
        AddButton("Build Rivers", nameof(BuildRivers));
        AddButton("Triangulate Rivers", nameof(TriangulateRivers));
        AddButton("Build Rivers New", nameof(BuildRiversNew));
        AddButton("Build Cities", nameof(BuildCities));
        AddButton("Build Roads", nameof(BuildRoads));
        AddButton("Build Factions", nameof(BuildFactions));
        AddButton("Build Strategic", nameof(BuildStrategic));
        AddButton("Rebuild Fronts", nameof(RebuildFronts));
        AddButton("Check All Fronts", nameof(CheckAllFronts));
    }

    public void Setup(WorldManager world)
    {
        _world = world;
    }
    private void AddButton(string name, string funcName)
    {
        var button = new Button();
        button.Text = name;
        button.Connect("button_up", this, funcName);
        _vBox.AddChild(button);
    }
    private void SetupSliders()
    {
        _numPlatesSlider = SliderEntry.GetSliderEntry(
            f => "Num Plates: " + (int) f,
            f => { },
            10, 50);
        _vBox.AddChild(_numPlatesSlider);
        
        _numCellsSlider = SliderEntry.GetSliderEntry(
            f => "Num Cells: " + (int) f,
            f => { },
            5, 20);
        _vBox.AddChild(_numCellsSlider);

        _percentLandSlider = SliderEntry.GetSliderEntry(
            f => "Percent Land: " + (int) f,
            f => { },
            10, 100);
        _vBox.AddChild(_percentLandSlider);
        
        _numFactionsSlider = SliderEntry.GetSliderEntry(
            f => "Num Factions: " + (int) f,
            f => { },
            1, FactionManagerExt.NumDefaultFactions);
        _vBox.AddChild(_numFactionsSlider);
    }

    private void DoBuild()
    {
        BuildPlates();
        DoFriction();
        BuildTerrains();
        BuildCities();
        BuildRoads();
        BuildFactions();
        BuildStrategic();
    }
    private void DoFriction()
    {
        RunAndTimeMapAction(new DoFriction());
    }
    private void BuildTerrains()
    {
        RunAndTimeMapAction(new BuildTerrains());
    }

    private void BuildPlates()
    {
        var action = new BuildPlates(
            Mathf.CeilToInt((float) _numPlatesSlider.Value),
            Mathf.CeilToInt((float) _numCellsSlider.Value),
            _world.Geometry.Dimensions,
            (float) _percentLandSlider.Value / 100f);
        RunAndTimeMapAction(action);
    }

    private void BuildRivers()
    {
        RunAndTimeMapAction(new BuildRivers());
    }
    private void BuildRiversNew()
    {
        // RunAndTimeMapAction(new BuildRiversNew());
    }
    private void BuildCities()
    {
        RunAndTimeMapAction(new BuildCities());
    }
    private void BuildRoads()
    {
        RunAndTimeMapAction(new BuildRoads());
    }

    private void BuildFactions()
    {
        var num = Mathf.CeilToInt((float) _numFactionsSlider.Value);
        RunAndTimeMapAction(new BuildFactions(num));
    }

    private void BuildStrategic()
    {
        RunAndTimeMapAction(new BuildStrategic());
    }
    private void RebuildFronts()
    {
        _world.Strategic.BuildFronts();
    }
    private void CheckAllFronts()
    {
        _world.Strategic.CheckAllFronts();
    }

    private void TriangulateRivers()
    {
        RunAndTimeMapAction(new TriangulateRivers());
    }
    private void RunAndTimeMapAction(MapAction mapAction)
    {
        var sw = new Stopwatch();
        sw.Start();
        Game.I.Commands.DoMapAction(mapAction);
        sw.Stop();
        GD.Print($"{mapAction.ToString()} took {sw.Elapsed.TotalSeconds} s");
    }
    
}
