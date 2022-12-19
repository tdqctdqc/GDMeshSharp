using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PolygonGraphics : Node2D
{
    private MeshInstance2D _triNode, _polygonEdgeNode;
    private Color _polygonEdgeColor;
    private WorldManager _world;
    private float _opacity = 1f;
    public void Setup(WorldManager world)
    {
        _world = world;
        _polygonEdgeColor = Colors.Yellow;
    }

    public void Start()
    {
        _triNode?.Free();
        _triNode = MeshGenerator.GetTrisMesh(
            _world.Geometry, 
            _world.Geometry.Triangles,
            _world.Geometry.Triangles.Select(
                t => new Color(t.Info.GetColor(), _opacity)
                    .Darkened(.25f)).ToList()
        );
        AddChild(_triNode);
        
        _polygonEdgeNode?.Free();
        var polyEdges = _world.GeologyPolygons.Polygons.SelectMany(p => p.BorderEdges).ToList();
        if (polyEdges.Count == 0) return;
        _polygonEdgeNode = MeshGenerator.GetEdgesMesh(polyEdges, 5f, _world.Geometry);
        _polygonEdgeNode.Modulate = _polygonEdgeColor;
        AddChild(_polygonEdgeNode);
        _polygonEdgeNode.Visible = false;
    }

    public void Stop()
    {
    }

    public void SetEdgeVisibility(bool vis)
    {
        _polygonEdgeNode.Visible = vis;
    }
    

}
