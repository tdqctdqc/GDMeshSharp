using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExt 
{
    public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
    {
        var index = Game.Random.RandiRange(0, enumerable.Count() - 1);
        return enumerable.ElementAt(index);
    }
}
