using Godot;
using System;
using System.Linq;
using DelaunatorNetStd;

public class WorldManager 
{
    public GeometryManager Geometry { get; private set; }
    public PolygonManager GeologyPolygons { get; private set; }
    public PlateManager Plates { get; private set; }
    public RiverManager Rivers { get; private set; }
    public RoadManager Roads { get; private set; }
    public ArmyManager Armies { get; private set; }
    public LandmassManager Landmasses { get; private set; }
    public FactionManager Factions { get; private set; }
    public LocationManager Locations { get; private set; }
    public StrategicManager Strategic { get; private set; }
    public WorldManager(Vector2 dimensions)
    {
        var points = PointsGenerator.GenerateConstrainedSemiRegularPoints(
            dimensions, 50f, 25f, true);
        var delaunay = new Delaunator(points.Select(p => new DelaunatorPoint(p)).ToArray());

        Geometry = new GeometryManager(delaunay, dimensions);
        GeologyPolygons = new PolygonManager(Geometry);
        Landmasses = new LandmassManager(this);
        Plates = new PlateManager();
        Rivers = new RiverManager(this);
        Roads = new RoadManager(this);
        Armies = new ArmyManager(this);
        Factions = new FactionManager(this);
        Locations = new LocationManager(this);
        Strategic = new StrategicManager(this);
    }

    public void Process(float delta)
    {
        Armies.Process(delta);
    }
}
