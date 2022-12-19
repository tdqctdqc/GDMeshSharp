using Godot;
using System;
using System.Collections.Generic;

public class PolygonReactiveGraphics : Node2D
{
    private Dictionary<int, Node2D> _triGraphics;
    private Node2D _triNode;
    private Dictionary<int, Node2D> _edgeGraphics;
    private Node2D _edgeNode;
    private Color _edgeColor;
    private GeometryManager _geometry;
    private PolygonManager _polygons;
    private float _opacity = .5f;
    public void Setup(GeometryManager geometry, PolygonManager polygons)
    {
        _polygons = polygons;
        _geometry = geometry;
        _edgeColor = Colors.Yellow;
        _triGraphics = new Dictionary<int, Node2D>();
        _triNode = new Node2D();
        AddChild(_triNode);
        _edgeGraphics = new Dictionary<int, Node2D>();
        _edgeNode = new Node2D();
        AddChild(_edgeNode);
    }

    public void Start()
    {
        foreach (var p in _polygons.Polygons)
        {
            AddPolygon(p);
            p.Triangles.ForEach(t => AddTriangle(p, t));
            p.BorderEdges.ForEach(e => AddOutsideEdge(e));
        }
        _polygons.AddedPolygon += AddPolygon;
        _polygons.RemovedPolygon += RemovePolygon;
        _polygons.PolygonAddedTri += AddTriangle;
        _polygons.PolygonRemovedTri += RemoveTriangle;

        _geometry.TriangleUpdated += UpdateTriangle;
    }

    public void Stop()
    {
        _polygons.AddedPolygon -= AddPolygon;
        _polygons.RemovedPolygon -= RemovePolygon;
        _polygons.PolygonAddedTri -= AddTriangle;
        _polygons.PolygonRemovedTri -= RemoveTriangle;
        _geometry.TriangleUpdated -= UpdateTriangle;
    }

    public override void _ExitTree()
    {
        _polygons.Polygons.ForEach(p => RemovePolygon(p));
        _polygons.AddedPolygon -= AddPolygon;
        _polygons.RemovedPolygon -= RemovePolygon;
        _geometry.TriangleUpdated -= UpdateTriangle;
    }
    public void AddPolygon(Polygon poly)
    {
    }
    public void RemovePolygon(Polygon poly)
    {
        foreach (var t in poly.Triangles)
        {
            RemoveOutsideEdge(t.HalfEdges[0]);
            RemoveOutsideEdge(t.HalfEdges[1]);
            RemoveOutsideEdge(t.HalfEdges[2]);
            _triGraphics[t.Id].Free();
            _triGraphics.Remove(t.Id);
        }
    }

    private void UpdateTriangle(Triangle tri)
    {
        if (_polygons.TrianglePolygons.ContainsKey(tri))
        {
            foreach (var edge in tri.HalfEdges)
            {
                if (_edgeGraphics.ContainsKey(edge))
                {
                    RemoveOutsideEdge(edge);
                    AddOutsideEdge(edge);
                }
            }
            var poly = _polygons.TrianglePolygons[tri];
            RemoveTriangle(poly, tri);
            AddTriangle(poly, tri);
        }
    }
    private void AddTriangle(Polygon poly, Triangle tri)
    {
        var mesh = MeshGenerator.GetTriMesh(_geometry, tri);
        mesh.Modulate = new Color(tri.Info.TerrainType.Color, _opacity);
        _triNode.AddChild(mesh);
        _triGraphics.Add(tri.Id, mesh);

        void checkEdgeOutside(int edge)
        {
            if (_geometry.HalfEdgePairs[edge] is int oEdge)
            {
                var oTri = _geometry.TrianglesByEdgeIds[oEdge];
                if(poly.Triangles.Contains(oTri))
                {
                    RemoveOutsideEdge(oEdge);
                    return;
                }
                else
                {
                    AddOutsideEdge(edge);
                }
            }
            else
            {
                AddOutsideEdge(edge);
            }
        }
        checkEdgeOutside(tri.HalfEdges[0]);
        checkEdgeOutside(tri.HalfEdges[1]);
        checkEdgeOutside(tri.HalfEdges[2]);
    }
    private void RemoveTriangle(Polygon poly, Triangle tri)
    {
        if (_triGraphics.ContainsKey(tri.Id))
        {
            _triGraphics[tri.Id].Free();
            _triGraphics.Remove(tri.Id);
        }
        void checkEdgeOutside(int edge)
        {
            RemoveOutsideEdge(edge);
            if (_geometry.HalfEdgePairs[edge] is int oEdge)
            {
                var oTri = _geometry.TrianglesByEdgeIds[oEdge];
                if(poly.Triangles.Contains(oTri))
                {
                    AddOutsideEdge(oEdge);
                }
                else
                {
                    return;
                }
            }
        }
        checkEdgeOutside(tri.HalfEdges[0]);
        checkEdgeOutside(tri.HalfEdges[1]);
        checkEdgeOutside(tri.HalfEdges[2]);
    }
    private void AddOutsideEdge(int edge)
    {
        if (_edgeGraphics.ContainsKey(edge)) return;
        var fromId = _geometry.From[edge];
        var from = _geometry.PointsById[fromId];
        var toId = _geometry.To[edge];
        var to = _geometry.PointsById[toId];
        var edgeMesh = MeshGenerator.GetLineMesh(from, to, 1f);
        edgeMesh.Modulate = _edgeColor;
        _edgeGraphics.Add(edge, edgeMesh);
        _edgeNode.AddChild(edgeMesh);
    }
    private void RemoveOutsideEdge(int edge)
    {
        if (_edgeGraphics.ContainsKey(edge))
        {
            _edgeGraphics[edge].Free();
            _edgeGraphics.Remove(edge);
        }
    }

}
