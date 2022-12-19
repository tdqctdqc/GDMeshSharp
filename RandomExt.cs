using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class RandomExt
{
    public static T GetRandomElementByWeight<T>(List<T> elements, List<float> weights)
    {
        var weightSum = weights.Sum();
        var value = Game.Random.RandfRange(0f, weightSum);
        var iter = 0f;
        for (int i = 0; i < elements.Count - 1; i++)
        {
            iter += weights[i];
            if (value < iter) return elements[i];
        }
        return elements[elements.Count - 1];
    }
}
