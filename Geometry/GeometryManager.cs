using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;



public class GeometryManager
{
    public Vector2 Dimensions { get; private set; }
    public Action<Triangle> TriangleAdded { get; set; }
    public Action<Triangle> TriangleUpdated { get; set; }
    public Action<Triangle> TriangleRemoved { get; set; }
        // = t => t.TriangleRemoved?.Invoke(t);
    public Action<Triangle, Triangle[]> TriangleDecomposed { get; set; }
    public ValueTypeBijection<int> HalfEdgePairs { get; private set; }
    public Dictionary<int, Vector2> PointsById { get; private set; }
    public Dictionary<int, List<int>> AdjacentPointsToPoint { get; private set; }
    public Dictionary<int, int> From { get; private set; }
    public Dictionary<int, int> To { get; private set; }
    public Dictionary<int, List<int>> FromEdgesForPoint { get; private set; }
    public Dictionary<int, List<int>> ToEdgesForPoint { get; private set; }
    public Graph<int, List<int>> EdgesBetweenPoints { get; private set; }
    public Dictionary<int, Triangle> TrianglesByEdgeIds { get; private set; }
    public List<Triangle> Triangles { get; private set; }
    public TriangleLookup TriLookup { get; private set; }
    public EdgeLookup EdgeLookup { get; private set; }
    private float _lookupCellSize = 1000f;
    private int _edgeIdCounter; 
    private int _triIdCounter;
    private int _pointIdCounter; 

    public GeometryManager(Delaunator delaunator, Vector2 dimensions)
    {
        Dimensions = dimensions;
        _edgeIdCounter = 0;
        _triIdCounter = 0;
        _pointIdCounter = 0;
        HalfEdgePairs = new ValueTypeBijection<int>();
        From = new Dictionary<int, int>();
        FromEdgesForPoint = new Dictionary<int, List<int>>();
        To = new Dictionary<int, int>();
        ToEdgesForPoint = new Dictionary<int, List<int>>();
        EdgesBetweenPoints = new Graph<int, List<int>>();
        TrianglesByEdgeIds = new Dictionary<int, Triangle>();
        PointsById = new Dictionary<int, Vector2>();
        AdjacentPointsToPoint = new Dictionary<int, List<int>>();
        Triangles = new List<Triangle>();
        EdgeLookup = new EdgeLookup(_lookupCellSize);
        EdgeLookup.LoadEdges(this);
        TriLookup = new TriangleLookup(_lookupCellSize, this);

        InitializeFromDelaunator(delaunator);
    }

    private void InitializeFromDelaunator(Delaunator d)
    {
        var edgeIdConversion = new Dictionary<int, int>();
        var pointIdConversion = new Dictionary<int, int>();
        for (int i = 0; i < d.Triangles.Length; i++)
        {
            var pointOldId = d.Triangles[i];
            var pPoint = d.Points[pointOldId];
            if (pointIdConversion.ContainsKey(pointOldId) == false)
            {
                var pVector2 = new Vector2((float)pPoint.X, (float)pPoint.Y);
                var pointNewId = AddPoint(pVector2);
                pointIdConversion.Add(pointOldId, pointNewId);
            }
        }

        for (int i = 0; i < d.Triangles.Length; i++)
        {
            var edgeIdOld = i;
            
            var fromPointOldId = d.Triangles[i];
            var fromPointNewId = pointIdConversion[fromPointOldId];
            
            var toPointOldId = d.Triangles[(i % 3 == 2) ? i - 2 : i + 1];
            var toPointNewId = pointIdConversion[toPointOldId];
            var edgeIdNew = AddEdge(fromPointNewId, toPointNewId);
            
            edgeIdConversion.Add(edgeIdOld, edgeIdNew);

        }

        for (int i = 0; i < d.Triangles.Length; i += 3)
        {
            AddTriangle(edgeIdConversion[i], 
                edgeIdConversion[i + 1], 
                edgeIdConversion[i + 2]);
        }
    }
    public int? SplitEdge(int edgeId, float distToSplitPoint, bool ignoreOpposite = false)
    {
        if (TrianglesByEdgeIds.ContainsKey(edgeId) == false) return null;
        var tri = TrianglesByEdgeIds[edgeId];
        var oldRoughness = tri.Info.Roughness.OutValue;
        var length = GetEdgeLength(edgeId);
        var oldFromPoint = PointsById[From[edgeId]];
        var oldToPoint = PointsById[To[edgeId]];
        var splitPointPos = oldFromPoint + (oldToPoint - oldFromPoint).Normalized() * distToSplitPoint;
        var splitPoint = AddPoint(splitPointPos);
        
        var fromEdgeId = GetPreviousEdgeInTri(edgeId);
        var toEdgeId = GetNextEdgeInTri(edgeId);
        
        var rayPoint = GetPointOppositeEdge(edgeId);
        var rayForward = AddEdge(rayPoint, splitPoint);
        var rayBackward = AddEdge(splitPoint, rayPoint);

        var beforeSplitEdge = AddEdge(From[edgeId], splitPoint);
        var new1 = AddTriangle(beforeSplitEdge, rayBackward, fromEdgeId);
        new1.Info.IncrementRoughness(oldRoughness);
        
        var afterSplitEdge = AddEdge(splitPoint, To[edgeId]);
        var new2 = AddTriangle(afterSplitEdge, rayForward, toEdgeId);
        new2.Info.IncrementRoughness(oldRoughness);

        TriangleDecomposed?.Invoke(tri, new Triangle[]{new1, new2});
        

        if (HalfEdgePairs[edgeId] is int oppEdgeId && ignoreOpposite == false)
        {
            var oppFromEdgeId = GetPreviousEdgeInTri(oppEdgeId);
            var oppToEdgeId = GetNextEdgeInTri(oppEdgeId);
            
            var oppRayPoint = GetPointOppositeEdge(oppEdgeId);
            var oppRayForwardId = AddEdge(oppRayPoint, splitPoint);
            var oppRayBackwardId = AddEdge(splitPoint, oppRayPoint);
            var oppTri = TrianglesByEdgeIds[oppEdgeId];
            var oppRoughness = oppTri.Info.Roughness.OutValue;

            var oppBeforeSplitEdge = AddEdge(From[oppEdgeId], splitPoint);
            var oppNew1 = AddTriangle(oppBeforeSplitEdge, oppFromEdgeId, oppRayBackwardId);
            oppNew1.Info.IncrementRoughness(oppRoughness);
            var oppAfterSplitEdge = AddEdge(splitPoint, To[oppEdgeId]);
            var oppNew2 = AddTriangle(oppAfterSplitEdge, oppToEdgeId, oppRayForwardId);
            oppNew2.Info.IncrementRoughness(oppRoughness);
            

            TriangleDecomposed?.Invoke(oppTri, new Triangle[]{oppNew1, oppNew2});
            RemoveEdge(oppEdgeId);
            RemoveTriangle(oppTri);
        }
        RemoveEdge(edgeId);
        RemoveTriangle(tri);
        return splitPoint;
    }
    public void InscribeTriangle(Triangle tri)
    {
        var splitPoints = new Dictionary<int, int>();
        var prevs = new int[3];
        var fromPoints = new int[3];
        var toPoints = new int[3];
        var lengths = new float[3];
        var oldRoughness = tri.Info.Roughness.OutValue;
        int getSplitPoint(int edge, float distToSplitPoint)
        {
            var outside = HalfEdgePairs[edge];
            if (outside is int o)
            {
                var length = GetEdgeLength(o);
                return (int)SplitEdge(o, length - distToSplitPoint, true);
            }
            else
            {
                var fromPos = PointsById[From[edge]];
                var toPos = PointsById[To[edge]];
                var travel = (toPos - fromPos).Normalized();
                var splitPointPos = fromPos + travel * distToSplitPoint;
                return AddPoint(splitPointPos);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            var edge = tri.HalfEdges[i];
            prevs[i] = GetPreviousEdgeInTri(edge);
            var length = GetEdgeLength(edge);
            lengths[i] = length;
            fromPoints[i] = From[edge];
            toPoints[i] = To[edge];
            splitPoints[edge] = getSplitPoint(edge, length / 2f);
        }

        var newTris = new Triangle[4];
        int addEdges(int edgeIndex, float distToSplitPoint)
        {
            var inside = tri.HalfEdges[edgeIndex];
            var from = fromPoints[edgeIndex];
            var to = toPoints[edgeIndex];

            int splitPoint = splitPoints[inside];
            var prevEdge = prevs[edgeIndex];

            var prevSplitPoint = splitPoints[prevEdge];
            var fromEdge = AddEdge(from, splitPoint);
            var toEdge = AddEdge(prevSplitPoint, from);
            var faceOut = AddEdge(splitPoint, prevSplitPoint);
            var faceIn = AddEdge(prevSplitPoint, splitPoint);
            var newTri = AddTriangle(fromEdge, toEdge, faceOut);
            newTri.Info.IncrementRoughness(oldRoughness);
            newTris[edgeIndex] = newTri;
            return faceIn;
        }
        var centerEdges = new int[3];
        for (int i = 0; i < 3; i++)
        {
            var length = lengths[i];
            centerEdges[i] = addEdges(i, length / 2f);
        }
        for (int i = 0; i < 3; i++)
        {
            RemoveEdge(tri.HalfEdges[i]);
        }
        newTris[3] = AddTriangle(centerEdges[0], centerEdges[1], centerEdges[2]);
        newTris[3].Info.IncrementRoughness(oldRoughness);
        TriangleDecomposed?.Invoke(tri, newTris);
        RemoveTriangle(tri);
    }

    public void MovePointAtPoint(Vector2 point)
    {
        var tri = TriLookup.GetTriAtPosition(point, this);
        if (tri != null)
        {
            var closePoint = tri.GetMinPointBy(pId => PointsById[pId].DistanceTo(point));
            MovePoint(closePoint, point);
        }
    }
    public void ExtrudeEdgeToPoint(Vector2 point)
    {
        var edge = EdgeLookup.GetClosestEdge(point, this);
        if (edge is int e && HalfEdgePairs[e] == null) ExtrudeNewTriangle(e, point);
    }
    public void SplitEdgeAtPoint(Vector2 point)
    {
        var edge = EdgeLookup.GetClosestEdge(point, this);
        if (edge is int e)
        {
            SplitEdge(e, GetEdgeLength(e) / 2f);
        }
    }
    public void TrisectTriAtPoint(Vector2 point)
    {
        var tri = TriLookup.GetTriAtPosition(point, this);
        if(tri != null) Trisect(tri, point);
    }
    public void InscribeNewTriangleAtPoint(Vector2 point)
    {
        var tri = TriLookup.GetTriAtPosition(point, this);
        if(tri != null) InscribeTriangle(tri);
    }
    private void AddInitialTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        var aId = AddPoint(a);
        var bId = AddPoint(b);
        var cId = AddPoint(c);

        var aB = AddEdge(aId, bId);
        var bC = AddEdge(bId, cId);
        var cA = AddEdge(cId, aId);

        AddTriangle(aB, bC, cA);
    }

    public void MovePoint(int pointId, Vector2 newPointPos)
    {
        if (PointsById.ContainsKey(pointId) == false) return;
        var edges = ToEdgesForPoint[pointId]
            .Union(FromEdgesForPoint[pointId])
            .ToList();
        var tris = edges.Select(e => TrianglesByEdgeIds[e]).Distinct().ToList();
        foreach (var tri in tris)
        {
            if (tri.PointInsideTriangle(newPointPos, this))
            {
                var pos = PointsById[pointId];
                PointsById[pointId] = newPointPos;
                edges.ForEach(e => EdgeLookup.UpdateEdge(e, this));
                tris.ForEach(t => TriLookup.UpdateTriangle(t, this));
                tris.ForEach(t => TriangleUpdated?.Invoke(t));
                return;
            }
        }
    }
    public Triangle ExtrudeNewTriangle(int edgeToExtrude, Vector2 newPoint)
    {
        if (HalfEdgePairs.Contains(edgeToExtrude)) return null;
        
        var newPointId = AddPoint(newPoint);
        var oldFrom = From[edgeToExtrude];
        var newTo = oldFrom;
        var oldTo = To[edgeToExtrude];
        var newFrom = oldTo;

        var facingEdge = AddEdge(newFrom, newTo);
        var goingEdge = AddEdge(newTo, newPointId);
        var returningEdge = AddEdge(newPointId, newFrom);

        return AddTriangle(facingEdge, goingEdge, returningEdge);
    }
    
    
    public void Trisect(Triangle triToSplit, Vector2 splitPoint)
    {
        var splitPointId = AddPoint(splitPoint);
        var oldRoughness = triToSplit.Info.Roughness.OutValue;
        var aB = triToSplit.HalfEdges[0];
        var bC = triToSplit.HalfEdges[1];
        var cA = triToSplit.HalfEdges[2];

        var a = From[aB];
        var b = From[bC];
        var c = From[cA];
        
        Triangle newTriOnEdge(int edge, int from, int to)
        {
            var goingEdge = AddEdge(to, splitPointId);
            var returningEdge = AddEdge(splitPointId, from);
            var newTri = AddTriangle(edge, goingEdge, returningEdge);
            newTri.Info.IncrementRoughness(oldRoughness);
            return newTri;
        }

        var newTris = new Triangle[3];
        newTris[0] = newTriOnEdge(aB, a, b);
        newTris[1] = newTriOnEdge(bC, b, c);
        newTris[2] = newTriOnEdge(cA, c, a);
        TriangleDecomposed?.Invoke(triToSplit, newTris);        
        RemoveTriangle(triToSplit);
    }

    public float GetDistBetweenPoints(int p1, int p2)
    {
        return PointsById[p1].DistanceTo(PointsById[p2]);
    }
    public float GetEdgeLength(int edgeId)
    {
        var fromTo = GetEdgeFromAndToPoints(edgeId);
        return fromTo.from.DistanceTo(fromTo.to);
    }

    public List<Triangle> GetTrianglesWithPoint(int point)
    {
        return FromEdgesForPoint[point]
            .Select(e => TrianglesByEdgeIds[e])
            .ToList();
    }
    public EdgeFromToPositions GetEdgeFromAndToPoints(int edgeId)
    {
        return new EdgeFromToPositions(PointsById[From[edgeId]], PointsById[To[edgeId]]);
    }

    public int GetPointOppositeEdge(int edgeId)
    {
        var tri = TrianglesByEdgeIds[edgeId];
        var index = tri.HalfEdges.IndexOf(edgeId);
        var nextEdge = tri.HalfEdges[(index + 1) % 3];
        return To[nextEdge];
    }

    public int GetPreviousEdgeInTri(int edgeId)
    {
        var tri = TrianglesByEdgeIds[edgeId];
        var index = tri.HalfEdges.IndexOf(edgeId);
        return tri.HalfEdges[(index + 2) % 3];  
    }

    public int GetNextEdgeInTri(int edgeId)
    {
        var tri = TrianglesByEdgeIds[edgeId];
        var index = tri.HalfEdges.IndexOf(edgeId);
        return tri.HalfEdges[(index + 1) % 3];
    }

    public Vector2 GetEdgeVector(int edgeId)
    {
        var fromTo = GetEdgeFromAndToPoints(edgeId);
        return fromTo.to - fromTo.from;
    }

    public int GetFirstEdgeBetweenPointsDirectionless(int p1, int p2)
    {
        return EdgesBetweenPoints.GetEdge(p1, p2)[0];
    }
    public int GetEdgeBetweenPoints(int fromPoint, int toPoint)
    {
        var froms = FromEdgesForPoint[fromPoint];
        for (var i = 0; i < froms.Count; i++)
        {
            var candEdge = froms[i];
            if (To[candEdge] == toPoint)
                return To[candEdge];
        }

        throw new Exception();
    }

    public bool PointsAreOnOutsideEdge(int p1, int p2)
    {
        var edges = EdgesBetweenPoints.GetEdge(p1, p2);
        if (edges.Count < 2) return true;
        return false;
    }
    public int AddPoint(Vector2 pos)
    {
        var point = TakePointId();
        PointsById.Add(point, pos);
        AdjacentPointsToPoint.Add(point, new List<int>());
        return point;
    }
    private void RemoveTriangle(Triangle tri)
    {
        TriLookup.RemoveTriangle(tri,  this);
        tri.HalfEdges.ForEach(e =>
        {
            if(TrianglesByEdgeIds[e] == tri) TrianglesByEdgeIds.Remove(e);
        });
        TriangleRemoved?.Invoke(tri);
        Triangles.Remove(tri);
    }
    private Triangle AddTriangle(params int[] halfEdges)
    {
        var tri = new Triangle(TakeTriId(), 
                                halfEdges[0], From[halfEdges[0]], 
                                halfEdges[1], From[halfEdges[1]],
                                halfEdges[2], From[halfEdges[2]]);
        Triangles.Add(tri);
        tri.HalfEdges.ForEach(
            e => { if (TrianglesByEdgeIds.ContainsKey(e)) TrianglesByEdgeIds[e] = tri;
                    else TrianglesByEdgeIds.Add(e, tri); } 
            );
        tri.SortEdges(this);
        TriLookup.AddTriangle(tri, this);
        TriangleAdded?.Invoke(tri);
        return tri;
    }
    private int? GetOppositeEdgeManual(int edgeId)
    {
        var from = From[edgeId];
        var to = To[edgeId];
        var toEdges = ToEdgesForPoint[from];
        var edges = toEdges.Where(e => From[e] == to);
        return edges.Count() > 0 ? (int?)edges.First() : null;
    }
    private int AddEdge(int fromId, int toId)
    {
        var edgeId = TakeEdgeId();
        
        From.Add(edgeId, fromId);
        if(FromEdgesForPoint.ContainsKey(fromId) == false)
            FromEdgesForPoint.Add(fromId, new List<int>());
        if(ToEdgesForPoint.ContainsKey(fromId) == false)
            ToEdgesForPoint.Add(fromId, new List<int>());
        FromEdgesForPoint[fromId].Add(edgeId);
        
        To.Add(edgeId, toId);
        if(ToEdgesForPoint.ContainsKey(toId) == false)
            ToEdgesForPoint.Add(toId, new List<int>());
        if(FromEdgesForPoint.ContainsKey(toId) == false)
            FromEdgesForPoint.Add(toId, new List<int>());
        ToEdgesForPoint[toId].Add(edgeId);

        if (EdgesBetweenPoints.HasEdge(fromId, toId) == false)
        {
            EdgesBetweenPoints.AddEdge(fromId, toId, new List<int>());
        }
        EdgesBetweenPoints.GetEdge(fromId, toId).Add(edgeId);
        AdjacentPointsToPoint[fromId].Add(toId);
        AdjacentPointsToPoint[toId].Add(fromId);

        if(GetOppositeEdgeManual(edgeId) is int oppositeEdgeId)
            HalfEdgePairs.TryAdd(edgeId, oppositeEdgeId);
        
        EdgeLookup.AddEdge(edgeId, this);
        return edgeId;
    }
    private void RemoveEdge(int edgeId)
    {
        EdgeLookup.RemoveEdge(edgeId);
        var from = From[edgeId];
        FromEdgesForPoint[from].Remove(edgeId);
        var to = To[edgeId];
        ToEdgesForPoint[to].Remove(edgeId);
        From.Remove(edgeId);
        To.Remove(edgeId);
        HalfEdgePairs.Remove(edgeId);
        EdgesBetweenPoints.GetEdge(from, to).Remove(edgeId);
        AdjacentPointsToPoint[from].Remove(to);
        AdjacentPointsToPoint[to].Remove(from);
    }
    
    private int TakeEdgeId()
    {
        _edgeIdCounter++;
        return _edgeIdCounter;
    }

    private int TakeTriId()
    {
        _triIdCounter++;
        return _triIdCounter;
    }

    private int TakePointId()
    {
        _pointIdCounter++;
        return _pointIdCounter;
    }
}

public static class GeometryManagerExt
{
    public static void DoForEveryTriBorderingPoint(this GeometryManager geometry,
        int point, Action<Triangle> action)
    {
        foreach (var edge in geometry.FromEdgesForPoint[point])
        {
            var tri = geometry.TrianglesByEdgeIds[edge];
            action.Invoke(tri);
        }
    }

    public static Vector2 GetEdgeKey(this GeometryManager geometry, int p1, int p2)
    {
        if (p1 < p2) return new Vector2(p1, p2);
        else if(p2 < p1) return new Vector2(p2, p1);
        throw new Exception("try to get edge key between same point");
    }

    public static int GetEdgeBetweenTris(this GeometryManager geometry, Triangle t1, Triangle t2)
    {
        for (var i = 0; i < t1.HalfEdges.Count; i++)
        {
            var edge = t1.HalfEdges[i];
            if (geometry.HalfEdgePairs[edge] is int oEdge)
            {
                if (geometry.TrianglesByEdgeIds[oEdge] == t2) return edge;
            }
        }

        throw new Exception("not adjacent tris");
    }

    public static Vector2 GetEdgeMidPoint(this GeometryManager geometry, int edge)
    {
        return (geometry.PointsById[geometry.From[edge]]
                + geometry.PointsById[geometry.To[edge]]) / 2f;
    }
    public static int? GetNextToEdgeAroundPoint(this GeometryManager geometry,
        int toEdge, int point, bool clockwise)
    {
        
        if (clockwise)
        {
            int nextEdgeInTri = geometry.GetNextEdgeInTri(toEdge);
            if (geometry.HalfEdgePairs[nextEdgeInTri] is int oEdge)
            {
                return oEdge;
            }
        }
        else if (geometry.HalfEdgePairs[toEdge] is int oEdge)
        {
            return geometry.GetPreviousEdgeInTri(oEdge);
        }
        return null;
    }

    public static void DoFan(this GeometryManager geometry, int startingToEdge, int point, bool clockwise,
        Action<int> foundEdgeAction)
    {
        var index = startingToEdge;
        while (geometry.GetNextToEdgeAroundPoint(index, point, clockwise)
               is int nextToEdge)
        {
            if (nextToEdge == startingToEdge) break;
            foundEdgeAction(nextToEdge);
            index = nextToEdge;
        }
    }

    public static bool PointOutOfBounds(this GeometryManager geometry, Vector2 p)
    {
        var dim = geometry.Dimensions;
        return p.x < 0f || p.x > dim.x || p.y < 0f || p.y > dim.y;
    }

    public static Tuple<List<Triangle>, List<Vector2>>
        GetLineSegmentTriIntersections(this GeometryManager geometry,
            Vector2 from, Vector2 to)

    {
        var tri = geometry.TriLookup.GetTriAtPosition(from, geometry);
        var pos = from;
        var axis = (to - from).Normalized();
        var incrementAmount = 1f;
        var travelled = 0f;
        var dist = from.DistanceTo(to);
        var tris = new List<Triangle>{tri};

        var points = new List<Vector2>{from};
        while (tri != null 
                && travelled < dist 
                && geometry.PointOutOfBounds(pos) == false)
        {
            bool found = false;
            for (int i = 0; i < 3; i++)
            {
                var p1 = geometry.PointsById[tri.Points[i]];
                var p2 = geometry.PointsById[tri.Points[(i + 1) % 3]];

                if (
                    
                    
                    GeometryExt.GetLineSegmentsIntersection(pos, to, p1, p2)
                    is Vector2 intersect)
                {
                    pos = intersect + axis * incrementAmount;
                    travelled = (from - pos).Length();
                    var edge = tri.HalfEdges[i];
                    points.Add(intersect);
                    if (geometry.HalfEdgePairs[edge] is int oEdge)
                    {
                        found = true;
                        tri = geometry.TrianglesByEdgeIds[oEdge];
                        tris.Add(tri);
                    }
                    else
                    {
                        tri = null;
                    }
                    break;
                }
            }

            if (found == false) break;
        }
        points.Add(to);

        return new Tuple<List<Triangle>, List<Vector2>>(tris, points);
    }


    public static List<List<int>> SortEdgesNew(this GeometryManager geometry, List<int> edgesSource)
    {
        var result = new List<List<int>>();
        var edges = new List<int>();
        edgesSource.ForEach(e => edges.Add(e));
        while (edges.Count > 0)
        {
            var edge = edges[0];
            edges.Remove(edge);
            var edgeFront = new List<int>{edge};
            result.Add(edgeFront);
            var fromBookmark = geometry.From[edge];
            var toBookmark = geometry.To[edge];
            while (true)
            {
                var toEdges = geometry.ToEdgesForPoint[fromBookmark];
                bool found = false;
                for (var i = 0; i < toEdges.Count; i++)
                {
                    var toEdge = toEdges[i];
                    if (edges.Contains(toEdge))
                    {
                        edges.Remove(toEdge);
                        edgeFront.Insert(0, toEdge);
                        fromBookmark = geometry.From[toEdge];
                        found = true;
                        break;
                    }
                }

                if (found == false) break;
            }
            
            
            while (true)
            {
                var fromEdges = geometry.FromEdgesForPoint[toBookmark];
                bool found = false;
                for (var i = 0; i < fromEdges.Count; i++)
                {
                    var fromEdge = fromEdges[i];
                    if (edges.Contains(fromEdge))
                    {
                        edges.Remove(fromEdge);
                        edgeFront.Add(fromEdge);
                        toBookmark = geometry.To[fromEdge];
                        found = true;
                        break;
                    }
                }

                if (found == false) break;
            }
        }
        
        return result;
    }

    public static List<List<int>> SortEdges(this GeometryManager geometry, List<int> edgesSource)
    {
        var edges = edgesSource.ToList();
        var result = new List<List<int>>();
        while (edges.Count > 0)
        {
            var e = edges[0];
            var list = new List<int> {e};
            result.Add(list);
            edges.RemoveAt(0);
            var right = geometry.From[e];
            var left = geometry.To[e];
            for (int i = edges.Count - 1; i >= 0; i--)
            {
                var candEdge = edges[i];
                if (geometry.To[candEdge] == right)
                {
                    right = geometry.From[candEdge];
                    list.Insert(0, candEdge);
                    edges.RemoveAt(i);
                    i = edges.Count - 1;
                }
                else if (geometry.From[candEdge] == left)
                {
                    right = geometry.To[candEdge];
                    list.Add(candEdge);
                    edges.RemoveAt(i);
                    i = edges.Count - 1;
                }
                else if (geometry.To[candEdge] == left)
                {
                    left = geometry.From[candEdge];
                    list.Insert(0, candEdge);
                    edges.RemoveAt(i);
                    i = edges.Count - 1;
                }
                else if (geometry.From[candEdge] == right)
                {
                    right = geometry.To[candEdge];
                    list.Add(candEdge);
                    edges.RemoveAt(i);
                    i = edges.Count - 1;
                }
            }
        }

        return result;
    }
   
}

public struct EdgeFromToPositions
{
    public Vector2 from;
    public Vector2 to;
    public EdgeFromToPositions(Vector2 from, Vector2 to)
    {
        this.from = from;
        this.to = to;
    }
}