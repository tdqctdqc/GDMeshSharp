using Godot;
using System;

public class PolygonEdgeInfo 
{
    public float Friction { get; private set; }

    public PolygonEdgeInfo()
    {
    }

    public void SetFriction(float friction)
    {
        Friction = friction;
    }
}
