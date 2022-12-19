using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Army
{
    public int Id { get; private set; }
    public List<FieldUnit> FieldUnits { get; private set; }
    public FieldUnit Left => FieldUnits[0];
    public FieldUnit Right => FieldUnits[FieldUnits.Count - 1];
    public Faction Faction { get; private set; }
    public Vector2 GoalPoint { get; private set; }
    public Vector2 LeftFlankPoint { get; private set; }
    public Vector2 RightFlankPoint { get; private set; }
    public Vector2 HqPosition => GetHqPosition();
    public Location Defend { get; private set; }
    public Location Attack { get; private set; }
    public Army(int id, Faction faction)
    {
        Faction = faction;
        Id = id;
        FieldUnits = new List<FieldUnit>();
        GoalPoint = Vector2.Zero;
    }

    public void SetFlanks(Vector2 leftFlank, Vector2 rightFlank)
    {
        LeftFlankPoint = leftFlank;
        RightFlankPoint = rightFlank;
    }
    public void SetGoal(Vector2 goal, Location defend, Location attack)
    {
        GoalPoint = goal;
        Defend = defend;
        Attack = attack;
        if(defend != null)
            FieldUnits.ForEach(u => u.Teleport(defend.Position));
    }

    public void ClearGoal()
    {
        Attack = null;
        Defend = null;
    }
    public Vector2 GetPosition()
    {
        if (FieldUnits.Count == 0) return Vector2.Zero;

        return FieldUnits.Select(u => u.CenterPosition).Aggregate((v, w) => v + w) / (float) FieldUnits.Count;
    }

    private Vector2 GetHqPosition()
    {
        var mid = (LeftFlankPoint + RightFlankPoint) / 2f;
        var perp = (RightFlankPoint - LeftFlankPoint).Normalized().Rotated(-Mathf.Pi / 2f);
        return mid + perp * 25f;
        
        
        if(Defend != null)
            return (GetPosition() + Defend.Position) / 2f;
        return (GetPosition() + GoalPoint) / 2f;
    }
}
