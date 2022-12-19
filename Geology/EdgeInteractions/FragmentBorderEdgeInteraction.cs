using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using InteractionAction = System.Action<GeometryManager, PolygonManager, Polygon, Polygon>;

public class FragmentBorderEdgeInteraction : EdgeInteraction
{
    public FragmentBorderEdgeInteraction() 
    {
    }

    protected override void Interaction(GeometryManager geometry, PolygonManager polygons, Polygon poly1, Polygon poly2)
    {
        
        GeologyUtility.SplitBorderEdges(geometry, polygons, poly1, poly2, 2f);
        GeologyUtility.DisturbEdges(geometry, polygons, poly1, poly2, 2f);
    }

    protected override bool Criterion1(Polygon poly)
    {
        return true;
    }

    protected override bool Criterion2(Polygon poly)
    {
        return true;
    }
}
