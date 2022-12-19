using Godot;
using System;

public class Ui : Control
{
    public PolygonUi PolygonUi { get; private set; }
    public ArmyUi ArmyUi { get; private set; }
    public ControlPanelUi ControlPanelUi { get; private set; }
    public TriUnderMouse TriUnderMouse { get; private set; }
    private Button _randomizeSeed, _redraw, _test;
    public override void _Ready()
    {
        
    }

    public void Setup(WorldManager world)
    {
        PolygonUi = (PolygonUi) FindNode("SelectedPolygonUi");
        TriUnderMouse = (TriUnderMouse) FindNode("TriUnderMouse");
        ArmyUi = (ArmyUi) FindNode("ArmyUi");
        ControlPanelUi = (ControlPanelUi) FindNode("GeologyUi");
        ControlPanelUi.Setup(world);
        _randomizeSeed = (Button) FindNode("RandomSeed");
        _randomizeSeed.Connect("button_up", this, nameof(RandomizeSeed));
        
        _redraw = (Button) FindNode("Redraw");
        _redraw.Connect("button_up", this, nameof(Redraw));
        
        _test = (Button) FindNode("Test");
        _test.Connect("button_up", this, nameof(Test));
    }
    private void RandomizeSeed()
    {
        Game.Random.Seed = (ulong) DateTime.Now.Millisecond;
    }

    private void Redraw()
    {
        Game.I.Graphics.Stop();
        Game.I.Graphics.Start();
    }

    private void Test()
    {
        Game.I.Commands.DoTest();
    }
}
