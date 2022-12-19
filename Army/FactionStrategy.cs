using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class FactionStrategy 
{
    public Faction Faction { get; private set; }
    public List<Theater> Theaters { get; private set; }
    public List<Army> FactionReserves { get; private set; }
    public ColorDispenser TheaterColorDispenser { get; private set; }
    
    public FactionStrategy(Faction faction, WorldManager world)
    {
        Faction = faction;
        Theaters = new List<Theater>();
        FactionReserves = new List<Army>();
        TheaterColorDispenser = new ColorDispenser();

    }
    public void AddTheater(Theater theater)
    {
        Theaters.Add(theater);
    }

    public void RemoveTheater(Theater theater)
    {
        Theaters.Remove(theater);
    }
    public void GetArmies(List<Army> armies, WorldManager world)
    {
        FactionReserves.AddRange(armies);
        var requests = Requester<Army, Theater>
            .DoRequests(Theaters, FactionReserves, 
                t => t.GetReinforcementNeed(world),
                a => 1f);
        foreach (var entry in requests)
        {
            entry.Key.GetArmies(entry.Value, world);
        }
    }
}
