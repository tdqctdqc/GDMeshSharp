using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class FactionGraphics : Node2D
{
    private WorldManager _world;
    private Node2D _borderHolderNode;
    private Node2D _theaterHolderNode;
    private List<TheaterGraphic> _theaterGraphics; 
    public void Setup(WorldManager world)
    {
        _world = world;
    }

    public void Start()
    {
        _borderHolderNode?.Free();
        _borderHolderNode = new Node2D();
        foreach (var f in _world.Factions.Factions)
        {
            var borders = f.Territory.BorderEdges;
            var borderMesh = MeshGenerator.GetEdgesMesh(borders, 3f, _world.Geometry);
            borderMesh.SelfModulate = f.Color;
            _borderHolderNode.AddChild(borderMesh);
        }
        AddChild(_borderHolderNode);


        _theaterGraphics = new List<TheaterGraphic>();
        _theaterHolderNode?.Free();
        _theaterHolderNode = new Node2D();
        foreach (var f in _world.Factions.Factions)
        {
            int iter = 0;
            foreach (var theater in f.Strategy.Theaters)
            {
                var theaterGraphic = new TheaterGraphic();
                theaterGraphic.Setup(theater, 
                    _world);
                _theaterHolderNode.AddChild(theaterGraphic);
                _theaterGraphics.Add(theaterGraphic);
                iter++;
            }
        }
        AddChild(_theaterHolderNode);
    }

    public override void _Process(float delta)
    {
        foreach (var theaterGraphic in _theaterGraphics)
        {
            theaterGraphic.Update(_world);
        }
    }

    public void Stop()
    {
        _borderHolderNode?.Free();
        _borderHolderNode = null;
        
        _theaterHolderNode?.Free();
        _theaterHolderNode = null;
        _theaterGraphics.Clear();
    }

    public void SetBorderVisibility(bool vis)
    {
        if (_borderHolderNode == null) return;
        _borderHolderNode.Visible = vis;
    }
    public void SetTheaterVisibility(bool vis)
    {
        if (_theaterHolderNode == null) return;
        _theaterHolderNode.Visible = vis;
    }
}
