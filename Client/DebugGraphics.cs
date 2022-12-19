using Godot;
using System;
using System.Collections.Generic;

public class DebugGraphics : Node
{
    private WorldManager _world;
    private Vector2 _cachedMousePos;
    public Line2D Line { get; private set; }
    private Node2D _trisCustomMesh,
        _trisCustomMeshHolder,
        
        _closestEdgeGraphic, 
        _closestEdgeGraphicHolder, 
        
        _triOutline,
        _triOutlineHolder,
        
        _polyGraphics,
        _polyGraphicsHolder,
        
        _lineGraphics,
        _lineGraphicsHolder;
    
    public void Setup(WorldManager world)
    {
        _world = world;
        _trisCustomMeshHolder = new Node2D();
        AddChild(_trisCustomMeshHolder);
        
        _closestEdgeGraphicHolder = new Node2D();
        AddChild(_closestEdgeGraphicHolder);
        
        _triOutlineHolder = new Node2D();
        AddChild(_triOutlineHolder);
        
        _polyGraphicsHolder = new Node2D();
        AddChild(_polyGraphicsHolder);
        
        _lineGraphicsHolder = new Node2D();
        AddChild(_lineGraphicsHolder);
    }

    public void Clear()
    {
        _trisCustomMesh?.Free();
        _trisCustomMesh = null;
        _closestEdgeGraphic?.Free();
        _closestEdgeGraphic = null;
        _triOutline?.Free();
        _triOutline = null;
        _polyGraphics?.Free();
        _polyGraphics = null;
        _lineGraphics?.Free();
        _lineGraphics = null;
    }
    public override void _Ready()
    {
        Line = new Line2D();
        Line.Width = 5f;
        Line.DefaultColor = Colors.White;
        AddChild(Line);
    }

    public override void _Process(float delta)
    {
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        if (mousePos != _cachedMousePos)
        {
            _cachedMousePos = mousePos;
            DrawClosestGraphics();
        }
    }

    public void DrawZebraLine(List<Vector2> points)
    {
        _lineGraphics?.Free();
        _lineGraphics = new Line2D();
        for (int i = 0; i < points.Count - 1; i++)
        {
            var line = new Line2D();
            line.Width = 5f;
            line.DefaultColor = i % 2 == 0
                ? Colors.Black
                : Colors.White;
            line.AddPoint(points[i]);
            line.AddPoint(points[i + 1]);
            _lineGraphics.AddChild(line);
        }
        _lineGraphicsHolder.AddChild(_lineGraphics);
        RemoveChild(_lineGraphicsHolder);
        AddChild(_lineGraphicsHolder);
    }
    public void DrawTriGroups(List<List<Triangle>> triGroups, List<Color> colors)
    {
        _trisCustomMesh?.Free();
        for (int i = 0; i < triGroups.Count; i++)
        {
            _trisCustomMesh = MeshGenerator.GetTrisMesh(_world.Geometry,
                triGroups[i]);
            _trisCustomMesh.Modulate = colors[i];
            _trisCustomMeshHolder.AddChild(_trisCustomMesh);
        }
        RemoveChild(_trisCustomMeshHolder);
        AddChild(_trisCustomMeshHolder);
    }
    public void DrawTris(List<Triangle> tris, List<Color> colors)
    {
        _trisCustomMesh?.Free();
        _trisCustomMesh = new Node2D();
        for (int i = 0; i < tris.Count; i++)
        {
            var mesh = MeshGenerator.GetTriMesh(_world.Geometry,
                tris[i]);
            mesh.Modulate = colors[i];
            _trisCustomMesh.AddChild(mesh);
        }
        _trisCustomMeshHolder.AddChild(_trisCustomMesh);
        RemoveChild(_trisCustomMeshHolder);
        AddChild(_trisCustomMeshHolder);
    }
    public void DrawLines(GeometryManager geometry, 
        List<List<Vector2>> paths, Color color)
    {
        foreach (var path in paths)
        {
            if (path.Count == 0) continue;
            var line = new Line2D();
            line.Width = 15;
            line.DefaultColor = color;
            for (var i = 0; i < path.Count; i++)
            {
                line.AddPoint(path[i]);
            }
            AddChild(line);
        }
    }
    private void DrawClosestGraphics()
    {
        ClearClosestGraphics();
        var tri = _world.Geometry.TriLookup.GetTriAtPosition(_cachedMousePos, _world.Geometry);
        if (tri != null)
        {
            // _triOutline = DrawTriOutline(tri);
            // _triOutlineHolder.AddChild(_triOutline);
            Game.I.Ui.TriUnderMouse.Draw(tri, _world);
        }
        else
        {
            Game.I.Ui.TriUnderMouse.Clear();
        }
        RemoveChild(_triOutlineHolder);
        AddChild(_triOutlineHolder);
    }

    private void ClearClosestGraphics()
    {
        _triOutline?.Free();
        _triOutline = null;
        _closestEdgeGraphic?.Free();
        _closestEdgeGraphic = null;
    }

    public void ClearPolygonLabels()
    {
        _polyGraphics?.Free();
        _polyGraphics = null;
    }
    public void DrawPolygonLabels()
    {
        ClearPolygonLabels();
        _polyGraphics = new Node2D();
        foreach (var poly in _world.GeologyPolygons.Polygons)
        {
            foreach (var tri in poly.Triangles)
            {
                _polyGraphics.AddChild(DrawTriOutline(tri));
            }
        }
        _polyGraphicsHolder.AddChild(_polyGraphics);
    }
    private Node2D DrawTriOutline(Triangle tri)
    {
        var triOutline = new Node2D();
        var labelScale = Vector2.One * tri.GetLongestEdgeLength(_world.Geometry) / 100f;
        var outline = MeshGenerator.GetTriOutlineMesh(_world.Geometry, tri, .05f);
        outline.Modulate = Colors.Red;
        triOutline.AddChild(outline);
        var splitPoints = tri.GetSplitPoints(_world.Geometry);
        var centroid = tri.GetCentroid(_world.Geometry);
        var triLabel = new Label();
        var triLabelNode = new Node2D();
        triLabelNode.AddChild(triLabel);
        triLabelNode.Position = centroid;
        triLabel.RectScale = labelScale;
        triLabel.Modulate = Colors.Yellow;
        triLabel.Text = tri.Id.ToString();
        triOutline.AddChild(triLabelNode);

        void drawEdge(int edge, Vector2 splitPoint)
        {
            var edgeNode = new Node2D();
            triOutline.AddChild(edgeNode);
            var edgeRay = _world.Geometry.GetEdgeVector(edge).Rotated(Mathf.Pi / 2f);
            var rot = Vector2.Up.AngleTo(edgeRay);
            edgeNode.Rotation = rot;
            edgeNode.Position = splitPoint + edgeRay.Normalized() * labelScale * .1f;
            
            var edgeLabel = new Label();
            edgeLabel.Modulate = Colors.DarkRed;
            edgeLabel.Text = edge.ToString();
            edgeLabel.RectScale = labelScale;
            edgeNode.AddChild(edgeLabel);
            edgeLabel.Align = Label.AlignEnum.Center;

            
            var from = _world.Geometry.From[edge];
            var fromPointNode = new Node2D();
            var fromPointLabel = new Label();
            fromPointLabel.Modulate = Colors.Green;
            fromPointLabel.Text = from.ToString();
            fromPointNode.AddChild(fromPointLabel);

            fromPointNode.Position = _world.Geometry.PointsById[from];
            triOutline.AddChild(fromPointNode);
        }
        drawEdge(tri.HalfEdges[0], splitPoints.a);
        drawEdge(tri.HalfEdges[1], splitPoints.b);
        drawEdge(tri.HalfEdges[2], splitPoints.c);
        
        return triOutline;
    }
}
