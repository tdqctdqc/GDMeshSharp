using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MeshGenerator 
{
    public static MeshInstance2D GetTriOutlineMesh(GeometryManager geometry, Triangle tri, float borderProportion)
    {
        var meshInstance = new MeshInstance2D();
        var longest = tri.GetLongestEdgeLength(geometry);
        var borderThickness = longest * borderProportion;
        var points = tri.GetPointPositions(geometry);

        var innerPoints = tri.GetInnerTriPoints(geometry, borderThickness);

        var triPoints = new Vector2[]
        {
            points.a, innerPoints[0], innerPoints[1],
            points.a, innerPoints[1], points.b,
            points.b, innerPoints[1], innerPoints[2],
            points.b, innerPoints[2], points.c,
            points.c, innerPoints[2], innerPoints[0],
            points.c, innerPoints[0], points.a
        };
        
        var mesh = GetArrayMesh(triPoints);
        meshInstance.Mesh = mesh;

        return meshInstance;
    }

    public static List<Vector2> GetTriOutlineMeshPoints(GeometryManager geometry, Triangle tri, float borderProportion)
    {
        var longest = tri.GetLongestEdgeLength(geometry);
        var borderThickness = longest * borderProportion;
        var points = tri.GetPointPositions(geometry);

        var innerPoints = tri.GetInnerTriPoints(geometry, borderThickness);

        return new List<Vector2>
        {
            points.a, innerPoints[0], innerPoints[1],
            points.a, innerPoints[1], points.b,
            points.b, innerPoints[1], innerPoints[2],
            points.b, innerPoints[2], points.c,
            points.c, innerPoints[2], innerPoints[0],
            points.c, innerPoints[0], points.a
        };
    }
    public static MeshInstance2D GetTriOutlinesMesh(GeometryManager geometry, List<Triangle> tris, float borderProportion)
    {
        var meshInstance = new MeshInstance2D();
        var triPoints = new List<Vector2>();
        foreach (var tri in tris)
        {
            triPoints.AddRange(GetTriOutlineMeshPoints(geometry, tri, borderProportion));
        }
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;

        return meshInstance;
    }
    public static MeshInstance2D GetTriMesh(GeometryManager geometry, Triangle tri)
    {
        var meshInstance = new MeshInstance2D();
        var points = tri.GetPointPositions(geometry);
        var aOuter = points.a;
        var bOuter = points.b;
        var cOuter = points.c;

        var triPoints = new Vector2[]
        {
            aOuter, bOuter, cOuter,
        };
        
        var mesh = GetArrayMesh(triPoints);
        meshInstance.Mesh = mesh;

        return meshInstance;
    }
    public static MeshInstance2D GetTrisMesh(GeometryManager geometry, 
        List<Triangle> tris,
        List<Color> colors = null)
    {
        var meshInstance = new MeshInstance2D();

        var meshPoints = new List<Vector2>();
        foreach (var tri in tris)
        {
            var points = tri.GetPointPositions(geometry);
            meshPoints.Add(points.a);
            meshPoints.Add(points.b);
            meshPoints.Add(points.c);
        }
        if(colors != null)
        {
            var mesh = GetArrayMesh(meshPoints.ToArray(), colors.ToArray());
            meshInstance.Mesh = mesh;
        }
        else
        {
            var mesh = GetArrayMesh(meshPoints.ToArray());
            meshInstance.Mesh = mesh;
        }

        return meshInstance;
    }

    public static MeshInstance2D GetRainbowEdgesMesh(List<int> edges,
        float thickness,
        GeometryManager geometry,
        Color backColor)
    {
        var colors = Enumerable.Range(0, edges.Count)
            .Select(i => ColorsExt.GetRainbowColor(i))
            .ToList();
        
        var triPoints = new List<Vector2>();
        Color[] triColors = colors == null 
            ? null 
            : new Color[edges.Count * 4];

        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            
            var from = geometry.PointsById[geometry.From[edge]];
            var to = geometry.PointsById[geometry.To[edge]];
            var perpendicular = (to - from).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from;
            var fromIn = from - perpendicular * thickness;
            var toOut = to;
            var toIn = to - perpendicular * thickness;
            
            var rFromOut = from - perpendicular * thickness * .25f;
            var rFromIn = from - perpendicular * thickness * .75f;
            var rToOut = to - perpendicular * thickness * .25f;
            var rToIn = to - perpendicular * thickness * .75f;
            
            
            if (colors != null)
            {
                var color = colors[i];
                triColors[4 * i] = backColor;
                triColors[4 * i + 1] = backColor;
                triColors[4 * i + 2] = color;
                triColors[4 * i + 3] = color;
            }
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
            
            triPoints.Add(rFromIn);
            triPoints.Add(rFromOut);
            triPoints.Add(rToOut);
            triPoints.Add(rToIn);
            triPoints.Add(rToOut);
            triPoints.Add(rFromIn);
        }

        
        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray(), triColors);
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetEdgesMesh(List<int> edges, 
        float thickness, 
        GeometryManager geometry,
        List<Color> colors = null,
        bool numbers = false)
    {
        var triPoints = new List<Vector2>();
        Color[] triColors = colors == null 
            ? null 
            : new Color[edges.Count * 2];

        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            
            var from = geometry.PointsById[geometry.From[edge]];
            var to = geometry.PointsById[geometry.To[edge]];
            var perpendicular = (to - from).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from;
            var fromIn = from - perpendicular * thickness;
            var toOut = to;
            var toIn = to - perpendicular * thickness;
            if (colors != null)
            {
                var color = colors[i];
                triColors[2 * i] = color;
                triColors[2 * i + 1] = color;
            }
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
            
        }

        
        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray(), triColors);
        meshInstance.Mesh = mesh;


        if (numbers)
        {
            for (var i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                var label = new Label();
                label.Text = i.ToString();
                label.SelfModulate = Colors.Black;
                var node = new Node2D();
                node.AddChild(label);
                node.Position = geometry.GetEdgeMidPoint(edge);
                meshInstance.AddChild(node);
            }
        }
        return meshInstance;
    }
    public static MeshInstance2D GetLinesMesh(List<Vector2> froms,
        List<Vector2> tos, float thickness, GeometryManager geometry)
    {
        var triPoints = new List<Vector2>();
        for (int i = 0; i < froms.Count; i++)
        {
            var from = froms[i];
            var to = tos[i];
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * .5f * thickness;
            var fromIn = from - perpendicular * .5f * thickness;
            var toOut = to + perpendicular * .5f * thickness;
            var toIn = to - perpendicular * .5f *thickness;
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
        }

        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetLinesMeshCustomWidths(List<Vector2> froms,
        List<Vector2> tos, List<float> widths, GeometryManager geometry)
    {
        var triPoints = new List<Vector2>();
        for (int i = 0; i < froms.Count; i++)
        {
            var from = froms[i];
            var to = tos[i];
            var width = widths[i];
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * width / 2f;
            var fromIn = from - perpendicular * width / 2f;
            var toOut = to + perpendicular * width / 2f;
            var toIn = to - perpendicular * width / 2f;
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
        }

        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetLineMesh(Vector2 from, Vector2 to, float thickness)
    {
        var meshInstance = new MeshInstance2D();
        var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
        var fromOut = from + perpendicular * thickness / 2f;
        var fromIn = from - perpendicular * thickness / 2f;
        var toOut = to + perpendicular * thickness / 2f;
        var toIn = to - perpendicular * thickness / 2f;
        
        var triPoints = new Vector2[]
        {
            fromIn, fromOut, toOut,
            toIn, toOut, fromIn
        };
        
        var mesh = GetArrayMesh(triPoints);
        meshInstance.Mesh = mesh;
        return meshInstance;
    }

    public static MeshInstance2D GetCircleMesh(Vector2 center, float radius, int resolution)
    {
        var angleIncrement = Mathf.Pi * 2f / (float) resolution;
        var triPoints = new List<Vector2>();
        for (int i = 0; i < resolution; i++)
        {
            var startAngle = angleIncrement * i;
            var startPoint = center + Vector2.Up.Rotated(startAngle) * radius;
            var endAngle = startAngle + angleIncrement;
            var endPoint = center + Vector2.Up.Rotated(endAngle) * radius;
            triPoints.Add(center);
            triPoints.Add(startPoint);
            triPoints.Add(endPoint);
        }

        var mesh = GetArrayMesh(triPoints.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static Node2D GetArrowGraphic(Vector2 from, Vector2 to, float thickness)
    {
        var arrow = new Node2D();
        var length = from.DistanceTo(to);
        var lineTo = from + (to - from).Normalized() * length * .8f;
        var perpendicular = (to - from).Normalized().Rotated(Mathf.Pi / 2f);
        var line = GetLineMesh(from, lineTo, thickness);
        arrow.AddChild(line);
        var triPoints = new Vector2[]
        {
            to,
            lineTo + perpendicular * thickness,
            lineTo - perpendicular * thickness,
        };
        var triMesh = GetArrayMesh(triPoints);
        var tri = new MeshInstance2D();
        tri.Mesh = triMesh;
        arrow.AddChild(tri);
        return arrow;
    }

    public static Node2D GetSubGraphMesh<TNode, TEdge>(SubGraph<TNode, TEdge> subGraph,
        float thickness,
        Func<TNode, Vector2> getVertexPos,
        Color color,
        Color foreignEdgeColor)
    {
        var node = new Node2D();
        for (var i = 0; i < subGraph.Elements.Count; i++)
        {
            var e = subGraph.Elements[i];
            var vertexPos = getVertexPos(e);
            var vertex = GetCircleMesh(vertexPos, thickness * 2f, 12);
            vertex.SelfModulate = color;
            node.AddChild(vertex);
            foreach (var n in subGraph.Graph[e].Neighbors)
            {
                var nPos = getVertexPos(n);
                var edge = GetLineMesh(vertexPos, nPos, thickness);
                edge.SelfModulate = foreignEdgeColor;
                node.AddChild(edge);
                if (subGraph.Graph.NodeSubGraphs.ContainsKey(n)
                    && subGraph.Graph.NodeSubGraphs[n] == subGraph)
                {
                    edge.SelfModulate = color;
                }
            }
        }
        return node;
    }
    public static ArrayMesh GetArrayMesh(Vector2[] triPoints, Color[] triColors = null)
    {
        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = triPoints;
        arrays[(int)ArrayMesh.ArrayType.Color] = ConvertTriToVertexColors(triColors); 
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        return arrayMesh; 
    }

    private static Color[] ConvertTriToVertexColors(Color[] triColors)
    {
        if (triColors == null) return null;
        var vertexColors = new Color[triColors.Length * 3];
        for (int i = 0; i < triColors.Length; i++)
        {
            vertexColors[3 * i] = triColors[i];
            vertexColors[3 * i + 1] = triColors[i];
            vertexColors[3 * i + 2] = triColors[i];
        }

        return vertexColors;
    }
}