using Godot;
using System;
using System.Linq;

public class Game : Node
{
    public static Game I { get; private set; }
    public Ui Ui { get; private set; }
    public static RandomNumberGenerator Random { get; private set; }
        = new RandomNumberGenerator();
    public Session Session { get; set; }
    public Commands Commands { get; private set; }
    private WorldManager _world;
    private InputHandler _inputHandler;
    public Graphics Graphics { get; private set; }
    public PolygonBrush PolyBrush { get; private set; }
    public override void _Ready()
    {
        Random.Seed = 1;
        if(I != null)
        {
            this.Free();
            return;
        }
        I = this;
        var dimensions = new Vector2(6000f, 4000f);

        _world = new WorldManager(dimensions);
        var points = PointsGenerator.GenerateConstrainedSemiRegularPoints(
            dimensions, 50f, 25f, true);
        
        Graphics = new Graphics();
        Graphics.Setup(_world);
        Graphics.Start();
        AddChild(Graphics);
        PolyBrush = new PolygonBrush();
        var uiLayer = new CanvasLayer();
        AddChild(uiLayer);
        Ui = (Ui)((PackedScene) GD.Load("res://UI/Ui.tscn")).Instance();
        Ui.Setup(_world);
        uiLayer.AddChild(Ui);
        var polyBrushUi = (PolygonBrushUi)((PackedScene)GD.Load("res://UI/PolygonBrushUi/PolygonBrushUi.tscn")).Instance();
        uiLayer.AddChild(polyBrushUi);
        polyBrushUi.Setup(PolyBrush);
        _inputHandler = new InputHandler(_world, Graphics, Ui);
        Commands = new Commands(_world);
    }
    
    public override void _UnhandledInput(InputEvent e)
    {
        _inputHandler.HandleInput(e);
    }

    public override void _Process(float delta)
    {
        _inputHandler.Process();
        _world.Process(delta);
    }

    
}
