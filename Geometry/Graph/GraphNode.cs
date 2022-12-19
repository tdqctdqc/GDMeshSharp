using Godot;
using System;
using System.Collections.Generic;

public class GraphNode<TNode, TEdge>
{
    public TNode Element {get; private set;}
    private Dictionary<TNode, GraphNode<TNode, TEdge>> _nodeDic;
    public List<TNode> Neighbors { get; private set; }
    private Dictionary<TNode, TEdge> _costs;
    public GraphNode(TNode element)
    {
        Element = element;
        Neighbors = new List<TNode>();
        _nodeDic = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
        _costs = new Dictionary<TNode, TEdge>();
    }

    public bool HasNeighbor(TNode t)
    {
        return _costs.ContainsKey(t);
    }
    public TEdge GetEdgeCost(GraphNode<TNode, TEdge> neighbor)
    {
        return _costs[neighbor.Element];
    }

    public void SetEdgeValue(GraphNode<TNode, TEdge> neighbor, TEdge newEdgeVal)
    {
        _costs[neighbor.Element] = newEdgeVal;
    }
    public void AddNeighbor(GraphNode<TNode, TEdge> neighbor, TEdge edge)
    {
        if (_costs.ContainsKey(neighbor.Element))
        {
            return;
        }
        Neighbors.Add(neighbor.Element);
        _nodeDic.Add(neighbor.Element, neighbor);
        _costs.Add(neighbor.Element, edge);
    }

    public void RemoveNeighbor(GraphNode<TNode, TEdge> neighbor)
    {
        Neighbors.Remove(neighbor.Element);
        _costs.Remove(neighbor.Element);
        _nodeDic.Remove(neighbor.Element);
    }
}
