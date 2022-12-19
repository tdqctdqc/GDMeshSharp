using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public static class Geology
{
    public static EdgeInteraction FragmentBorderEdgeInteraction { get; private set; } 
        = new FragmentBorderEdgeInteraction();
    public static FragmentPolygon FragmentPolygon { get; private set; }
        = new FragmentPolygon();

    public static void DoMapAction(MapAction action, WorldManager world)
    {
        action.DoAction(world);
    }
    public static void DoEdgeInteraction(EdgeInteraction interaction, WorldManager world)
    {
        var binding = new EdgeSet<Polygon>();
        var polys = world.GeologyPolygons.Polygons.ToList();
        foreach (var poly in polys)
        {
            foreach (var nPoly in poly.GetAdjacentPolygons(world.Geometry))
            {
                if (binding.Contains(poly, nPoly)) continue;
                binding.Add(poly, nPoly);
                if (interaction.CheckInteractionValid(poly, nPoly))
                {
                    interaction.DoInteraction(world.Geometry, world.GeologyPolygons, poly, nPoly);
                }
            }
        }
    }
    
    public static void DoPolygonSelfAction(PolygonSelfAction polyAction, WorldManager world)
    {
        var polys = world.GeologyPolygons.Polygons.ToList();
        polys.TryParallel(p => polyAction.PerformAction(p, world));
    }
}
