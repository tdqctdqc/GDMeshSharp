using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public static class MethodExt
{
    private static Stopwatch _sw = new Stopwatch();
    public static void TryParallel<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        try
        {
            Parallel.ForEach(enumerable, action);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
