using Godot;
using System;
using System.Collections.Generic;

public static class PointsGenerator 
{
    public static List<Vector2> GenerateSemiRegularPoints(Vector2 dim, 
                                                    float cellSize, 
                                                    int triesBeforeExit,
                                                    bool square,
                                                    int maxPoints = int.MaxValue)
    {
        var points = new List<Vector2>();
        var grid = new RegularGrid<Vector2>(v => v, cellSize);
        var rand = new RandomNumberGenerator();
        int badTries = 0;

        while(badTries < triesBeforeExit && points.Count < maxPoints)
        {
            float x = rand.RandfRange(0f, dim.x);
            float y = rand.RandfRange(0f, dim.y);
            var vec = new Vector2(x,y);
            if(grid.GetElementsAtPoint(vec).Count == 0)
            {
                badTries = 0;
                grid.AddElement(vec, vec);
                points.Add(vec);
            }
            else
            {
                badTries++;
            }
        }
        if(square)
        {
            AddMeshBorder(points, dim, cellSize, cellSize / 2f);
        }
        return points;
    }

    public static List<Vector2> GenerateConstrainedSemiRegularPoints(Vector2 dim,
                                                                float cellSize,
                                                                float constraintSize,
                                                                bool square)
    {
        var rand = new RandomNumberGenerator();
        var points = new List<Vector2>();
        int xCells = Mathf.CeilToInt(dim.x / cellSize);
        int yCells = Mathf.CeilToInt(dim.y / cellSize);
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < yCells; j++)
            {
                var cellCenter = Vector2.One * cellSize / 2f + new Vector2(i,j) * cellSize;
                float x = rand.RandfRange(-constraintSize / 2f, constraintSize / 2f);
                float y = rand.RandfRange(-constraintSize / 2f, constraintSize / 2f);
                var point = cellCenter + new Vector2(x,y);
                points.Add(point);
            }
        }
        if(square) AddMeshBorder(points, dim, cellSize, cellSize / 2f);
        return points;
    }

    private static void SquareMesh(List<Vector2> points, Vector2 dim)
    {
        var tl = Vector2.Zero;
        if(points.Contains(tl) == false) points.Add(tl);

        var tr = Vector2.Right * dim.x;
        if(points.Contains(tr) == false) points.Add(tr);

        var bl = Vector2.Down * dim.y;
        if(points.Contains(bl) == false) points.Add(bl);

        var br = dim;
        if(points.Contains(br) == false) points.Add(br);
    }
    private static void AddMeshBorder(List<Vector2> points, 
                                        Vector2 dim, 
                                        float cellSize,
                                        float margin)
    {
        int xPoints = Mathf.CeilToInt((dim.x + margin * 2f) / cellSize);
        int yPoints = Mathf.CeilToInt((dim.y + margin * 2f) / cellSize);

        for (int i = 0; i < xPoints; i++)
        {
            var top = new Vector2(i * cellSize, 0f) - Vector2.One * margin;
            var bottom = new Vector2(i * cellSize, dim.y) + Vector2.One * margin;
            if(points.Contains(top) == false) points.Add(top);
            if(points.Contains(bottom) == false) points.Add(bottom);
        }
        for (int i = 0; i < yPoints; i++)
        {
            var left = new Vector2(0f, i * cellSize) - Vector2.One * margin;
            var right = new Vector2(dim.x, i * cellSize) + Vector2.One * margin;
            if(points.Contains(left) == false) points.Add(left);
            if(points.Contains(right) == false) points.Add(right);
        }
        var bl = new Vector2(-margin, cellSize * (yPoints - .5f));
        var tr = new Vector2(cellSize * (xPoints - .5f), -margin);
        if(points.Contains(bl) == false) points.Add(bl);
        if(points.Contains(tr) == false) points.Add(tr);
    }
}