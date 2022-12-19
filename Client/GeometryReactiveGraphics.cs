using Godot;
using System;
using System.Collections.Generic;

public class GeometryReactiveGraphics : Node2D
{
    private List<Triangle> _tris;
    private List<Vector2> _triPointTrios;
    private List<Vector2> _triOutlinePointTrios;
    private MeshInstance2D _triMeshInstance, _outlinesMeshInstance;
    private Color _fillColor = new Color(1f, 1f, 1f, .5f);
    private GeometryManager _geometry;
    public void Setup(GeometryManager geometry)
    {
        _geometry = geometry;
        _tris = new List<Triangle>();
        _triPointTrios = new List<Vector2>();
        _triOutlinePointTrios = new List<Vector2>();
        _triMeshInstance = new MeshInstance2D();
        _triMeshInstance.Modulate = _fillColor;
        AddChild(_triMeshInstance);
        _outlinesMeshInstance = new MeshInstance2D();
        _outlinesMeshInstance.Modulate = Colors.Black;
        AddChild(_outlinesMeshInstance);
    }

    public void Start()
    {
        _geometry.TriangleAdded += AddTriangle;
        _geometry.TriangleRemoved += RemoveTriangle;
        _geometry.TriangleUpdated += UpdateTriangle;
        _geometry.Triangles.ForEach(t => AddTriangle(t));
        BakeGraphics();
    }

    public void Stop()
    {
        _geometry.TriangleAdded -= AddTriangle;
        _geometry.TriangleRemoved -= RemoveTriangle;
        _geometry.TriangleUpdated -= UpdateTriangle;
        _geometry.Triangles.ForEach(t => RemoveTriangle(t));
    }

    public override void _ExitTree()
    {
        _geometry.TriangleAdded -= AddTriangle;
        _geometry.TriangleRemoved -= RemoveTriangle;
        _geometry.TriangleUpdated -= UpdateTriangle;
    }

    private void BakeGraphics()
    {
        _triMeshInstance.Mesh = MeshGenerator.GetArrayMesh(_triPointTrios.ToArray());
        _outlinesMeshInstance.Mesh = MeshGenerator.GetArrayMesh(_triOutlinePointTrios.ToArray());
    }
    private void UpdateTriangle(Triangle tri)
    {
        var index = _tris.IndexOf(tri);
        if (index < 0) return;
        var pointPositions = tri.GetPointPositions(_geometry);
        var triPointIndex = index * 3;
        _triPointTrios[triPointIndex] = pointPositions.a;
        _triPointTrios[triPointIndex + 1] = pointPositions.b;
        _triPointTrios[triPointIndex + 2] = pointPositions.c;
        var outlineMeshPoints = MeshGenerator.GetTriOutlineMeshPoints(_geometry, tri, .05f);
        var outlinePointCount = outlineMeshPoints.Count;
        var outlineIndex = index * outlinePointCount;
        for (int i = 0; i < outlineMeshPoints.Count; i++)
        {
            _triOutlinePointTrios[outlineIndex + i] = outlineMeshPoints[i];
        }
        BakeGraphics();
    }
    private void AddTriangle(Triangle tri)
    {
        var pointPositions = tri.GetPointPositions(_geometry);
        _triPointTrios.Add(pointPositions.a);
        _triPointTrios.Add(pointPositions.b);
        _triPointTrios.Add(pointPositions.c);
        _tris.Add(tri);
        _triOutlinePointTrios.AddRange(MeshGenerator.GetTriOutlineMeshPoints(_geometry, tri, .05f));
        BakeGraphics();
    }
    private void RemoveTriangle(Triangle tri)
    {
        var index = _tris.IndexOf(tri);
        if (index < 0) return;
        _triPointTrios.RemoveAt(index * 3 + 2);
        _triPointTrios.RemoveAt(index * 3 + 1);
        _triPointTrios.RemoveAt(index * 3);
        _tris.Remove(tri);
        _triOutlinePointTrios.RemoveRange(index * 18, 18);
    }
}
