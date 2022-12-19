using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EdgeSet<T>
{
    private Dictionary<T, HashSet<T>> _lowHashEdgePairs;

    
    public EdgeSet()
    {
        _lowHashEdgePairs = new Dictionary<T, HashSet<T>>();
    }
    public List<T> GetEdgePairs()
    {
        var list = new List<T>();
        var keys = _lowHashEdgePairs.Keys.ToList();
        var values = _lowHashEdgePairs.Values.ToList();
        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            for (var j = 0; j < values[i].Count; j++)
            {
                list.Add(key);
                list.Add(values[i].ElementAt(j));
            }
        }
        return list;
    }
    public void Add(T t1, T t2)
    {
        var highHash = t1.GetHashCode() > t2.GetHashCode()
            ? t1
            : t2;
        var lowHash = t1.Equals(highHash) ? t2 : t1;
        if (_lowHashEdgePairs.ContainsKey(lowHash) == false)
        {
            _lowHashEdgePairs.Add(lowHash, new HashSet<T>());
        }
        _lowHashEdgePairs[lowHash].Add(highHash);
    }
    
    public bool Contains(T t1, T t2)
    {
        var highHash = t1.GetHashCode() > t2.GetHashCode()
            ? t1
            : t2;
        var lowHash = t1.Equals(highHash) ? t2 : t1;
        if (_lowHashEdgePairs.ContainsKey(lowHash) == false) return false;
        return _lowHashEdgePairs[lowHash].Contains(highHash);
    }
}
