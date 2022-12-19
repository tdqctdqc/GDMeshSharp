using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;

public class StrategicManager
{
    private WorldManager _world;
    public List<StrategicNode> StrategicNodes { get; private set; }
    public Dictionary<Location, StrategicNode> LocationStratNodes { get; private set; }
    public List<FrontNode> FrontNodes { get; private set; }
    public List<Theater> Theaters { get; private set; }
    public Dictionary<SubGraph<Location,LocationEdge>, Theater> SubGraphTheaters { get; private set; }
    public Dictionary<int, FrontNode> EdgeFrontNodes { get; private set; }
    public StrategicManager(WorldManager world)
    {
        _world = world;
        StrategicNodes = new List<StrategicNode>();
        LocationStratNodes = new Dictionary<Location, StrategicNode>();
        FrontNodes = new List<FrontNode>();
        Theaters = new List<Theater>();
        SubGraphTheaters = new Dictionary<SubGraph<Location, LocationEdge>, Theater>();
        EdgeFrontNodes = new Dictionary<int, FrontNode>();
        _world.Factions.TriChangedFaction += ChangeTriFaction;
    }
    public void ChangeTriFaction(TriChangedFactionEvent evnt)
    {
        var tri = evnt.Tri;
        var fnsToUpdate = new HashSet<FrontNode>();
        for (int i = 0; i < 3; i++)
        {
            var e = tri.HalfEdges[i];
            if (EdgeFrontNodes.ContainsKey(e))
            {
                var fn = EdgeFrontNodes[e];
                fnsToUpdate.Add(fn);
            }
            if (_world.Geometry.HalfEdgePairs[e] is int o 
                     && EdgeFrontNodes.ContainsKey(o))
            {
                var fn = EdgeFrontNodes[o];
                fnsToUpdate.Add(fn);
            }
        }

        FrontNode winnerFn = null;
        FrontNode loserFn = null;
        foreach (var frontNode in fnsToUpdate)
        {
            if (frontNode.HomeNode.Faction == evnt.GainingFaction
                && winnerFn == null)
            {
                winnerFn = frontNode;
            }
            else if (frontNode.HomeNode.Faction == evnt.LosingFaction
                     && loserFn == null)
            {
                loserFn = frontNode;
            }
        }
        if(loserFn != null) loserFn.HandleTriFactionChange(evnt, _world);
        if(winnerFn != null) winnerFn.HandleTriFactionChange(evnt, _world);
    }
    public StrategicNode AddStrategicNode(Location loc, Faction fac, Theater theater)
    {
        var sn = new StrategicNode(fac, loc, theater);
        LocationStratNodes.Add(loc, sn);
        StrategicNodes.Add(sn);
        return sn;
    }

    public void RemoveStrategicNode(StrategicNode node)
    {
        LocationStratNodes.Remove(node.Location);
        StrategicNodes.Remove(node);
    }
    public FrontNode AddFrontNode(StrategicNode strategicNode)
    {
        var frontNode = new FrontNode(strategicNode);
        strategicNode.AddFrontNode(frontNode);
        FrontNodes.Add(frontNode);
        return frontNode;
    }

    public void RemoveFrontNode(FrontNode fn)
    {
        FrontNodes.Remove(fn);
        fn.BorderEdges.ForEach(e =>
        {
            if (EdgeFrontNodes.ContainsKey(e) && EdgeFrontNodes[e] == fn) 
                EdgeFrontNodes.Remove(e);
        });
    }
    public Theater AddTheater(Faction fac, SubGraph<Location, LocationEdge> subGraph = null)
    {
        if(subGraph == null) subGraph = _world.Locations.LocationGraph.AddSubGraph();

        var theater = new Theater(fac, subGraph, fac.Strategy.TheaterColorDispenser.GetColor());
        Theaters.Add(theater);
        SubGraphTheaters.Add(subGraph, theater);
        fac.Strategy.AddTheater(theater);
        return theater;
    }

    public void RemoveTheater(Theater theater)
    {
        Theaters.Remove(theater);
        theater.Faction.Strategy.RemoveTheater(theater);
        var locGraph = _world.Locations.LocationGraph;
        locGraph.RemoveSubGraph(theater.SubGraph);
        SubGraphTheaters.Remove(theater.SubGraph);
    }
    public void StartStrategy()
    {
        _world.Locations.LocationChangedFaction += e => HandleLocationFactionChange(e, _world);

        for (var i = 0; i < _world.Factions.Factions.Count; i++)
        {
            var fac = _world.Factions.Factions[i];
            var locGraph = _world.Locations.LocationGraph;
            var facLocs = _world.Locations.Locations.Where(l => l.Tri.Info.Faction == fac);
            if (facLocs.Count() == 0)
            {
                continue;
            }
            for (var j = 0; j < _world.Landmasses.LandMasses.Count; j++)
            {
                var landmass = _world.Landmasses.LandMasses[j];
                var landmassLocs = facLocs.Where(l => landmass.Contains(l.Tri));
                if (landmassLocs.Count() == 0) continue;
                var theater = AddTheater(fac);
                foreach (var l in landmassLocs)
                {
                    theater.AddLoc(l);
                }
                theater.CheckTheater(_world);
            }
        }
    }

    public void CheckAllFronts()
    {
        for (var i = FrontNodes.Count - 1; i >= 0; i--)
        {
            FrontNodes[i].Check(_world);
        }
    }
    public void BuildFronts()
    {
        FrontNodes.Clear();
        StrategicNodes.ForEach(sn => sn.ClearFronts(_world));
        _world.Factions.Factions.ForEach(BuildFactionFronts);
        _world.Factions.Factions.ForEach(f => f.Strategy.GetArmies(new List<Army>(), _world));
    }
    public void BuildFactionFronts(Faction fac)
    {
        var landmasses = _world.Landmasses.LandMasses;
        var facStrategicNodes = StrategicNodes.Where(n => n.Faction == fac);
        var borderEdges = fac.Territory.BorderEdges
            .Where(e => _world.Geometry.HalfEdgePairs[e] is int o
                    && _world.Geometry.TrianglesByEdgeIds[o].Info.Faction != null);
        foreach (var landmass in landmasses)
        {
            var landmassStratNodes = facStrategicNodes
                .Where(n => landmass.Contains(n.Location.Tri))
                .ToList();
            if (landmassStratNodes.Count == 0) continue;
            var landmassBorderEdges = borderEdges
                .Where(e => landmass
                    .Contains(_world.Geometry.TrianglesByEdgeIds[e]))
                .ToList();
            var edgeHashList = new HashList<int>(landmassBorderEdges);
            while (edgeHashList.List.Count > 0)
            {
                var edge = edgeHashList.List[0];
                edgeHashList.Remove(edge);
                var edgePos = _world.Geometry.GetEdgeMidPoint(edge);
                var closeStratNode = landmassStratNodes
                    .OrderBy(sn => sn.Location.Position.DistanceTo(edgePos))
                    .First();
                var frontNode = AddFrontNode(closeStratNode);
                frontNode.AddEdge(edge, _world);
                var leftPoint = _world.Geometry.From[edge];
                var rightPoint = _world.Geometry.To[edge];
                int expand = 2;
                bool openLeft = true;
                bool openRight = true;
                for (var i = 0; i < 2; i++)
                {
                    if (openLeft)
                    {
                        openLeft = false;
                        var leftEdges = _world.Geometry.ToEdgesForPoint[leftPoint];
                        for (var j = 0; j < leftEdges.Count; j++)
                        {
                            var leftEdge = leftEdges[j];
                            if (edgeHashList.Contains(leftEdge))
                            {
                                edgeHashList.Remove(leftEdge);
                                frontNode.AddEdge(leftEdge, _world);
                                leftPoint = _world.Geometry.From[leftEdge];
                                openLeft = true;
                                break;
                            }
                        }
                    }
                    if(openRight)
                    {
                        openRight = false;
                        var rightEdges = _world.Geometry.FromEdgesForPoint[rightPoint];
                        for (var j = 0; j < rightEdges.Count; j++)
                        {
                            var rightEdge = rightEdges[j];
                            if (edgeHashList.Contains(rightEdge))
                            {
                                frontNode.AddEdge(rightEdge, _world);
                                edgeHashList.Remove(rightEdge);
                                rightPoint = _world.Geometry.To[rightEdge];
                                openRight = true;
                                break;
                            }
                        }
                    }

                    if (openLeft == false && openRight == false) break;
                }
            }
        }
    }
    
    
    private void HandleLocationFactionChange(LocationChangedFactionEvent evnt, WorldManager world)
    {
        var loc = evnt.Location;
        var locGraph = world.Locations.LocationGraph;
        
        if(evnt.LosingTheater != null) evnt.LosingTheater.LostLocation(loc, world);
        if(evnt.GainingTheater != null) evnt.GainingTheater.WonLoc(loc, world);
        
        var neighbors = world.Locations.LocationGraph[loc].Neighbors;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var n = neighbors[i];
            var nFac = n.Tri.Info.Faction;
            if (nFac == null) continue;
            if (locGraph.NodeSubGraphs.ContainsKey(n) == false) continue;
            
            var subGraph = locGraph.NodeSubGraphs[n];
            var theater = world.Strategic.SubGraphTheaters[subGraph];
            if(theater != evnt.GainingTheater && theater != evnt.LosingTheater)
            {
                theater.NeighborLocationChangedHands(loc, world);
            }
        }
    }
}
