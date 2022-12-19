using Godot;
using System;

public class Location
{
    public Vector2 Position { get; private set; }
    public Triangle Tri { get; private set; }

    public Location(Triangle tri, Vector2 pos)
    {
        Tri = tri;
        Position = pos;
    }
}
