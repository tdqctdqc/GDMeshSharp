using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Commands
{
    private WorldManager _world;

    public Commands(WorldManager world)
    {
        _world = world;
    }

    public void ClearAllPolygons()
    {
        _world.GeologyPolygons.Clear();
    }

    public void DoTest()
    {
    }
    public void DoMapAction(MapAction action)
    {
        Geology.DoMapAction(action, _world);
    }
    public void DoGeologyEdgeInteraction(EdgeInteraction i)
    {
        Geology.DoEdgeInteraction(i, _world);
    }

    public void DoGeologySelfAction(PolygonSelfAction a)
    {
        Geology.DoPolygonSelfAction(a, _world);
    }
    
    public void BuildPlates(int numContinents, int numPlatesPerContinent, float percentLand)
    {
        
        Geology.DoMapAction(new BuildPlates(numContinents, numPlatesPerContinent,
            _world.Geometry.Dimensions, percentLand), _world);
        
    }

    public void TransferTriangle(Vector2 mousePos)
    {
        var tri = _world.Geometry.TriLookup.GetTriAtPosition(mousePos, _world.Geometry);
        if (tri == null) return;
        for (int i = 0; i < 3; i++)
        {
            var e = tri.HalfEdges[i];
            if (_world.Geometry.HalfEdgePairs[e] is int o
                && _world.Geometry.TrianglesByEdgeIds[o].Info.Faction is Faction f
                && f != tri.Info.Faction)
            {
                _world.Factions.ChangeTriFaction(tri, f);
                break;
            }
        }
    }
}
