using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Theater
{
    public Faction Faction { get; private set; }
    public Color Color { get; private set; }
    public SubGraph<Location, LocationEdge> SubGraph { get; private set; }
    public Dictionary<Location, StrategicNode> StrategicNodes { get; private set; }
    public TheaterReserve Reserves { get; private set; }
    public Vector2 Position { get; private set; }
    
    public Theater(Faction faction, SubGraph<Location, LocationEdge> subGraph, Color color)
    {
        Color = color;
        Faction = faction;
        SubGraph = subGraph;
        StrategicNodes = new Dictionary<Location, StrategicNode>();
        Reserves = new TheaterReserve(this);
        Position = Vector2.Zero;
    }

    public void GetArmies(List<Army> armies, WorldManager world)
    {
        Reserves.AddRange(armies);
        ReinforceStratNodes(world, StrategicNodes.Values.ToArray());
    }
    private void ReinforceStratNodes(WorldManager world, 
        params StrategicNode[] nodes)
    {
        var requests = Requester<Army, StrategicNode>
            .DoRequests(nodes.ToList(),
                Reserves.Armies.ToList(),
                fn => fn.GetReinforcementNeed(world),
                a => 1f
            );
        foreach (var entry in requests)
        {
            entry.Value.ForEach(a => Reserves.Remove(a));
            entry.Key.GetArmies(entry.Value, world);
        }
    }

    public void AddLoc(Location location)
    {
        SubGraph.AddNode(location);
        SetPosition();
    }

    public void BuildStrategicNodes(WorldManager world)
    {
        var strategicLocs = SubGraph.GetBorder();
        foreach (var strategicLoc in strategicLocs)
        {
            CheckStratLocation(strategicLoc, world);
        }
    }
    private bool CheckLocHasDangerousNeighbors(Location loc, WorldManager world)
    {
        return world.Locations.LocationGraph[loc].Neighbors.Exists(n =>
                                   n.Tri.Info.Faction != Faction
                                   && n.Tri.Info.Faction != null);
    }
    private void CheckStratLocation(Location loc, WorldManager world)
    {
        if (StrategicNodes.ContainsKey(loc) && CheckLocHasDangerousNeighbors(loc, world) == false)
        {
            RemoveStratNode(loc, world);
        }
        if (StrategicNodes.ContainsKey(loc) == false
             && CheckLocHasDangerousNeighbors(loc, world)
             && loc.Tri.Info.Faction == Faction)
        {
            AddStratNode(loc, world);
        }
    }

    public void WonLoc(Location loc, WorldManager world)
    {
        AddLoc(loc);
        AddStratNode(loc, world);
        
        var neighbors = world.Locations.LocationGraph[loc].Neighbors;
        var frontNodesAttacking = new List<FrontNode>();
        foreach (var n in neighbors)
        {
            if (StrategicNodes.ContainsKey(n))
            {
                var nNode = StrategicNodes[n];
                
                for (var i = 0; i < nNode.FrontNodes.Count; i++)
                {
                    var fn = nNode.FrontNodes[i];
                    if (fn.TargetLoc == loc)
                    {
                        nNode.TakeFrontNode(fn);
                        frontNodesAttacking.Add(fn);
                    }
                }
                if (CheckLocHasDangerousNeighbors(n, world) == false)
                {
                    //todo redistribute front nodes
                    DisbandStrategicNode(n, world);
                }
            }
        }
        var node = StrategicNodes[loc];
        frontNodesAttacking.ForEach(fn => fn.Transfer(node));
        ReinforceStratNodes(world, node);
        
        if (CheckLocHasDangerousNeighbors(loc, world) == false)
        {
            RemoveStratNode(loc, world);
        }
    }

    private void AddStratNode(Location loc, WorldManager world)
    {
        AddLoc(loc);
        var sn = world.Strategic.AddStrategicNode(loc, Faction, this);
        StrategicNodes.Add(loc, sn);
    }

    public void LostLocation(Location loc, WorldManager world)
    {
        SubGraph.RemoveNode(loc);
        SetPosition();
        if (StrategicNodes.ContainsKey(loc))
        {
            DisbandStrategicNode(loc, world);
        }
        
        var neighbors = world.Locations.LocationGraph[loc].Neighbors;
        var neighborsToReinforce = new List<StrategicNode>();
        foreach (var neighbor in neighbors)
        {
            CheckStratLocation(neighbor, world);
            if(StrategicNodes.ContainsKey(neighbor)) 
                neighborsToReinforce.Add(StrategicNodes[neighbor]);
        }

        
        
        CheckTheater(world);
    }

    private void DisbandStrategicNode(Location loc, WorldManager world)
    {
        var node = StrategicNodes[loc];
        if (StrategicNodes.Count > 1)
        {
            var otherStratNodes = StrategicNodes.Values.ToList();
            otherStratNodes.Remove(node);
            for (var i = node.FrontNodes.Count - 1; i >= 0; i--)
            {
                var fnToTransfer = node.FrontNodes[i];
                var closeStratNode = otherStratNodes
                    .OrderBy(oSN =>
                        oSN.Location.Position.DistanceTo(fnToTransfer.GetApproxPos(world.Geometry)))
                    .First();
                fnToTransfer.Transfer(closeStratNode);
            }
        }
        else
        {
            //todo maybe they jsut dissolve? or try to transfer to closest theater in landmass
        }
        
        RemoveStratNode(loc, world);
    }

    public void NeighborLocationChangedHands(Location loc, WorldManager world)
    {
        var neighbors = world.Locations.LocationGraph[loc].Neighbors;
        foreach (var neighbor in neighbors)
        {
            if (StrategicNodes.ContainsKey(neighbor))
            {
                CheckStratLocation(neighbor, world);
            }
        }
    }
    private void RemoveStratNode(Location loc, WorldManager world)
    {
        var stratNode = StrategicNodes[loc];
        var disband = stratNode.Disband(world);
        StrategicNodes.Remove(loc);
        Reserves.AddRange(disband);
        var ns = world.Locations.LocationGraph[loc].Neighbors;
        ns.ForEach(n => CheckStratLocation(n, world));
        world.Strategic.RemoveStrategicNode(stratNode);
    }

    public void CheckTheater(WorldManager world)
    {
        var locGraph = world.Locations.LocationGraph;

        foreach (var l in SubGraph.Elements)
        {
            CheckStratLocation(l, world);
        }
        if (StrategicNodes.Count == 0)
        {
            world.Strategic.RemoveTheater(this);
            //todo try to transfer units to other theater
            return;
        }
        var unions = UnionFind<Location, bool>.DoUnionFind(
            StrategicNodes.Keys.ToList(),
            (t, r) => true,
            t => locGraph[t].Neighbors);
        if (unions.Count != 1)
        {
            locGraph.RemoveSubGraph(SubGraph);
            
            foreach (var union in unions)
            {
                var newTheater = world.Strategic.AddTheater(Faction);
                foreach (var location in union)
                {
                    newTheater.AddLoc(location);
                    var oldStratNode = StrategicNodes[location];
                    oldStratNode.Transfer(newTheater);
                }
            }
            world.Strategic.RemoveTheater(this);
        }
    }

    public float GetReinforcementNeed(WorldManager world)
    {
        var val = StrategicNodes
            .Select(sn => sn.Value.GetReinforcementNeed(world))
            .Sum()
            - Reserves.Armies.Count;
        return Mathf.Max(0f, val);
    }

    private void SetPosition()
    {
        if (SubGraph.Elements.Count == 0)
        {
            Position = Vector2.Zero;
            return;
        }
        Position = Vector2.Zero;
        SubGraph.Elements.ForEach(e => Position += e.Position);
        Position /= (float)SubGraph.Elements.Count;
    }
}
