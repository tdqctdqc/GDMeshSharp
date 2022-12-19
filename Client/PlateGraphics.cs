using Godot;
using System;
using System.Linq;

public class PlateGraphics : Node2D
{
    private WorldManager _world;
    private MeshInstance2D _edgeNode;
    private Color _edgeColor;
    public void Setup(WorldManager world)
    {
        _world = world;
        _edgeColor = Colors.Red;
    }
    public void Start()
    {
        _edgeNode?.Free();

        var edges = _world.Plates.Plates.SelectMany(p => p.GetBorderEdges(_world)).ToList();
        if (edges.Count == 0) return;
        _edgeNode = MeshGenerator.GetEdgesMesh(edges, 15f, _world.Geometry);
        _edgeNode.Modulate = _edgeColor;
        AddChild(_edgeNode);
        _edgeNode.Visible = false;
    }
    public void Stop()
    {
        
    }

    public void SetEdgeVisibility(bool vis)
    {
        _edgeNode.Visible = vis;
    }
}
