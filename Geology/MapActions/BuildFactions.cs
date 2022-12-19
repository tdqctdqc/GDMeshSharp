using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BuildFactions : MapAction
{
    private int _numFactions;
    public BuildFactions(int numFactions)
    {
        _numFactions = numFactions;
    }
    public override void DoAction(WorldManager world)
    {
        CreateFactions(world);
        FactionsClaimSeedTris(world);
        FactionsFillOutMap(world);
    }

    private void CreateFactions(WorldManager world)
    {
        var factions = world.Factions;
        factions.AddDefaultFactions(_numFactions);
    }

    private void FactionsClaimSeedTris(WorldManager world)
    {
        var factions = world.Factions;
        var seedTris = new Dictionary<Faction, Triangle>();
        var takenCitiesHash = new HashSet<Triangle>();
        if (factions.Factions.Count > world.Locations.CityGraph.Elements.Count) throw new Exception();
        foreach (var f in world.Factions.Factions)
        {
            while (true)
            {
                var seedTri = world.Locations.CityGraph.Elements
                    .GetRandomElement().Tri;
                if (takenCitiesHash.Contains(seedTri) == false)
                {
                    seedTris.Add(f, seedTri);
                    takenCitiesHash.Add(seedTri);
                    break;
                }
            }
        }
        foreach (var entry in seedTris)
        {
            var fac = entry.Key;
            var tri = entry.Value;
            factions.ChangeTriFaction(tri, fac);
        }
    }

    private void FactionsFillOutMap(WorldManager world)
    {
        world.Factions.Territories.ExpandPolygons(t => t.Info.IsLand);
        foreach (var f in world.Factions.Factions)
        {
            foreach (var t in f.Territory.Triangles)
            {
                t.Info.SetFaction(f);
            }
        }
    }
}
