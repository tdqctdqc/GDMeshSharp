using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class FactionManager
{
    public List<Faction> Factions { get; private set; }
    public PolygonManager Territories { get; private set; }
    private WorldManager _world;
    public Action<TriChangedFactionEvent> TriChangedFaction { get; set; } 
    private int _factionIdCounter;
    public FactionManager(WorldManager world)
    {
        _world = world;
        Factions = new List<Faction>();
        Territories = new PolygonManager(world.Geometry);
    }

    public Faction AddFaction(string name, Color color)
    {
        _factionIdCounter++;
        var terr = Territories.AddPolygon();
        var fac = new Faction(_world, _factionIdCounter, name, color, terr);
        Factions.Add(fac);
        return fac;
    }
    public void ChangeTriFaction(Triangle tri, Faction fac)
    {
        Faction losingFaction = null;
        if (tri.Info.Faction != null)
        {
            losingFaction = tri.Info.Faction;
            Territories.RemoveTriFromPolygon(losingFaction.Territory, tri);
        }
        Territories.AddTriToPolygon(fac.Territory, tri);   
        tri.Info.SetFaction(fac);
        var evnt = new TriChangedFactionEvent(tri, fac, losingFaction);
        TriChangedFaction?.Invoke(evnt);
        
        if (_world.Locations.TriangleLocations.ContainsKey(tri))
        {
            var loc = _world.Locations.TriangleLocations[tri];
            var locEvnt = new LocationChangedFactionEvent(losingFaction, fac, loc, _world);
            _world.Locations.LocationChangedFaction?.Invoke(locEvnt);
        }
    }
}

public static class FactionManagerExt
{
    public static int NumDefaultFactions => _defaultFactionNames.Count;
    private static List<string> _defaultFactionNames = new List<string>
    {
        "Lilliput",
        "Blefescu",
        "Brobdingnag",
        "Laputa",
        "Balnibarbi",
        "Glubbdubdrib",
        "Houyhnhnmia",
        "Yahooland",
        "Luggnagg"
    };
    private static List<Color> _defaultFactionColors = new List<Color>
    {
        Colors.Green,
        Colors.HotPink,
        Colors.Yellow,
        Colors.Lavender,
        Colors.Purple,
        Colors.Chocolate,
        Colors.White,
        Colors.Khaki,
        Colors.Black
    };

    public static void AddDefaultFactions(this FactionManager factions,
        int numFactions)
    {
        for (int i = 0; i < numFactions; i++)
        {
            factions.AddFaction(_defaultFactionNames[i], 
                _defaultFactionColors[i]);
        }
    }
}

