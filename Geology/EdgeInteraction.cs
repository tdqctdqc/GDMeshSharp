using Godot;
using System;
using InteractionAction = System.Action<GeometryManager, PolygonManager, Polygon, Polygon>;

public abstract class EdgeInteraction
{

    public EdgeInteraction()
    {
    }

    protected abstract void Interaction(GeometryManager geometry, PolygonManager polygons, Polygon p1, Polygon p2);
    protected abstract bool Criterion1(Polygon poly);
    protected abstract bool Criterion2(Polygon poly);
    public void DoInteraction(GeometryManager geometry, PolygonManager polygons, Polygon p1, Polygon p2)
    {
        if ( (Criterion1(p1) == false || Criterion2(p2) == false) 
            && 
            (Criterion1(p2) == false || Criterion2(p1) == false) ) 
        { throw new Exception(); }
            
        Interaction(geometry, polygons, p1, p2);
    }

    public bool CheckInteractionValid(Polygon p1, Polygon p2)
    {
        return Criterion1(p1) && Criterion2(p2) 
               ||
               Criterion1(p2) && Criterion2(p1);
    }
}

