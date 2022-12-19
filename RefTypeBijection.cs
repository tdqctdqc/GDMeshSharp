using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RefTypeBijection<T> where T : class
{
    public T this[T key] => Get(key);
    private Dictionary<T, T> _dic;
    public RefTypeBijection()
    {
        _dic = new Dictionary<T, T>();
    }

    private T Get(T key)
    {
        if (_dic.ContainsKey(key)) return _dic[key];
        return null;
    }

    public bool TryAdd(T t1, T t2)
    {
        if (_dic.ContainsKey(t1) || _dic.ContainsKey(t2))
        {
            return false;
        }
        Add(t1, t2);
        return true;
    }
    public void Add(T t1, T t2)
    {
        if (_dic.ContainsKey(t1)) throw new Exception();
        _dic.Add(t1, t2);
        _dic.Add(t2, t1);
    }

    public void Remove(T t)
    {
        if (_dic.ContainsKey(t) == false) return;
        var pair = _dic[t];
        _dic.Remove(t);
        _dic.Remove(pair);
    }

    public bool Contains(T t)
    {
        return _dic.ContainsKey(t);
    }
}