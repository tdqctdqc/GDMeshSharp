using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class FrontNode 
{
    public StrategicNode HomeNode { get; private set; }
    public Location TargetLoc { get; private set; }
    public List<int> BorderEdges { get; private set; }
    public List<Army> Armies { get; private set; }
    private static int _maxEdges = 7;
    private static int _minEdges = 3;
    public FrontNode(StrategicNode node)
    {
        HomeNode = node;
        BorderEdges = new List<int>();
        Armies = new List<Army>();
    }

    public void HandleTriFactionChange(TriChangedFactionEvent evnt, WorldManager world)
    {
        if (evnt.GainingFaction == HomeNode.Faction)
        {
            GainTri(evnt, world);
        }
        else if (evnt.LosingFaction == HomeNode.Faction)
        {
            LoseTri(evnt, world);
        }
    }

    private void GainTri(TriChangedFactionEvent evnt, WorldManager world)
    {
        var geometry = world.Geometry;
        for (int i = 0; i < 3; i++)
        {
            var edge = evnt.Tri.HalfEdges[i];
            if (geometry.HalfEdgePairs[edge] is int oEdge)
            {
                RemoveEdge(oEdge, world);
                var oTri = geometry.TrianglesByEdgeIds[oEdge];
                if (oTri.Info.Faction != HomeNode.Faction
                    && oTri.Info.Faction != null)
                {
                    AddEdge(edge, world);
                }
            }
        }
    }
    
    private void LoseTri(TriChangedFactionEvent evnt, WorldManager world)
    {
        var geometry = world.Geometry;
        for (int i = 0; i < 3; i++)
        {
            var edge = evnt.Tri.HalfEdges[i];
            RemoveEdge(edge, world);
            if (geometry.HalfEdgePairs[edge] is int oEdge)
            {
                var oTri = geometry.TrianglesByEdgeIds[oEdge];
                if (oTri.Info.Faction == HomeNode.Faction)
                {
                    AddEdge(oEdge, world);
                }
            }
        }
    }

    private bool EdgeGoodForBorder(int edge, WorldManager world)
    {
        if (world.Geometry.HalfEdgePairs[edge] is int oEdge == false)
            return false;
        if (world.Geometry.TrianglesByEdgeIds[edge].Info.Faction != HomeNode.Faction)
        {
            return false;
        }

        if (world.Geometry.TrianglesByEdgeIds[oEdge].Info.Faction == HomeNode.Faction
            || world.Geometry.TrianglesByEdgeIds[oEdge].Info.Faction == null)
        {
            return false;
        }

        return true;
    }
    public void Transfer(StrategicNode newHome)
    {
        if (HomeNode != null)
        {
            HomeNode.TakeFrontNode(this);
        }

        HomeNode = newHome;
        newHome.AddFrontNode(this);
        Armies.ForEach(a => a.SetGoal(HomeNode.Location.Position, HomeNode.Location, TargetLoc));
    }
    public void AddEdge(int edge, WorldManager world)
    {
        if (BorderEdges.Contains(edge) == false)
        {
            BorderEdges.Add(edge);
            if (world.Strategic.EdgeFrontNodes.ContainsKey(edge))
            {
                var oldFn = world.Strategic.EdgeFrontNodes[edge];
                oldFn.RemoveEdge(edge, world);
            }
            world.Strategic.EdgeFrontNodes.Add(edge, this);
        }
    }

    public void RemoveEdge(int edge, WorldManager world)
    {
        if (BorderEdges.Contains(edge))
        {
            BorderEdges.Remove(edge);
            world.Strategic.EdgeFrontNodes.Remove(edge);
        }
    }

    public List<Army> Disband(WorldManager world)
    {
        for (var i = BorderEdges.Count - 1; i >= 0; i--)
        {
            var edge = BorderEdges[i];
            RemoveEdge(edge, world);
        }
        world.Strategic.RemoveFrontNode(this);
        var armies = Armies.ToList();
        Armies.Clear();
        return armies;
    }
    public float GetReinforcementNeed(WorldManager world)
    {
        return Mathf.Max(0f, BorderEdges.Count - Armies.Count);
    }
    public void GetArmies(List<Army> armies, WorldManager world)
    {
        Armies.AddRange(armies);
        DeployArmies(world);
    }
    public void GetArmy(Army army, WorldManager world)
    {
        Armies.Add(army);
        DeployArmies(world);
    }
    private void DeployArmies(WorldManager world)
    {
        if (BorderEdges.Count == 0) return;
        
        var avgPos = Vector2.Zero;
        BorderEdges.ForEach(e => avgPos += world.Geometry.GetEdgeMidPoint(e));
        avgPos /= BorderEdges.Count;

        var borderEdgesSorted = world.Geometry.SortEdges(BorderEdges)[0];
        
        var neighbors = world.Locations.LocationGraph[HomeNode.Location].Neighbors
            .Where(n => n.Tri.Info.Faction != HomeNode.Faction);
        
        if (neighbors.Count() == 0)
        {
            TargetLoc = HomeNode.Location;
        }
        else
        {
           TargetLoc = neighbors
                       .OrderBy(n => n.Position.DistanceTo(avgPos))
                       .First(); 
        }
        
        var backUp = (avgPos - HomeNode.Location.Position).Normalized() * 10f;
        var frontPoints = new List<Vector2>();
        
        frontPoints.Add(world.Geometry.PointsById[world.Geometry.From[BorderEdges[0]]]);
        for (int i = 0; i < borderEdgesSorted.Count; i++)
        {
            var e = borderEdgesSorted[i];
            var to = world.Geometry.PointsById[world.Geometry.To[e]];
            frontPoints.Add(to - backUp);
        }
        var frontline = new Frontline(
            world,
            frontPoints,
            t => t.GetFrontageCost()
        );
        var points = frontline.GetFrontStartEndPoints(world, Armies, army => 1f);
        for (var i = 0; i < Armies.Count; i++)
        {
            var army = Armies[i];
            army.SetFlanks(points[i], points[i + 1]);
            var mid = (points[i] + points[i + 1]) / 2f;
            
            army.SetGoal(mid, HomeNode.Location, TargetLoc);
        }
    }

    public Vector2 GetApproxPos(GeometryManager geometry)
    {
        var pos = Vector2.Zero;
        if (BorderEdges.Count > 0)
        {
            for (var i = 0; i < BorderEdges.Count; i++)
            {
                pos += geometry.GetEdgeMidPoint(BorderEdges[i]);
            }

            pos /= BorderEdges.Count;
        }
        
        return pos;
    }

    public void Check(WorldManager world)
    {
        var geometry = world.Geometry;
        TrimEnds(world);

        if (BorderEdges.Count == 0)
        {
            HomeNode.DisbandFrontNode(this, world);
            return;
        }
        
        CheckStratNodeClosest(world);

        var contiguous = CheckContiguous(world);
        if (contiguous == false) return;
        
        DeployArmies(world);
    }

    private void TrimEnds(WorldManager world)
    {
        int trimFromStart = 0;
        for (int i = 0; i < BorderEdges.Count; i++)
        {
            var edge = BorderEdges[i];
            if (EdgeGoodForBorder(edge, world) == false)
            {
                trimFromStart++;
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < trimFromStart; i++)
        {
            var remove = BorderEdges[0];
            RemoveEdge(remove, world);
        }
        
        int trimFromEnd = 0;
        for (int i = BorderEdges.Count - 1; i >= 0; i--)
        {
            var edge = BorderEdges[i];
            if (EdgeGoodForBorder(edge, world) == false)
            {
                trimFromEnd++;
            }
            else
            {
                break;
            }
        }
        for (int i = 0; i < trimFromEnd; i++)
        {
            var remove = BorderEdges[BorderEdges.Count - 1];
            RemoveEdge(remove, world);
        }
    }

    private void CheckStratNodeClosest(WorldManager world)
    {
        var stratNodesInGraph = HomeNode.Theater.SubGraph.Elements;
        var approxPos = GetApproxPos(world.Geometry);

        var closestStratNode = HomeNode.Theater.StrategicNodes.Values
            .OrderBy(sn => sn.Location.Position.DistanceTo(approxPos))
            .First();
        if (closestStratNode != HomeNode)
        {
            Transfer(closestStratNode);
        }
    }

    private bool CheckContiguous(WorldManager world)
    {
        var geometry = world.Geometry;
        bool bad = false;
        
        for (var i = BorderEdges.Count - 1; i >= 0; i--)
        {
            var edge = BorderEdges[i];
            if (i != 0)
            {
                var prev = BorderEdges[i - 1];
                if (geometry.From[edge] != geometry.To[prev])
                {
                    bad = true;
                }
            }
            if (EdgeGoodForBorder(edge, world) == false)
            {
                RemoveEdge(edge, world);
                bad = true;
            }
        }

        if (bad)
        {
            SplitNoncontiguousFront(world);
            return false;
        }
        return true;
    }
    private void SplitNoncontiguousFront(WorldManager world)
    {
        var sorted = world.Geometry.SortEdges(BorderEdges);
        var newFronts = new List<FrontNode>();
        var armyNewFrontDic = new Dictionary<FrontNode, List<Army>>();
        foreach (var sortedBorder in sorted)
        {
            if (sortedBorder.Count == 0) continue;
            var fn = world.Strategic.AddFrontNode(HomeNode);
            newFronts.Add(fn);
            armyNewFrontDic.Add(fn, new List<Army>());
            for (var i = 0; i < sortedBorder.Count; i++)
            {
                RemoveEdge(sortedBorder[i], world);
                fn.AddEdge(sortedBorder[i], world);
            }
        }
        for (var i = Armies.Count - 1; i >= 0; i--)
        {
            var army = Armies[i];
            Armies.Remove(army);
            var closeFront = newFronts
                .OrderBy(fn => fn
                    .GetApproxPos(world.Geometry).DistanceTo(army.GetPosition()))
                .First();
            armyNewFrontDic[closeFront].Add(army);
        }
        foreach (var keyValuePair in armyNewFrontDic)
        {
            var newFront = keyValuePair.Key;
            var armies = keyValuePair.Value;
            newFront.GetArmies(armies, world);
        }
        HomeNode.DisbandFrontNode(this, world);
    }
}
