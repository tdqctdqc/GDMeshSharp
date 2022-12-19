using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GraphGenerator
{
    public static Graph<TNode, TEdge> GenerateDelaunayGraph<TNode, TEdge>
        (List<TNode> elements, Func<TNode, Vector2> posFunc,
            Func<TNode,TNode,TEdge> getEdgeFunc)
    {
        var graph = new Graph<TNode, TEdge>();
        var poses = new List<Vector2>();
        for (int i = 0; i < elements.Count; i++)
        {
            poses.Add(posFunc(elements[i]));
            var node = new GraphNode<TNode, TEdge>(elements[i]);
            graph.AddNode(node);
        }
        var tris = DelaunayTriangulator.TriangulatePoints(poses);
        for (int i = 0; i < tris.Count; i+=3)
        {
            var elementA = elements[poses.IndexOf(tris[i])];
            var a = graph[elementA];
            
            var elementB = elements[poses.IndexOf(tris[i + 1])];
            var b = graph[elementB];
            
            var elementC = elements[poses.IndexOf(tris[i + 2])];
            var c = graph[elementC];

            graph.AddUndirectedEdge(a, b, getEdgeFunc(a.Element, b.Element));
            graph.AddUndirectedEdge(a, c, getEdgeFunc(a.Element, c.Element));
            graph.AddUndirectedEdge(c, b, getEdgeFunc(c.Element,b.Element));
        }

        return graph;
    }
    public static Graph<Vector2, float> GenerateDelaunayGraph(List<Vector2> poses)
    {
        var graph = new Graph<Vector2, float>();

        foreach (var pos in poses)
        {
            var node = new GraphNode<Vector2, float>(pos);
            graph.AddNode(node);
        }
        var tris = DelaunayTriangulator.TriangulatePoints(poses);
        for (int i = 0; i < tris.Count; i+=3)
        {
            var a = graph[tris[i]];
            var b = graph[tris[i+1]];
            var c = graph[tris[i+2]];

            graph.AddUndirectedEdge(a,b,1f);
            graph.AddUndirectedEdge(a,c,1f);
            graph.AddUndirectedEdge(c,b,1f);
        }

        return graph;
    }
    public static Graph<Vector2, float> GenerateSpanningGraph(Graph<Vector2, float> delaunayGraph,
                                                        int branchLength,
                                                        int maxVertexDegree)
    {
        var graph = new Graph<Vector2, float>();
        foreach (var e in delaunayGraph.Elements)
        {
            graph.AddNode(e);
        }
        var openNodes = new List<GraphNode<Vector2, float>>(graph.Nodes);

        while(openNodes.Count > 0)
        {
            var node = openNodes[0];
            var branchNodes = new List<GraphNode<Vector2, float>>();
            int branch = 0;
            branchNodes.Add(node);
            openNodes.Remove(node);
            
            while(branch < branchLength)
            {
                var backingNode = delaunayGraph[node.Element];
                var openNs = backingNode.Neighbors
                    .Where(n => 
                        branchNodes.Contains(delaunayGraph[n]) == false
                    )
                    .Select(n => delaunayGraph[n]);
                if(openNs.Count() == 0) break;
                var next = openNs.ElementAt(0);
                graph.AddUndirectedEdge(node, next, 1f);
                node = next;
                branch++;
                branchNodes.Add(node);
                openNodes.Remove(node);
            }
        }
        return graph; 
    }
}
