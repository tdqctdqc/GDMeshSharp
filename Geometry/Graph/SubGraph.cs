using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class SubGraph<TNode, TEdge>
{
    public Graph<TNode, TEdge> Graph { get; private set; }
    public List<TNode> Elements { get; private set; }
    private Dictionary<TNode, GraphNode<TNode, TEdge>> _nodeDic;
    private Dictionary<TNode, GraphNode<TNode, TEdge>> _frontierDic;

    public SubGraph(Graph<TNode, TEdge> graph)
    {
        Elements = new List<TNode>();
        Graph = graph;
        _nodeDic = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
        _frontierDic = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
    }
    public bool Expand()
    {
        if (_frontierDic.Count == 0) return false;
        foreach (var t in _frontierDic.Keys)
        {
            if (Graph.NodeSubGraphs.ContainsKey(t) == false)
            {
                AddNode(t);
                return true;
            }
        }
        return false;
    }

    public List<TNode> GetBorder()
    {
        return _nodeDic.Values
            .Where(node => 
                node.Neighbors.Exists(n => 
                    _frontierDic.ContainsKey(n)))
            .Select(node => node.Element)
            .ToList();
    }
    public void AddNode(TNode t)
    {
        if (Elements.Contains(t)) return;
        Elements.Add(t);
        var node = Graph[t];
        _nodeDic.Add(t, node);
        _frontierDic.Remove(t);
        foreach (var n in node.Neighbors)
        {
            if (_nodeDic.ContainsKey(n) == false
                && _frontierDic.ContainsKey(n) == false)
            {
                _frontierDic.Add(n, Graph[n]);
            }
        }

        Graph.NodeSubGraphs[t] = this;
    }

    public List<SubGraph<TNode, TEdge>> Split(int numNewGraphs)
    {
        if (numNewGraphs > Elements.Count) throw new Exception();
        Graph.RemoveSubGraph(this);
        var newSubGraphs = Enumerable
            .Range(0, numNewGraphs)
            .Select(i => Graph.AddSubGraph())
            .ToList();
        var result = newSubGraphs.ToList();
        
        
        foreach (var newSubGraph in newSubGraphs)
        {
            var e = Elements.GetRandomElement();
            Elements.Remove(e);
            newSubGraph.AddNode(e);
        }

        while (newSubGraphs.Count > 0)
        {
            var sub = newSubGraphs[0];
            var success = sub.Expand();
            newSubGraphs.RemoveAt(0);
            if (success)
            {
                newSubGraphs.Add(sub);
            }
        }

        return result;
    }

    public void RemoveNode(TNode t)
    {
        Elements.Remove(t);
        var node = Graph[t];
        _nodeDic.Remove(t);
        for (var i = 0; i < node.Neighbors.Count; i++)
        {
            var n = node.Neighbors[i];
            if (_nodeDic.ContainsKey(n)) continue;
            var nNode = Graph[n];
            bool stillHasNeighbor = false;
            for (var j = 0; j < nNode.Neighbors.Count; j++)
            {
                if (_nodeDic.ContainsKey(nNode.Neighbors[j]))
                {
                    stillHasNeighbor = true;
                    break;
                }
            }

            if (stillHasNeighbor == false)
            {
                _frontierDic.Remove(n);
            }
        }

        Graph.NodeSubGraphs.Remove(t);
    }
    
    
    
}
