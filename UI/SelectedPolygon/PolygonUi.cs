using Godot;
using System;
using System.Linq;

public class PolygonUi : Node
{
    private Control _container; 
    public override void _Ready()
    {
        _container = GetNode<VBoxContainer>("VBoxContainer");

        var clearAllPolys = new ActionButton();
        clearAllPolys.Setup("Clear All Polygons", () => Game.I.Commands.ClearAllPolygons());

        var showPolyEdges = CheckBoxEntry.GetCheckBoxEntry("Show Poly Edges", Game.I.Graphics.PolygonGraphics.SetEdgeVisibility);
        _container.AddChild(showPolyEdges);        
        
        var showPlateEdges = CheckBoxEntry.GetCheckBoxEntry("Show Plate Edges", Game.I.Graphics.PlateGraphics.SetEdgeVisibility);
        _container.AddChild(showPlateEdges);

        var showBorders = CheckBoxEntry.GetCheckBoxEntry("Show Borders", Game.I.Graphics.FactionGraphics.SetBorderVisibility);
        _container.AddChild(showBorders);
    }
}
