using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class PolygonManager
{
    private GeometryManager _geometry;
    public List<Polygon> Polygons { get; private set; }
    public Graph<Polygon, PolygonEdgeInfo> Edges { get; private set; }
    public Dictionary<Triangle, Polygon> TrianglePolygons { get; private set; }
    public Action<Polygon, Triangle> PolygonAddedTri { get; set; }
    public Action<Polygon, Triangle> PolygonRemovedTri { get; set; }
    public Action<Polygon> AddedPolygon { get; set; }
    public Action<Polygon> RemovedPolygon { get; set; }
    private int _polyIdCounter;
    public PolygonManager(GeometryManager geometry)
    {
        Polygons = new List<Polygon>();
        _geometry = geometry;
        _geometry.TriangleDecomposed += TriangleDecomposed;
        TrianglePolygons = new Dictionary<Triangle, Polygon>();
    }
    public void BuildGraph(WorldManager world)
    {
        Edges = new Graph<Polygon, PolygonEdgeInfo>();
        for (var i = 0; i < Polygons.Count; i++)
        {
            var poly = Polygons[i];
            Edges.AddNode(poly);
        }
        
        void addNeighbors(Polygon poly)
        {
            var neighbors = poly.GetAdjacentPolygons(_geometry);
            for (int i = 0; i < neighbors.Count; i++)
            {
                var n = neighbors[i];
                Edges.AddDirectedEdge(poly, n, new PolygonEdgeInfo());
            }
        }
        Polygons.TryParallel(addNeighbors);
    }
    public Polygon AddNewPolygonWithTrisNoChecks(List<Triangle> tris)
    {
        var poly = AddPolygon();
        poly.SetTriangles(tris.ToHashSet(), _geometry);
        if (PolygonAddedTri != null)
        {
            tris.ForEach(t => PolygonAddedTri.Invoke(poly, t));
        }
        return poly;
    }
    public Polygon AddNewPolygonWithTris(List<Triangle> tris)
    {
        foreach (var tri in tris)
        {
            if (TrianglePolygons.ContainsKey(tri))
            {
                var oldPoly = TrianglePolygons[tri];
                if(oldPoly != null)
                    RemoveTriFromPolygon(oldPoly, tri);
            }
        }
        var poly = AddPolygon();
        for (int i = 0; i < tris.Count; i++)
        {
            AddTriToPolygon(poly, tris[i]);
        }
        return poly;
    }
    public Polygon AddNewPolygonWithTri(Triangle tri)
    {
        if (TrianglePolygons.ContainsKey(tri))
        {
            var oldPoly = TrianglePolygons[tri];
            RemoveTriFromPolygon(oldPoly, tri);
        }
        var poly = AddPolygon();
        AddTriToPolygon(poly, tri);

        return poly;
    }

    public void DividePolygon(Polygon poly, List<List<Triangle>> fragments,
        List<TerrainType> terrains)
    {
        RemovePolygon(poly);
        foreach (var fragment in fragments)
        {
            var terrain = Terrain.GetTerrainByRoughness(
                fragment[0].Info.Roughness.OutValue
            );
            var newPoly = AddNewPolygonWithTris(fragment);
        }
    }
    public void TransferTriToPolygon(Polygon poly, Triangle tri)
    {
        if (poly.Triangles.Contains(tri)) return;
        if (TrianglePolygons.ContainsKey(tri))
        {
            RemoveTriFromPolygon(TrianglePolygons[tri], tri);
        }
        AddTriToPolygon(poly, tri);
    }
    public void RemoveTriAtPointFromPolygon(Vector2 point)
    {
        var tri = _geometry.TriLookup.GetTriAtPosition(point, _geometry);
        if (tri == null) return;

        if (TrianglePolygons.ContainsKey(tri) == false) return;
        RemoveTriFromPolygon(TrianglePolygons[tri], tri);
    }
    
    public Polygon AddPolygon()
    {
        var poly = new Polygon(TakeId(), _geometry, this);
        RegisterPolygon(poly);
        return poly;
    }

    private void RegisterPolygon(Polygon poly)
    {
        AddedPolygon?.Invoke(poly);
        Polygons.Add(poly);
    }
    
    public void RemovePolygon(Polygon poly)
    {
        for (int i = poly.Triangles.Count - 1; i >= 0; i--)
        {
            var tri = poly.Triangles[i];
            RemoveTriFromPolygon(poly, tri);
        }
        Polygons.Remove(poly);
        RemovedPolygon?.Invoke(poly);
    }
    public void Clear()
    {
        for (int i = Polygons.Count - 1; i >= 0; i--)
        {
            foreach (var triangle in Polygons[i].Triangles)
            {
                triangle.Info.IncrementRoughness(-triangle.Info.Roughness.OutValue);
            }
            RemovePolygon(Polygons[i]);
        }
    }

    public void AddTriToPolygon(Polygon poly, Triangle tri)
    {
        poly.AddTriangle(tri, _geometry);
        if (TrianglePolygons.ContainsKey(tri)) TrianglePolygons.Remove(tri);
        TrianglePolygons.Add(tri, poly);
        PolygonAddedTri?.Invoke(poly, tri);
    }

    public void RemoveTriFromPolygon(Polygon poly, Triangle tri)
    {
        poly.RemoveTriangle(tri, _geometry);
        // if (poly.Triangles.Count == 0)
        // {
        //     RemovePolygon(poly);
        // }
        if (TrianglePolygons.ContainsKey(tri))
        {
            if(TrianglePolygons[tri] == poly) TrianglePolygons.Remove(tri);
        }
        PolygonRemovedTri?.Invoke(poly, tri);
    }

    public void ExpandPolygons(Func<Triangle, bool> canExpandInto)
    {
        var activePolygons = Polygons.ToList();

        bool expandPoly(Polygon poly)
        {
            for (int i = 0; i < poly.BorderEdges.Count; i++)
            {
                var e = poly.BorderEdges[i];
                if (_geometry.HalfEdgePairs[e] is int o)
                {
                    var oTri = _geometry.TrianglesByEdgeIds[o];
                    if (TrianglePolygons.ContainsKey(oTri) == false
                        && canExpandInto(oTri))
                    {
                        AddTriToPolygon(poly, oTri);
                        return true;
                    }
                }
            }
            return false;
        }
        
        
        while (activePolygons.Count > 0)
        {
            var poly = activePolygons[0];
            activePolygons.RemoveAt(0);
            bool active = expandPoly(poly);

            if (active)
            {
                activePolygons.Add(poly);
            }
        }
    }
    public void ChangePolygonIsLand(Polygon poly, bool isLand)
    {
        poly.SetPolygonIsLand(isLand);
    }
    private void TriangleDecomposed(Triangle tri, Triangle[] newTris)
    {
        if (TrianglePolygons.ContainsKey(tri))
        {
            TrianglePolygons[tri].DecomposeTriangle(_geometry, tri, newTris);
        }
    }
    private int TakeId()
    {
        _polyIdCounter++;
        return _polyIdCounter;
    }
}
