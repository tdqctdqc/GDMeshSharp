using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class DoFriction : MapAction
{
    private static float _landDecayPerDist = .05f;
    private static float _waterDecayPerDist = .01f;
    private static float _landRandomizeStrength = .25f;
    private static float _waterRandomizeStrength = .01f;
    public override void DoAction(WorldManager world)
    {
        Do(world);
    }

    private void Do(WorldManager world)
    {
        var polygons = world.GeologyPolygons;
        var plates = world.Plates;
        void setFriction(Polygon poly)
        {
            var neighbors = polygons.Edges.GetNeighbors(poly);
            var count = neighbors.Count;
            for (var j = 0; j < count; j++)
            {
                var aPoly = neighbors[j];
                if (aPoly.Id < poly.Id)
                {
                    if (plates.CellPlates[poly] == plates.CellPlates[aPoly]) continue;
                    polygons.Edges.GetEdge(poly, aPoly)
                        .SetFriction(poly.GetDriftFrictionStrength(aPoly, world));
                }
            }
        }
        polygons.Polygons.TryParallel(setFriction);
        void doPoly(Polygon poly)
        {
            var adjacentPolys = polygons.Edges.GetNeighbors(poly);
            var adjCount = adjacentPolys.Count;
            for (int i = 0; i < adjCount; i++)
            {
                var aPoly = adjacentPolys[i];
                if (plates.CellPlates[poly] == plates.CellPlates[aPoly]) continue;

                var driftStrength = polygons.Edges.GetEdge(poly, aPoly).Friction;
                if (poly.Info.IsLand && aPoly.Info.IsLand)
                {
                    LandMeetingLand(world, poly, aPoly, driftStrength);
                }
                else if (poly.Info.IsLand && aPoly.Info.IsLand == false)
                {
                    LandMeetingWater(world, poly, aPoly, driftStrength);
                }
                else if (poly.Info.IsLand == false && aPoly.Info.IsLand)
                {
                    WaterMeetingLand(world, poly, aPoly, driftStrength);
                }
                else if (poly.Info.IsLand == false && aPoly.Info.IsLand == false)
                {
                    WaterMeetingWater(world, poly, aPoly, driftStrength);
                }
            }
        }
        polygons.Polygons.TryParallel(doPoly);
    }
    private static void LandMeetingLand(WorldManager world, Polygon poly, Polygon oPoly,
        float driftStrength)
    {
        // if (driftStrength < .25f) return;    
        var geometry = world.Geometry;
        var polygons = world.GeologyPolygons;
        var edges = poly.GetEdgesBorderingPolygon(oPoly, geometry);
        BuildMountainRange(poly, edges, world, driftStrength, 0f);
    }

    private static void LandMeetingWater(WorldManager world, Polygon poly, Polygon oPoly,
        float driftStrength)
    {
        var geometry = world.Geometry;
        var polygons = world.GeologyPolygons;
        if (driftStrength < .25f) return;
        var edges = poly.GetEdgesBorderingPolygon(oPoly, geometry);
        BuildMountainRange(poly, edges, world, driftStrength, 50f);
    }
    private static void WaterMeetingLand(WorldManager world, Polygon poly, Polygon oPoly,
        float driftStrength)
    {
        var geometry = world.Geometry;
        var polygons = world.GeologyPolygons;
        var edges = poly.GetEdgesBorderingPolygon(oPoly, geometry);
        ExtendShore(poly, edges, world, driftStrength);
    }
    private static void WaterMeetingWater(WorldManager world, Polygon poly, Polygon oPoly,
        float driftStrength)
    {
        var geometry = world.Geometry;
        var polygons = world.GeologyPolygons;
        if (driftStrength < .25f) return;
        var edges = poly.GetEdgesBorderingPolygon(oPoly, geometry);
        MakeIslandChain(poly, edges, world, driftStrength);
    }
    private static void BuildMountainRange(Polygon poly,
        List<int> borderEdges, WorldManager world, float driftStrength, float clearanceFromEdge)
    {
        var geometry = world.Geometry;
        var edgeMidPoints = borderEdges
            .Select(e => (geometry.PointsById[geometry.From[e]] 
                          + geometry.PointsById[geometry.To[e]]) / 2f)
            .ToList();

        var distToMakeMountainsTo = 150f;
        var potentialMountainSeeds = new List<Triangle>();
        var mountainSeeds = new List<Triangle>();
        var numMountains = Mathf.CeilToInt(borderEdges.Count * driftStrength);
        foreach (var tri in poly.Triangles)
        {
            var centroid = tri.GetCentroid(geometry);
            var dist = edgeMidPoints.Select(p => p.DistanceTo(centroid)).Min();
            if (dist < distToMakeMountainsTo && dist > clearanceFromEdge)
            {
                potentialMountainSeeds.Add(tri);
            }
        }

        while (mountainSeeds.Count < numMountains && potentialMountainSeeds.Count > 0)
        {
            var seed = potentialMountainSeeds.GetRandomElement();
            potentialMountainSeeds.Remove(seed);
            mountainSeeds.Add(seed);
        }

        foreach (var seed in mountainSeeds)
        {
            var effect = RandomizeEffect(driftStrength, _landRandomizeStrength);
            seed.Info.IncrementRoughness(effect);
        }
        
        foreach (var foothill in potentialMountainSeeds)
        {
            var effect = RandomizeEffect(driftStrength / 2f, _landRandomizeStrength);
            foothill.Info.IncrementRoughness(effect);
        }
    }

    private static void ExtendShore(Polygon poly,
        List<int> borderEdges, WorldManager world, float driftStrength)
    {
        var geometry = world.Geometry;
        var edgeMidPoints = borderEdges
            .Select(e => (geometry.PointsById[geometry.From[e]] 
                          + geometry.PointsById[geometry.To[e]]) / 2f)
            .ToList();

        var distToMakeLand = 200f * driftStrength;
        foreach (var tri in poly.Triangles)
        {
            var centroid = tri.GetCentroid(geometry);
            var dist = edgeMidPoints.Select(p => p.DistanceTo(centroid)).Min();
            if (dist < distToMakeLand)
            {
                tri.Info.SetIsLand(true);
            }
        }
    }

    private static void MakeIslandChain(Polygon poly,
        List<int> borderEdges, WorldManager world, float driftStrength)
    {
        var geometry = world.Geometry;
        if (driftStrength < .5f) return;
        var edgeMidPoints = borderEdges
            .Select(e => (geometry.PointsById[geometry.From[e]] 
                          + geometry.PointsById[geometry.To[e]]) / 2f)
            .ToList();

        var distToMakeIslandsTo = 75f;
        
        var potentialIslandSeeds = new List<Triangle>();
        var islandSeeds = new List<Triangle>();
        var numIslands = Mathf.CeilToInt(borderEdges.Count * driftStrength / 10f);
        foreach (var tri in poly.Triangles)
        {
            var centroid = tri.GetCentroid(geometry);
            var dist = edgeMidPoints.Select(p => p.DistanceTo(centroid)).Min();
            if (dist < distToMakeIslandsTo)
            {
                potentialIslandSeeds.Add(tri);
            }
        }

        while (islandSeeds.Count < numIslands && potentialIslandSeeds.Count > 0)
        {
            var seed = potentialIslandSeeds.GetRandomElement();
            potentialIslandSeeds.Remove(seed);
            islandSeeds.Add(seed);
        }
        
        foreach (var seed in islandSeeds)
        {
            var effect = RandomizeEffect(driftStrength / 10f, _landRandomizeStrength);
            seed.Info.IncrementRoughness(effect);
            seed.Info.SetIsLand(true);

            var neighbors = seed
                .GetTrisSharingEdge(geometry)
                .Where(t => poly.Triangles.Contains(t))
                    .Where(t => islandSeeds.Contains(t) == false);
            foreach (var neighbor in neighbors)
            {
                if (Game.Random.RandfRange(0f, 1f) < driftStrength)
                {
                    var e = RandomizeEffect(driftStrength / 15f, _landRandomizeStrength);
                    neighbor.Info.IncrementRoughness(e);
                    neighbor.Info.SetIsLand(true);
                }
            }
        }
    }
    private static float GetDecayFactor(float dist, float decayStrength)
    {
        return Mathf.Max(0f, 1f - dist * decayStrength);
        if (dist <= 100f) return 1f;
        return 1f / (decayStrength * dist - 1f);
    }

    private static float RandomizeEffect(float effect, float randStrength)
    {
        var rand = Game.Random.RandfRange(
            effect - effect * randStrength, 
            effect + effect * randStrength);
        return rand;
    }
}
