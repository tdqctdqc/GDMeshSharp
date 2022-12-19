using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Faction 
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public Polygon Territory { get; private set; }
    // public List<Theater> Theaters { get; private set; }
    // public List<Army> FactionReserves { get; private set; }
    public FactionStrategy Strategy { get; private set; }

    public Faction(WorldManager world, int id, string name, Color color, Polygon territory)
    {
        Id = id;
        Name = name;
        Color = color;
        Territory = territory;
        Strategy = new FactionStrategy(this, world);

    }

    
}
