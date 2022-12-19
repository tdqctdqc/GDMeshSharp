using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BuildStrategic : MapAction
{
    public override void DoAction(WorldManager world)
    {
        world.Strategic.StartStrategy();
        world.Strategic.BuildFronts();
        BuildArmies(world);
    }
    
    private void BuildArmies(WorldManager world)
    {
        foreach (var fac in world.Factions.Factions)
        {
            var pos = fac.Territory.Triangles[0].GetCentroid(world.Geometry);
            var armies = new List<Army>();
            var numFrontEdges = fac.Strategy.Theaters
                .SelectMany(t => t.StrategicNodes.Values)
                .SelectMany(fn => fn.FrontNodes)
                .SelectMany(fe => fe.BorderEdges)
                .Count();
            
            
            
            for (int i = 0; i < numFrontEdges; i++)
            {
                var army = world.Armies.AddArmyWithUnits(fac, 5, pos);
                armies.Add(army);
            }
            fac.Strategy.GetArmies(armies, world);
        }
    }
}
