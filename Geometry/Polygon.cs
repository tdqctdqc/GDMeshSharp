using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Polygon
{
    public int Id { get; private set; }
    public List<int> BorderEdges { get; private set; }
    public List<Triangle> Triangles { get; private set; }
    public PolygonTerrainInfo Info { get; private set; }
    private PolygonManager _polygons;
    
    public Polygon(int id, GeometryManager geometry, PolygonManager polygons)
    {
        _polygons = polygons;
        Id = id;
        Info = new PolygonTerrainInfo();
        BorderEdges = new List<int>();
        Triangles = new List<Triangle>();
    }

    public void SetTriangles(HashSet<Triangle> tris, GeometryManager geometry)
    {
        Triangles = tris.ToList();
        BorderEdges = Triangles
            .SelectMany(t => t.HalfEdges)
            .Distinct()
            .Where(e => geometry.HalfEdgePairs[e] is int o == false
                        || tris.Contains(geometry.TrianglesByEdgeIds[o]) == false)
            .ToList();
    }
    
    public void AddTriangle(Triangle tri, GeometryManager geometry)
    {
        if (Triangles.Contains(tri)) return;
        Triangles.Add(tri);
        foreach (var edge in tri.HalfEdges)
        {
            if (geometry.HalfEdgePairs[edge] is int opposingEdge)
            {
                RemoveOutsideEdge(opposingEdge);
                
                var opposingTri = geometry.TrianglesByEdgeIds[opposingEdge];
                if (Triangles.Contains(opposingTri) == false)
                {
                    AddOutsideEdge(edge);
                }
            }
            else
            {
                AddOutsideEdge(edge);
            }
        }
    }

    public void RemoveTriangle(Triangle tri, GeometryManager geometry)
    {
        RemoveTriPriv(tri,geometry);
    }

    private void RemoveTriPriv(Triangle tri, GeometryManager geometry)
    {
        if (Triangles.Contains(tri) == false) return;
        foreach (var e in tri.HalfEdges)
        {
            if (geometry.HalfEdgePairs[e] is int o)
            {
                if (geometry.TrianglesByEdgeIds.ContainsKey(o))
                {
                    var oTri = geometry.TrianglesByEdgeIds[o];
                    if (Triangles.Contains(oTri))
                    {
                        AddOutsideEdge(o);
                    }
                }
            }
            RemoveOutsideEdge(e);
        }
        Triangles.Remove(tri);
    }

    public void DecomposeTriangle(GeometryManager geometry, 
        Triangle tri, Triangle[] newTris)
    {
        if (Triangles.Contains(tri) == false) return;
        RemoveTriPriv(tri, geometry);
        foreach (var newTri in newTris)
        {
            _polygons.AddTriToPolygon(this, newTri);
        }
    }

    private void AddOutsideEdge(int outsideEdge)
    {
        BorderEdges.Add(outsideEdge);
    }
    private void RemoveOutsideEdge(int outsideEdge)
    {
        BorderEdges.Remove(outsideEdge);
    }

    public List<int> GetShoreEdges(GeometryManager geometry)
    {
        bool oppEdgeIsWater(int edge)
        {
            if (geometry.HalfEdgePairs[edge] is int o)
            {
                return geometry.TrianglesByEdgeIds[o].Info.IsWater;
            }
            return false;
        }
        return Triangles
            .Where(t => t.Info.IsLand)
            .SelectMany(t => t.HalfEdges)
            .Where(e => oppEdgeIsWater(e))
            .ToList();
    }
    public List<Polygon> GetAdjacentPolygons(GeometryManager geometry)
    {
        var neighbors = new HashSet<Polygon>();
        var edgePairs = geometry.HalfEdgePairs;
        var edgeTriLookup = geometry.TrianglesByEdgeIds;
        var triPolygonLookup = _polygons.TrianglePolygons;
        foreach (var e in BorderEdges)
        {
            if (edgePairs[e] is int o)
            {
                var tri = edgeTriLookup[o];
                if (triPolygonLookup.ContainsKey(tri))
                {
                    neighbors.Add(triPolygonLookup[tri]);
                }
            }
        }
        return neighbors.ToList();
    }

    public List<List<int>> GetSortedEdgesBorderingPolygon(Polygon poly, GeometryManager geometry)
    {
        var edges = GetEdgesBorderingPolygon(poly, geometry);
        return geometry.SortEdges(edges);
    }
    public List<int> GetEdgesBorderingPolygon(Polygon poly, GeometryManager geometry)
    {
        var borderEdges = new List<int>();
        foreach (var borderEdge in BorderEdges)
        {
            if (geometry.HalfEdgePairs[borderEdge] is int oppEdge)
            {
                var oTri = geometry.TrianglesByEdgeIds[oppEdge];
                if (_polygons.TrianglePolygons.ContainsKey(oTri) == false)
                    continue;
                if(_polygons.TrianglePolygons[oTri] == poly)
                    borderEdges.Add(borderEdge);
            }
        }

        return borderEdges;
    }

    public List<Triangle> GetTrisBorderingPolygon(Polygon oPoly, GeometryManager geometry, 
            PolygonManager polygons)
    {
        var borderTris = new HashSet<Triangle>();

        foreach (var borderEdge in BorderEdges)
        {
            if (geometry.HalfEdgePairs[borderEdge] is int oppEdge)
            {
                if (oPoly.BorderEdges.Contains(oppEdge))
                {
                    borderTris.Add(geometry.TrianglesByEdgeIds[borderEdge]);
                }
            }
        }
        return borderTris.ToList();
    }

    public Vector2 GetCenter(GeometryManager geometry)
    {
        Vector2 center = Vector2.Zero;
        foreach (var tri in Triangles)
        {
            center += tri.GetCentroid(geometry);
        }

        return center / (float) Triangles.Count;
    }
    public float GetDriftFrictionStrength(Polygon oPoly, WorldManager world)
    {
        var drift1 = GetDriftToPolyPlate(oPoly, world);
        var drift2 = oPoly.GetDriftToPolyPlate(this, world);
        return (drift1 + drift2) / 2f;
    }

    private float GetDriftToPoly(Polygon oPoly, WorldManager world)
    {
        var plate = world.Plates.CellPlates[this];
        var polyCenter = GetCenter(world.Geometry);
        var oPolyCenter = oPoly.GetCenter(world.Geometry);
        var axis = oPolyCenter - polyCenter;
        var posNeg = Mathf.Abs(plate.Drift.AngleTo(axis)) > Mathf.Pi / 2f
            ? -1f
            : 1f;
        return posNeg * GeometryExt.GetProjectionLength(plate.Drift, axis);
    }
    private float GetDriftToPolyPlate(Polygon oPoly, WorldManager world)
    {
        var plate = world.Plates.CellPlates[this];
        var oPlate = world.Plates.CellPlates[oPoly];
        var plateCenter = plate.GetCenter(world.Geometry);
        var oPlateCenter = oPlate.GetCenter(world.Geometry);
        var axis = oPlateCenter - plateCenter;
        var posNeg = Mathf.Abs(plate.Drift.AngleTo(axis)) > Mathf.Pi / 2f
            ? -1f
            : 1f;
        return posNeg * GeometryExt.GetProjectionLength(plate.Drift, axis);
    }
    public List<List<Triangle>> GetPolygonDivision(GeometryManager geometry, 
        PolygonManager polygons, 
        Func<Triangle, Triangle, bool> comparer)
    {
        var fragments = new List<List<Triangle>>();
        var tris = Triangles.ToList();
        while (tris.Count > 0)
        {
            var tri = tris[0];
            var fragment = new List<Triangle> {tri};
            tris.Remove(tri);
            fragment.Add(tri);
            var frontierTris = new List<Triangle>();
            frontierTris.Add(tri);
            while (frontierTris.Count > 0)
            {
                var frontierTri = frontierTris[0];
                frontierTris.Remove(frontierTri);
                var adjacents = frontierTri
                    .GetTrisSharingEdge(geometry)
                    .Where(t => Triangles.Contains(t))
                    .Where(t => fragment.Contains(t) == false);
                foreach (var adj in adjacents)
                {
                    if (comparer(tri, adj))
                    {
                        fragment.Add(adj);
                        tris.Remove(adj);
                        frontierTris.Add(adj);
                    }
                }
            }
            fragments.Add(fragment);
        }
        return fragments;
    }

    public void SetPolygonIsLand(bool isLand)
    {
        Info.SetIsLand(isLand);
        foreach (var tri in Triangles)
        {
            tri.Info.SetIsLand(isLand);
        }
    }
}
