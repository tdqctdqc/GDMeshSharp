using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class LandmassManager
{
    private PolygonManager _landmassPolygons;
    private WorldManager _world;
    public List<HashList<Triangle>> LandMasses { get; private set; }
    public List<HashList<Triangle>> WaterMasses { get; private set; }
    public List<Graph<Triangle, float>> LandmassUrbanGraphs { get; private set; }
    public List<Graph<Triangle, float>> LandmassCityGraphs { get; private set; }
    public LandmassManager(WorldManager world)
    {
        _world = world;
        _landmassPolygons = new PolygonManager(world.Geometry);
        LandMasses = new List<HashList<Triangle>>();
        WaterMasses = new List<HashList<Triangle>>();
        LandmassUrbanGraphs = new List<Graph<Triangle, float>>();
        LandmassCityGraphs = new List<Graph<Triangle, float>>();
    }

    public void AddLandWaterMass(List<Triangle> mass)
    {
        var hashList = new HashList<Triangle>(mass);
        if (mass[0].Info.IsLand) LandMasses.Add(hashList);
        else WaterMasses.Add(hashList);
        _landmassPolygons.AddNewPolygonWithTrisNoChecks(mass);
    }

    public void BuildLandmassGraph()
    {
        _landmassPolygons.BuildGraph(_world);
    }

    public void BuildCityGraphs()
    {
        bool triIsUrban(Triangle tri)
        {
            return tri.Info.TerrainType == Terrain.DenseUrban
                   || tri.Info.TerrainType == Terrain.LightUrban;
        }

        bool triIsCity(Triangle tri)
        {
            return tri.Info.TerrainType == Terrain.DenseUrban;
        }
        for (int i = 0; i < LandMasses.Count; i++)
        {
            var landMass = LandMasses[i];
            var urban = landMass.List.Where(triIsUrban).ToList();
            var cities = urban.Where(triIsCity).ToList();
            if (urban.Count < 3) continue;
            var urbanGraph = GraphGenerator.GenerateDelaunayGraph<Triangle, float>(
                urban,
                t => t.GetCentroid(_world.Geometry),
                (t1,t2) => t1.GetCentroid(_world.Geometry).DistanceTo(t2.GetCentroid(_world.Geometry))
            );
            LandmassUrbanGraphs.Add(urbanGraph);
            if (cities.Count < 3) continue;
            var cityGraph = GraphGenerator.GenerateDelaunayGraph<Triangle, float>(
                cities,
                t => t.GetCentroid(_world.Geometry),
                (t1,t2) => t1.GetCentroid(_world.Geometry).DistanceTo(t2.GetCentroid(_world.Geometry))

            );
            LandmassCityGraphs.Add(cityGraph);
        }
    }
}
