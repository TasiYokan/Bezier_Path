using System;
using System.Collections.Generic;
using System.Linq;

public static class List_Extension
{
    public static void Resize<T>(this List<T> _list, int _size, T _newFill)
    {
        int cur = _list.Count;
        if (_size < cur)
            _list.RemoveRange(_size, cur - _size);
        else if (_size > cur)
        {
            if (_size > _list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                _list.Capacity = _size;
            _list.AddRange(Enumerable.Repeat(_newFill, _size - cur));
        }
    }
    public static void Resize<T>(this List<T> _list, int _size) where T : new()
    {
        Resize(_list, _size, new T());
    }
}
