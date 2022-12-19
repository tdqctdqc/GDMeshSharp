using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HashList<T>
{
    public T this[int i] => List[i];
    public List<T> List { get; private set; }
    public HashSet<T> Hash { get; private set; }
    public int Count => List.Count;
    public HashList()
    {
        List = new List<T>();
        Hash = new HashSet<T>();
    }
    public HashList(IEnumerable<T> enumerable)
    {
        List = enumerable.ToList();
        Hash = new HashSet<T>(enumerable);
    }

    public bool Insert(int index, T t)
    {
        if (Hash.Contains(t)) return false;
        Hash.Add(t);
        List.Insert(index, t);
        return true;
    }
    public bool Add(T t)
    {
        if (Hash.Contains(t)) return false;
        Hash.Add(t);
        List.Add(t);
        return true;
    }

    public void Remove(T t)
    {
        if (Hash.Contains(t) == false) return;
        Hash.Remove(t);
        List.Remove(t);
    }

    public bool Contains(T t)
    {
        return Hash.Contains(t);
    }
}
