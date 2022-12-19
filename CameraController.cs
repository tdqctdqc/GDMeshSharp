using Godot;
using System;

public class CameraController : Camera2D
{
    private float _zoomSpeed = 1000f;

    public override void _Ready()
    {
        Current = true;
    }

    public override void _Process(float delta)
    {
        if(Input.IsKeyPressed((int)KeyList.W))
        {
            Position += Vector2.Up * delta * Zoom * _zoomSpeed;
        }
        if(Input.IsKeyPressed((int)KeyList.A))
        {
            Position += Vector2.Left * delta * Zoom * _zoomSpeed;
        }
        if(Input.IsKeyPressed((int)KeyList.S))
        {
            Position += Vector2.Down * delta * Zoom * _zoomSpeed;
        }
        if(Input.IsKeyPressed((int)KeyList.D))
        {
            Position += Vector2.Right * delta * Zoom * _zoomSpeed;
        }


        if(Input.IsKeyPressed((int)KeyList.Z))
        {
            Zoom *= .9f;
        }
        if(Input.IsKeyPressed((int)KeyList.X))
        {
            Zoom *= 1.1f;
        }
    }
}