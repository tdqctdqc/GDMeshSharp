using Godot;
using System;
using System.Linq;

public class BuildTerrains : MapAction
{
    public override void DoAction(WorldManager world)
    {
        BuildPolyTerrains(world);
        BuildLandmassGraph(world);
    }

    private void BuildPolyTerrains(WorldManager world)
    {
        void doPoly(Polygon poly)
        {
            foreach (var tri in poly.Triangles)
            {
                if (tri.Info.IsLand)
                {
                    var terrain = Terrain.GetTerrainByRoughness(tri.Info.Roughness.OutValue);
                    tri.Info.SetTerrain(terrain);
                }
                else
                {
                    tri.Info.SetTerrain(Terrain.ProtoWater);
                }
            }

        }

        world.GeologyPolygons.Polygons.TryParallel(doPoly);
    }

    private void BuildLandmassGraph(WorldManager world)
    {
        var geometry = world.Geometry;

        var unions = UnionFind<Triangle, bool>.DoUnionFind(
            geometry.Triangles,
            (t,r) => t.Info.IsWater == r.Info.IsWater,
            t => t.GetTrisSharingEdge(geometry)
        );
        var landUnions = unions.Where(u => u[0].Info.IsLand).ToList();
        var waterUnions = unions.Where(u => u[0].Info.IsWater).ToList();
        foreach (var landUnion in landUnions)
        {
            world.Landmasses.AddLandWaterMass(landUnion);
        }
        foreach (var waterUnion in waterUnions)
        {
            world.Landmasses.AddLandWaterMass(waterUnion);
        }
        world.Landmasses.BuildLandmassGraph();
    }
}
