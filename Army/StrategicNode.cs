using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StrategicNode
{
    public Faction Faction { get; private set; }
    public Location Location { get; private set; }
    public Theater Theater { get; private set; }
    public List<FrontNode> FrontNodes { get; private set; }
    public List<Army> Reserves { get; private set; }
    public StrategicNode(Faction fac, Location location, Theater theater)
    {
        Theater = theater;
        Faction = fac;
        Location = location;
        Reserves = new List<Army>();
        FrontNodes = new List<FrontNode>();
    }
    
    public void ClearFronts(WorldManager world)
    {
        foreach (var frontNode in FrontNodes)
        {
            Reserves.AddRange(frontNode.Disband(world));
        }
        FrontNodes.Clear();
    }

    public void TakeFrontNode(FrontNode fn)
    {
        FrontNodes.Remove(fn);
    }
    public void Transfer(Theater newTheater)
    {
        if (Theater != null) Theater.StrategicNodes.Remove(Location);
        Theater = newTheater;
        Theater.StrategicNodes[Location] = this;
    }
    public void GetArmies(List<Army> armies, WorldManager world)
    {
        Reserves.AddRange(armies);
        var requests = Requester<Army, FrontNode>
            .DoRequests(FrontNodes, 
                Reserves, 
                fe => fe.GetReinforcementNeed(world),
                a => 1f);
        foreach (var entry in requests)
        {
            entry.Key.GetArmies(entry.Value, world);
        }
    }

    public Army AskForReinforcement(float askPriority, WorldManager world)
    {
        if (Reserves.Count > 0 && askPriority > GetReinforcementNeed(world))
        {
            var reinforce = Reserves[0];
            Reserves.RemoveAt(0);
            reinforce.ClearGoal();
            return reinforce;
        }

        return null;
    }
    public void AddFrontNode(FrontNode frontNode)
    {
        FrontNodes.Add(frontNode);
    }

    public void DisbandFrontNode(FrontNode frontNode, WorldManager world)
    {
        Reserves.AddRange(frontNode.Disband(world));
        FrontNodes.Remove(frontNode);
    }
    public List<Army> Disband(WorldManager world)
    {
        foreach (var frontNode in FrontNodes)
        {
            Reserves.AddRange(frontNode.Disband(world));
        }
        Reserves.ForEach(a => a.ClearGoal());
        if (FrontNodes.Count != 0)
        {
            throw new Exception("trying to disband strat node with active front nodes");
        }
        return Reserves;
    }
    public float GetReinforcementNeed(WorldManager world)
    {
        var val = FrontNodes
                      .Select(fn => fn.GetReinforcementNeed(world))
                      .Sum()
                  - Reserves.Count;
        
        return Mathf.Max(0f, val);
    }
}
