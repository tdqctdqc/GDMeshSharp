using Godot;
using System;
using System.Linq;

public class StrategicNodeGraphic : Node2D
{
    private static PackedScene _scene = (PackedScene) GD.Load("res://Client/ArmyGraphics/StrategicNodeGraphic.tscn");
    private Control _handle;
    private Node2D _mouseOverGraphics;
    private StrategicNode _strategicNode;
    private WorldManager _world;
    private Line2D _lineToArmies;
    public void Setup(StrategicNode strategicNode, WorldManager world)
    {
        _world = world;
        _strategicNode = strategicNode;
        Position = strategicNode.Location.Position;
        _handle = (Control) FindNode("Handle");
        _handle.Connect("mouse_entered", this, nameof(MouseEntered));
        _handle.Connect("mouse_exited", this, nameof(MouseExited));

        var facColor = (Node2D) FindNode("FactionColor");
        facColor.SelfModulate = strategicNode.Faction.Color;
        var theaterColor = (Node2D) FindNode("TheaterColor");
        theaterColor.SelfModulate = strategicNode.Theater.Color;


        _lineToArmies = new Line2D();
        _lineToArmies.Width = 3f;
        _lineToArmies.DefaultColor = Colors.Blue;
        AddChild(_lineToArmies);
        _lineToArmies.Visible = false; 
    }

    public void Update(WorldManager world)
    {
        _lineToArmies.ClearPoints();
        var armies = _strategicNode.FrontNodes.SelectMany(fe => fe.Armies);
        foreach (var army in armies)
        {
            _lineToArmies.AddPoint(Vector2.Zero);
            _lineToArmies.AddPoint(army.HqPosition - Position);
        }
    }
    public static StrategicNodeGraphic GetGraphic(StrategicNode node, WorldManager world)
    {
        var graphic = (StrategicNodeGraphic) _scene.Instance();
        graphic.Setup(node, world);
        return graphic;
    }

    private void MouseEntered()
    {
        _mouseOverGraphics = new Node2D();
        _mouseOverGraphics.Position = -Position;
        AddChild(_mouseOverGraphics);
        var theater = _strategicNode.Theater;
        var friendlyNeighbors = theater.SubGraph
            .Graph[_strategicNode.Location].Neighbors
            .Where(n => theater.StrategicNodes.ContainsKey(n));
        var enemyNeighbors = theater.SubGraph
            .Graph[_strategicNode.Location].Neighbors
            .Except(friendlyNeighbors);
        foreach (var friendlyNeighbor in friendlyNeighbors)
        {
            var frontline = MeshGenerator.GetLineMesh(
                _strategicNode.Location.Position,
                friendlyNeighbor.Position,
                5f);
            frontline.SelfModulate = theater.Faction.Color;
            _mouseOverGraphics.AddChild(frontline);
        }
        
        
        foreach (var enemyNeighbor in enemyNeighbors)
        {
            var frontline = MeshGenerator.GetLineMesh(
                _strategicNode.Location.Position,
                enemyNeighbor.Position,
                5f);
            frontline.SelfModulate = Colors.Red;
            _mouseOverGraphics.AddChild(frontline);
        }
        int iter = 0;
        foreach (var frontNode in _strategicNode.FrontNodes)
        {
            if (frontNode.BorderEdges.Count == 0) continue;
            var lines = MeshGenerator.GetEdgesMesh(frontNode.BorderEdges, 10f, _world.Geometry, null, true);
            _mouseOverGraphics.AddChild(lines);
            lines.SelfModulate = ColorsExt.GetRainbowColor(iter);
            iter++;
        }

        _lineToArmies.Visible = true;
    }
    private void MouseExited()
    {
        _mouseOverGraphics?.Free();
        _mouseOverGraphics = null;

        _lineToArmies.Visible = false;
    }
}
