using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GeologyUtility
{
    public static void DisturbEdges(GeometryManager geometry, PolygonManager polygons,
        Polygon poly1, Polygon poly2, float minLength)
    {
        var edges = poly1.GetEdgesBorderingPolygon(poly2, geometry);
        foreach (var edge in edges)
        {
            DisturbEdge(edge, geometry, polygons, minLength);
        }
    }

    public static void DisturbEdge(int edge, GeometryManager geometry, 
        PolygonManager polygons,  float minLength)
    {
        var pointToDisturb = geometry.From[edge];
        var pointToDisturbPos = geometry.PointsById[pointToDisturb];
        var adjacentTris = geometry.GetTrianglesWithPoint(pointToDisturb);

        var randTriPoints = new List<Vector2>();
        var triAreaWeights = new List<float>();

        foreach (var adjacentTri in adjacentTris)
        {
            var rays = adjacentTri.GetRaysFromPoint(geometry, pointToDisturb);
            var smallLength = Mathf.Min(rays.backRay.Length(), rays.forwardRay.Length());
            if (smallLength <= minLength)
            {
                return;
            }
            
            var minMargin = minLength / smallLength;
            var minMarginInverse = 1f - minMargin;
            var marginEffective = Mathf.Min(minMarginInverse, .25f);
            var point = adjacentTri.GetRandomPointInCone(geometry, pointToDisturb, marginEffective);
            var area = adjacentTri.GetArea(geometry);
            randTriPoints.Add(point);
            triAreaWeights.Add(area * area);
        }

        if (randTriPoints.Count == 0) return;
        
        var randPoint = RandomExt.GetRandomElementByWeight<Vector2>
                                            (randTriPoints,triAreaWeights);

        foreach (var adjacentTri in adjacentTris)
        {
            var oppEdge = adjacentTri.GetEdgeOppositePoint(geometry, pointToDisturb);
            var fromTo = geometry.GetEdgeFromAndToPoints(oppEdge);
            if (pointToDisturbPos.PointIsLeftOfLine(fromTo.from, fromTo.to)
                != randPoint.PointIsLeftOfLine(fromTo.from, fromTo.to))
            {
                return;
            }
            if (TriangleUtility.GetMinAltitude(randPoint, fromTo.from, fromTo.to) < minLength
                || TriangleUtility.GetMinEdgeLength(randPoint, fromTo.from, fromTo.to) < minLength)
            {
                return;
            }
        }
        geometry.MovePoint(pointToDisturb, randPoint);
    }
    public static void SplitBorderEdges(GeometryManager geometry, PolygonManager polygons, 
        Polygon poly, Polygon oppPoly, float minLength)
    {
        var borderTris = poly.GetTrisBorderingPolygon(oppPoly, geometry, polygons);
        int iter = 0;
        while (borderTris.Count > 0)
        {
            var borderTri = borderTris[0];
            borderTris.Remove(borderTri);
            var triBorderEdges = borderTri.HalfEdges
                .Where(e =>
                {
                    return geometry.HalfEdgePairs[e] is int o
                           && oppPoly.Triangles.Contains(geometry.TrianglesByEdgeIds[o]);
                })
                .Where(e => geometry.GetEdgeLength(e) > minLength * 2f);
            var borderCount = triBorderEdges.Count();
            if (borderCount == 0) continue; 
            else if (borderCount == 1)
            {
                var e = triBorderEdges.First();
                SplitEdge(e, geometry, polygons, minLength);
            }
            else
            {
                var oldAdjTris = borderTri.GetTrisSharingEdge(geometry);
                InscribeTriangle(borderTri, geometry, polygons, minLength);
                var newAdjTris = borderTri.GetTrisSharingEdge(geometry);
                var remove = oldAdjTris.Except(newAdjTris);
                var add = newAdjTris.Except(oldAdjTris);
                foreach (var r in remove)
                {
                    borderTris.Remove(r);
                }
                borderTris.AddRange(add);
            }
        }
    }
    public static void SplitEdge(int edge, GeometryManager geometry, 
        PolygonManager polygons, float minLength)
    {
        var length = geometry.GetEdgeLength(edge);
        if (length >= minLength * 2f)
        {
            var oppPoint = geometry.PointsById[geometry.GetPointOppositeEdge(edge)];
            var fromTo = geometry.GetEdgeFromAndToPoints(edge);
            var splitPoint = (fromTo.from + fromTo.to) / 2f;

            if (TriangleUtility.BadTri(minLength, splitPoint, oppPoint, fromTo.from)) return;
            if (TriangleUtility.BadTri(minLength, splitPoint, oppPoint, fromTo.to)) return;
            geometry.SplitEdge(edge, length / 2f);
        }
    }

    public static void FragmentPolygon(Polygon poly, WorldManager world, float minLength)
    {
        var tris = poly.Triangles.ToList();
        foreach (var tri in tris)
        {
            TrisectTriangle(tri, world.Geometry, world.GeologyPolygons, minLength);
        }
    }

    public static void TrisectTriangle(Triangle tri, GeometryManager geometry,
        PolygonManager polygons, float minLength)
    {
        var splitPoint = tri.GetCentroid(geometry);
        for (int i = 0; i < 3; i++)
        {
            var edge = tri.HalfEdges[i];
            var fromTo = geometry.GetEdgeFromAndToPoints(edge);
            if (TriangleUtility.BadTri(minLength, splitPoint, fromTo.from, fromTo.to))
                return;
        }
        geometry.Trisect(tri, splitPoint);
    }
    public static void InscribeTriangle(Triangle tri, GeometryManager geometry, 
        PolygonManager polygons, float minLength)
    {
        var splitPoints = tri.GetSplitPoints(geometry);
        if (TriangleUtility.BadTri(minLength, splitPoints.a, splitPoints.b, splitPoints.c))
        {
            return;
        }

        bool checkBadTris(int edge, Vector2 splitPoint, Vector2 prevSplitPoint)
        {
            var oppPoint = geometry.GetPointOppositeEdge(edge);
            var oppPointPos = geometry.PointsById[oppPoint];
            var fromTo = geometry.GetEdgeFromAndToPoints(edge);
            
            if (TriangleUtility.BadTri(minLength, fromTo.from, prevSplitPoint, splitPoint)) return true;
            if (TriangleUtility.BadTri(minLength, fromTo.from, oppPointPos, splitPoint)) return true;
            if (TriangleUtility.BadTri(minLength, fromTo.to, oppPointPos, splitPoint)) return true;
            return false;
        }

        if (checkBadTris(tri.HalfEdges[0], splitPoints.a, splitPoints.c)) return;
        if (checkBadTris(tri.HalfEdges[1], splitPoints.b, splitPoints.a)) return;
        if (checkBadTris(tri.HalfEdges[2], splitPoints.c, splitPoints.a)) return;
        
        geometry.InscribeTriangle(tri);
    }

    public static async void BuildPlates(Vector2 dimensions, int numContinents, 
        int platesPerContinent, float percentLand,
        GeometryManager geometry, PolygonManager polygons)
    {
        var numPlates = numContinents * platesPerContinent;

        var platePoints = new List<Vector2>();
        for (int i = 0; i < numPlates; i++)
        {
            var point = new Vector2(
                Game.Random.RandfRange(0f, dimensions.x),
                Game.Random.RandfRange(0f, dimensions.y)
            );
            platePoints.Add(point);
        }
        var triPlates = new ConcurrentDictionary<Triangle, int>();

        await Task.WhenAll(geometry.Triangles.Select(
            t => Task.Run(
                () => registerTri(t))));
        
        void registerTri(Triangle tri)
        {
            var closePoint = platePoints.OrderBy(p => p.DistanceTo(tri.GetCentroid(geometry))).First();
            var closePlatePointIndex = platePoints.IndexOf(closePoint);
                // plateQuadTree.GetClosestElement(tri.GetCentroid(geometry));
            triPlates.TryAdd(tri, closePlatePointIndex);
        }
        var plateTris = platePoints.ToDictionary(p => platePoints.IndexOf(p), p => new List<Triangle>());
        foreach (var entry in triPlates)
        {
            plateTris[entry.Value].Add(entry.Key);
        }
        
        foreach (var entry in plateTris)
        {
            if (entry.Value == null) return;
            if (entry.Value.Count == 0) return;
            var poly = polygons.AddNewPolygonWithTris(entry.Value);
        }
        var polyAdjancencies = new Dictionary<Polygon, List<Polygon>>();
        foreach (var poly in polygons.Polygons)
        {
            polyAdjancencies.Add(poly, poly.GetAdjacentPolygons(geometry));
        }

        var polysToPick = polygons.Polygons.ToList();

        var continents = new List<List<Polygon>>();
        var polyContinents = new Dictionary<Polygon, int>();
        var continentIsLand = Enumerable.Range(0, numContinents)
            .Select(i =>
                Game.Random.RandfRange(0f, 1f) < percentLand
                    ? true
                    : false
            )
            .ToList();
        
        for (int i = 0; i < numContinents; i++)
        {
            var rand = polysToPick.GetRandomElement();
            polysToPick.Remove(rand);
            polyContinents.Add(rand, i);
            continents.Add(new List<Polygon>{rand});
        }
        var openContinents = continents.ToList();

        while (polysToPick.Count > 0)
        {
            var cont = openContinents[0];
            var borderPolys = cont
                .SelectMany(p => p.GetAdjacentPolygons(geometry))
                .Where(nP => polysToPick.Contains(nP));
            var borderPolyCount = borderPolys.Count();
            if (borderPolyCount == 0)
            {
                openContinents.RemoveAt(0);
            }
            else
            {
                var rand = Game.Random.RandiRange(0, borderPolyCount - 1);
                var pick = borderPolys.ElementAt(rand);
                if (polyContinents.ContainsKey(pick)) continue;
                cont.Add(pick);
                polyContinents.Add(pick, continents.IndexOf(cont));
                polysToPick.Remove(pick);
                openContinents.RemoveAt(0);
                openContinents.Add(cont);
            }
        }
        
        foreach (var entry in polyContinents)
        {
            var isLand = continentIsLand[entry.Value];
            polygons.ChangePolygonIsLand(entry.Key, isLand);
        }

        var contTris = new List<List<Triangle>>();
        void setContTris(List<Polygon> cont)
        {
            var index = continents.IndexOf(cont);
            var allTris = new List<Triangle>();
            for (int i = 0; i < cont.Count; i++)
            {
                var poly = cont[i];
                allTris.AddRange(poly.Triangles);
                polygons.RemovePolygon(poly);
            }

            polygons.AddNewPolygonWithTris(allTris);
        }

        foreach (var cont in continents)
        {
            setContTris(cont);
        }
    }
    public static Graph<Polygon, float> GetPolygonGraph(GeometryManager geometry, PolygonManager polygons)
    {
        var graph = new Graph<Polygon, float>();
        foreach (var poly in polygons.Polygons)
        {
            var node = new GraphNode<Polygon, float>(poly);
            graph.AddNode(poly);
        }

        foreach (var poly in polygons.Polygons)
        {
            var nPolys = poly.GetAdjacentPolygons(geometry);
            foreach (var nPoly in nPolys)
            {
                graph.AddUndirectedEdge(graph[poly], graph[nPoly], 1f);
            }
        }

        return graph;
    }
}
