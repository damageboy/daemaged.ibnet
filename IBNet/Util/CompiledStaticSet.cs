using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace IBNet.Util
{
  public class CompiledStaticSet<T> : ISet<T>
  {
    Func<T, bool> _existenceTester;
    HashSet<T> _set;

    public CompiledStaticSet()
    {
      _set = new HashSet<T>();
      RegenerateCompiledFunctions();
    }

    public CompiledStaticSet(IEnumerable<T> set)
    {
      _set = new HashSet<T>(set);
      RegenerateCompiledFunctions();
    }

    void RegenerateCompiledFunctions()
    {
      Func<T, bool> newexistenceTester;
      if (_set.Count == 0) {
        newexistenceTester = x => false;
      }
      else {
        newexistenceTester = StaticMapperCompiler.CompileHashSetFunc(_set);
      }
      Interlocked.Exchange(ref _existenceTester, newexistenceTester);
    }



    public IEnumerator<T> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void ICollection<T>.Add(T item)
    {
      _set.Add(item);
      RegenerateCompiledFunctions();
    }

    public void UnionWith(IEnumerable<T> other)
    {
      _set.UnionWith(other);
      RegenerateCompiledFunctions();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
      _set.IntersectWith(other);
      RegenerateCompiledFunctions();
    }

    public void ExceptWith(IEnumerable<T> other)
    {
      _set.ExceptWith(other);
      RegenerateCompiledFunctions();
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
      _set.SymmetricExceptWith(other);
      RegenerateCompiledFunctions();
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
      return _set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
      return _set.IsSupersetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
      return _set.IsProperSupersetOf(other);
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
      return _set.IsProperSubsetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
      return _set.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other)
    {
      return _set.SetEquals(other);
    }

    public bool Add(T item)
    {
      var r = _set.Add(item);
      if (r)
        RegenerateCompiledFunctions();
      return r;
    }

    public void Clear()
    {
      _set.Clear();
      RegenerateCompiledFunctions();
    }

    public bool Contains(T item)
    {
      return _existenceTester(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
      var r = _set.Remove(item);
      if (r)
        RegenerateCompiledFunctions();
      return r;
    }

    public int Count { get { return _set.Count; } }
    public bool IsReadOnly { get { return false; } }
  }
}