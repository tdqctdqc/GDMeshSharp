using Godot;
using System;
using System.Collections.Generic;

public class TheaterReserve 
{
    public Theater Theater { get; set; }
    public List<Army> Armies { get; private set; }

    public TheaterReserve(Theater theater)
    {
        Theater = theater;
        Armies = new List<Army>();
    }

    public void Add(Army army)
    {
        Armies.Add(army);
        army.SetGoal(Theater.Position, null, null);
        army.SetFlanks(Theater.Position + Vector2.One * 100f, 
            Theater.Position - Vector2.One * 100f);
    }

    public void Remove(Army army)
    {
        Armies.Remove(army);
    }
    public void AddRange(IEnumerable<Army> armies)
    {

        Armies.AddRange(armies);
        foreach (var army in armies)
        {
            army.SetGoal(Theater.Position, null, null);
            army.SetFlanks(Theater.Position + Vector2.One * 100f, 
                Theater.Position - Vector2.One * 100f);
        }
    }
}
