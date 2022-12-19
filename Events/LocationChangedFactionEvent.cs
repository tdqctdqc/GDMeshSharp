using Godot;
using System;
using System.Linq;

public class LocationChangedFactionEvent 
{
    public Faction LosingFaction { get; private set; }
    public Theater LosingTheater { get; private set; }
    public Faction GainingFaction { get; private set; }
    public Theater GainingTheater { get; private set; }

    public Location Location { get; private set; }

    public LocationChangedFactionEvent(Faction losingFaction, Faction gainingFaction, 
        Location location, 
        WorldManager world)
    {
        LosingFaction = losingFaction;
        GainingFaction = gainingFaction;
        Location = location;
        if (losingFaction != null)
        {
            // var loserSubGraph = locGraph.NodeSubGraphs[location];

            LosingTheater = losingFaction.Strategy.Theaters
                .Where(t => t.SubGraph.Elements.Contains(location))
                .FirstOrDefault();
            //world.Strategic.SubGraphTheaters[loserSubGraph];
        }

        if (gainingFaction != null)
        {
            var locGraph = world.Locations.LocationGraph;
            var neighbors = locGraph[location].Neighbors;
            for (var i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                if (neighbor.Tri.Info.Faction == gainingFaction
                    && locGraph.NodeSubGraphs.ContainsKey(neighbor))
                {
                    var subGraph = locGraph.NodeSubGraphs[neighbor];
                    GainingTheater = world.Strategic.SubGraphTheaters[subGraph];
                    break;
                }
            }
            //todo if gaining theater null create new one 
        }
    }
}
