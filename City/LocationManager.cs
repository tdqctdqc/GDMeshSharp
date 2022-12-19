using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class LocationManager
{
    private WorldManager _world;
    public Graph<Location, LocationEdge> LocationGraph { get; private set; }
    public Graph<Location, LocationEdge> CityGraph { get; private set; }
    public List<Location> Locations { get; private set; }
    public Dictionary<Triangle, Location> TriangleLocations { get; private set; }
    public Action<LocationChangedFactionEvent> LocationChangedFaction { get; set; }
    public LocationManager(WorldManager world)
    {
        _world = world;
        Locations = new List<Location>();
        TriangleLocations = new Dictionary<Triangle, Location>();
    }

    public Location AddLocation(Triangle tri)
    {
        var location = new Location(tri, tri.GetCentroid(_world.Geometry));
        Locations.Add(location);
        TriangleLocations.Add(location.Tri, location);
        return location;
    }

    public void RemoveLocation(Location location)
    {
        Locations.Remove(location);
        TriangleLocations.Remove(location.Tri);
    }
    public void BuildGraphs()
    {
        bool triIsCity(Triangle tri)
        {
            return tri.Info.TerrainType == Terrain.DenseUrban;
        }
        var cityLocs = Locations.Where(l => triIsCity(l.Tri)).ToList();
        LocationGraph = GraphGenerator.GenerateDelaunayGraph<Location, LocationEdge>(
            Locations,
            l => l.Tri.GetCentroid(_world.Geometry),
            (l1,l2) => new LocationEdge(l1,l2)
        );
        CityGraph = GraphGenerator.GenerateDelaunayGraph<Location, LocationEdge>(
            cityLocs,
            l => l.Tri.GetCentroid(_world.Geometry),
            (l1,l2) => new LocationEdge(l1,l2)
        );
    }
}
