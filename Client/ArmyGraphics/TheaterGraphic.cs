using Godot;
using System;
using System.Collections.Generic;

public class TheaterGraphic : Node2D
{
    private List<StrategicNodeGraphic> _frontNodeGraphics;
    private TheaterHQGraphic _hq;
    private Theater _theater;
    public void Setup(Theater theater, WorldManager world)
    {
        _theater = theater;
        var geometry = world.Geometry;
        var locGraph = world.Locations.LocationGraph;
        _frontNodeGraphics = new List<StrategicNodeGraphic>();
        foreach (var frontNode in theater.StrategicNodes.Values)
        {
            var frontNodeGraphic = StrategicNodeGraphic.GetGraphic(frontNode, world);
            _frontNodeGraphics.Add(frontNodeGraphic);
            AddChild(frontNodeGraphic);
        }
        _hq = TheaterHQGraphic.GetGraphic(theater);
        AddChild(_hq);
    }

    public void Update(WorldManager world)
    {
        foreach (var frontNodeGraphic in _frontNodeGraphics)
        {
            frontNodeGraphic.Update(world);
        }

        _hq.Position = _theater.Position;
    }
}
