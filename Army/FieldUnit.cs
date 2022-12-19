using Godot;
using System;
using System.Linq;

public class FieldUnit
{
    public int Id { get; private set; }
    public Vector2 LinePosition { get; private set; }
    public Vector2 CenterPosition { get; private set; }
    public Vector2 LeftFlankPosition { get; private set; }
    public Vector2 RightFlankPosition { get; private set; }
    public float Strength { get; private set; } = 1f;
    public float Speed { get; private set; } = 20f;
    public Army Army { get; private set; }
    public FieldUnit(int id, Army army)
    {
        Army = army;
        Id = id;
    }

    public void Teleport(Vector2 pos)
    {
        CenterPosition = pos;
    }
    public void Process(WorldManager world, float delta)
    {
        var move = GetMove(world);
        CenterPosition = CenterPosition + move.Normalized() * Speed * delta;
        
    }

    private Vector2 GetMove(WorldManager world)
    {
        Vector2 move = Vector2.Zero;
        //pull to army goal point
        if (Army.Attack == null)
        {
            move = Army.GoalPoint - CenterPosition;
            return move;
        }
        var goal = Army.GoalPoint;
        var attackLoc = Army.Attack.Position;
        var defendLoc = Army.Defend.Position;
        var goalAxis = attackLoc - defendLoc;
        var leftPerp = goalAxis.Rotated(Mathf.Pi / 2f).Normalized();
        var rightPerp = goalAxis.Rotated(-Mathf.Pi / 2f).Normalized();
        move += ((goal - CenterPosition) / 100f ) * 1f;
        
        
        var indexInArmy = Army.FieldUnits.IndexOf(this);
        float smallDist = 5f;
        if (this == Army.Left)
        {
            if(Army.LeftFlankPoint.DistanceTo(CenterPosition) < smallDist) return Vector2.Zero;
            move += (Army.LeftFlankPoint - CenterPosition).Normalized() * 2f;
        }
        else if (this == Army.Right)
        {
            if(Army.RightFlankPoint.DistanceTo(CenterPosition) < smallDist) return Vector2.Zero;
            move += (Army.RightFlankPoint - CenterPosition).Normalized() * 2f;
        }
        else
        {
            var progress = (float) indexInArmy / (float) Army.FieldUnits.Count;
            var pos = Army.LeftFlankPoint.LinearInterpolate(Army.RightFlankPoint, progress);
            if(pos.DistanceTo(CenterPosition) < smallDist) return Vector2.Zero;
            move += (pos - CenterPosition).Normalized() * 2f;
        }

        var closeUnits = world.Armies.UnitGrid.GetElementsInRadius(CenterPosition, 10f);
        foreach (var closeUnit in closeUnits)
        {
            if (closeUnit.Army.Faction != Army.Faction)
            {
                move -= (closeUnit.CenterPosition - Army.HqPosition).Normalized() * 5f;
            }
        }
        
        //push from too close units

        // float minDist = 25f;
        // for (int i = 0; i < Army.FieldUnits.Count; i++)
        // {
        //     if (i == indexInArmy) continue;
        //     var unit = Army.FieldUnits[i];
        //     if (unit.CenterPosition.DistanceTo(CenterPosition) < minDist)
        //     {
        //         move += -(unit.CenterPosition - CenterPosition).Normalized() * 1f;
        //     }
        // }
        
        return move;
    }
}
