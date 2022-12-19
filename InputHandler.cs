using Godot;
using System;

public class InputHandler
{
    private WorldManager _world;
    private Ui _ui;
    private MouseDragBehavior _drag;
    
    public InputHandler(WorldManager world, Graphics graphics, Ui ui)
    {
        _world = world;
        _ui = ui;

        _drag = new DrawTriIntersectionsLine(2, world);
    }

    public void Process()
    {
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        _drag.Process(mousePos);
    }
    public void HandleInput(InputEvent e)
    {
        var mousePos = Game.I.Graphics.GetGlobalMousePosition();
        if (e is InputEventMouseButton m)
        {
            _drag.HandleInput(m);
        }

        if (e is InputEventKey k 
            && k.Pressed == false)
        {
            if(k.Scancode == (int)KeyList.B)
            {
            }
            else if (k.Scancode == (int) KeyList.T)
            {
                TransferTriangle(mousePos);
            }
        }
        
    }


    private void TransferTriangle(Vector2 mousePos)
    {
        Game.I.Commands.TransferTriangle(mousePos);
        Game.I.Graphics.FactionGraphics.Stop();
        Game.I.Graphics.FactionGraphics.Start();
    }
}
