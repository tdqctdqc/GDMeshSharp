using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Graphics : Node2D
{
    private WorldManager _world;
    public GeometryGraphics GeometryGraphics { get; private set; }
    public PolygonGraphics PolygonGraphics { get; private set; }
    public PlateGraphics PlateGraphics { get; private set; }
    public RiverGraphics RiverGraphics { get; private set; }
    public RoadGraphics RoadGraphics { get; private set; }
    public ArmyGraphics ArmyGraphics { get; private set; }
    public FactionGraphics FactionGraphics { get; private set; }
    public DebugGraphics DebugGraphics { get; private set; }
    public void Setup(WorldManager world)
    {
        _world = world;
        GeometryGraphics = new GeometryGraphics();
        GeometryGraphics.Setup(_world.Geometry);
        AddChild(GeometryGraphics);
        
        
        PolygonGraphics = new PolygonGraphics();
        PolygonGraphics.Setup(_world);
        AddChild(PolygonGraphics);

        RiverGraphics = new RiverGraphics();
        RiverGraphics.Setup(world);
        AddChild(RiverGraphics);

        PlateGraphics = new PlateGraphics();
        PlateGraphics.Setup(world);
        AddChild(PlateGraphics);

        RoadGraphics = new RoadGraphics();
        RoadGraphics.Setup(world);
        AddChild(RoadGraphics);

        ArmyGraphics = new ArmyGraphics();
        ArmyGraphics.Setup(world);
        AddChild(ArmyGraphics);

        FactionGraphics = new FactionGraphics();
        FactionGraphics.Setup(world);
        AddChild(FactionGraphics);

        DebugGraphics = new DebugGraphics();
        DebugGraphics.Setup(world);
        AddChild(DebugGraphics);
    }

    
    public void Stop()
    {
        PolygonGraphics.Stop();
        GeometryGraphics.Stop();
        PlateGraphics.Stop();
        RiverGraphics.Stop();
        RoadGraphics.Stop();
        ArmyGraphics.Stop();
        FactionGraphics.Stop();
        DebugGraphics.Clear();
    }

    public void Start()
    {
        GeometryGraphics.Start();
        PolygonGraphics.Start();
        PlateGraphics.Start();
        RiverGraphics.Start();
        RoadGraphics.Start();
        ArmyGraphics.Start();
        FactionGraphics.Start();
    }
    public override void _Process(float delta)
    {
        
        ArmyGraphics.Update(_world);
    }
    
    
}
