using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeometryGraphics : Node2D
{
    private MeshInstance2D _triEdgeNode;
    private Color _triEdgeColor = new Color(0f, 0f, 0f, 0f);
    private GeometryManager _geometry;
    public void Setup(GeometryManager geometry)
    {
        _geometry = geometry;
    }

    public void Start()
    {
        _triEdgeNode?.Free();
        var triEdges = _geometry.From.Keys.ToList();
        _triEdgeNode = MeshGenerator.GetEdgesMesh(triEdges, 5f, _geometry);
        _triEdgeNode.Modulate = _triEdgeColor;
        AddChild(_triEdgeNode);
    }

    public void Stop()
    {
        
    }


}
