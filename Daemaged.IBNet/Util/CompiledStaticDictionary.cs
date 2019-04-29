using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace IBNet.Util
{
  public class CompiledStaticDictionary<TK, TV> : IDictionary<TK, TV>
  {
    Func<TK, TV> _mapper;
    Func<TK, bool> _existenceTester;
    Dictionary<TK, TV> _dict;
    bool _inBatch;

    public CompiledStaticDictionary(IDictionary<TK, TV> dict=null)
    {
      _dict = dict == null ? new Dictionary<TK, TV>() : new Dictionary<TK, TV>(dict);
      RegenerateCompiledFunctions();
    }

    TV RegenerateCompiledFunctions()
    {
      Func<TK, TV> newMapper;
      Func<TK, bool> newexistenceTester;
      if (_dict.Count == 0) {
        newMapper = x => { throw new KeyNotFoundException(); };
        newexistenceTester = x => false;
      } else {
        newMapper = StaticMapperCompiler.CompileDictionaryFunc(_dict);
        newexistenceTester = StaticMapperCompiler.CompileHashSetFunc(_dict.Select(x => x.Key));
      }
      Interlocked.Exchange(ref _mapper, newMapper);
      Interlocked.Exchange(ref _existenceTester, newexistenceTester);
      if (_dict.Count > 0)
      {
        var anyKey = _dict.Keys.First();
        _existenceTester(anyKey);
        return _mapper(anyKey);
      }
      return default(TV);
    }

    /// <summary>
    /// Puts the dictionary into batch mode. During this time, updates are fast but are not
    /// reflected in *most* view/read functions. EndBatch synchronizes the updates and
    /// makes them appear in all views.
    /// </summary>
    public void StartBatch()
    {
      Debug.Assert(!_inBatch);
      _inBatch = true;
    }

    public void EndBatch()
    {
      Debug.Assert(_inBatch);
      _inBatch = false;
      RegenerateCompiledFunctions();
    }

    #region IDictionary<string,Type> Members

    public void Add(TK key, TV value)
    {
      _dict.Add(key, value);
      if (!_inBatch)
        RegenerateCompiledFunctions();
    }

    public bool ContainsKey(TK key)
    {
      return _existenceTester(key);
    }

    public ICollection<TK> Keys => _dict.Keys;

    public bool Remove(TK key)
    {
      var r = _dict.Remove(key);
      if (r && !_inBatch)
        RegenerateCompiledFunctions();
      return r;
    }

    public bool TryGetValue(TK key, out TV value)
    {
      if (_existenceTester(key))
      {
        value = _mapper(key);
        return true;
      }
      value = default(TV);
      return false;
    }

    public ICollection<TV> Values => _dict.Values;

    public TV this[TK key]
    {
      get => _mapper(key);
      set
      {
        _dict[key] = value;
        if (!_inBatch)
          RegenerateCompiledFunctions();
      }
    }

    public void Add(KeyValuePair<TK, TV> item)
    {
      Add(item.Key, item.Value);
      if (!_inBatch)
        RegenerateCompiledFunctions();
    }

    public void Clear()
    {
      _dict.Clear();
      if (!_inBatch)
        RegenerateCompiledFunctions();
    }

    public bool Contains(KeyValuePair<TK, TV> item)
    {
      return ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
    {
      throw new NotImplementedException();
      //_dict.CopyTo(array, arrayIndex);
    }

    public int Count => _dict.Count;

    public bool IsReadOnly => false;

    public bool Remove(KeyValuePair<TK, TV> item)
    {
      return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
      return _dict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }

  public class CompiledStaticDictionary<TK, TK2, TV> : IDictionary<TK, TV>
  {
    Func<TK2, TV> _mapper;
    Func<TK2, bool> _existenceTester;
    Dictionary<TK, TV> _dict;
    Func<TK, TK2> _keyMapper;
    bool _inBatch;

    public CompiledStaticDictionary(Func<TK, TK2> keyMapper, IDictionary<TK, TV> dict=null)
    {
      if (keyMapper == null)
        throw new ArgumentNullException("keyMapper");
      _keyMapper = keyMapper;
      _dict = dict == null ? new Dictionary<TK, TV>() : new Dictionary<TK, TV>(dict);
      RegenerateCompiledFunctions();
    }

    TV RegenerateCompiledFunctions()
    {
      Func<TK2, TV> newMapper;
      Func<TK2, bool> newexistenceTester;
      if (_dict.Count == 0)
      {
        newMapper = x => { throw new KeyNotFoundException(); };
        newexistenceTester = x => false;
      }
      else
      {
        newMapper = StaticMapperCompiler.CompileDictionaryFunc(_dict.ToDictionary(kv => _keyMapper(kv.Key), kv => kv.Value));
        newexistenceTester = StaticMapperCompiler.CompileHashSetFunc(_dict.Select(x => _keyMapper(x.Key)));
      }
      Interlocked.Exchange(ref _mapper, newMapper);
      Interlocked.Exchange(ref _existenceTester, newexistenceTester);
      if (_dict.Count > 0)
      {
        var anyKey = _keyMapper(_dict.Keys.First());
        _existenceTester(anyKey);
        return _mapper(anyKey);
      }
      return default(TV);
    }

    /// <summary>
    /// Puts the dictionary into batch mode. During this time, updates are fast but are not
    /// reflected in *most* view/read functions. EndBatch synchronizes the updates and
    /// makes them appear in all views.
    /// </summary>
    public void StartBatch()
    {
      Debug.Assert(!_inBatch);
      _inBatch = true;
    }

    public void EndBatch()
    {
      Debug.Assert(_inBatch);
      _inBatch = false;
      RegenerateCompiledFunctions();
    }

    #region IDictionary<string,Type> Members

    public void Add(TK key, TV value)
    {
      _dict.Add(key, value);
      if (!_inBatch)
        RegenerateCompiledFunctions();
    }

    public bool ContainsKey(TK key)
    {
      return _existenceTester(_keyMapper(key));
    }

    public ICollection<TK> Keys => _dict.Keys;

    public bool Remove(TK key)
    {
      var r = _dict.Remove(key);
      if (r && !_inBatch)
        RegenerateCompiledFunctions();
      return r;
    }

    public bool TryGetValue(TK key, out TV value)
    {
      if (_existenceTester(_keyMapper(key)))
      {
        value = _mapper(_keyMapper(key));
        return true;
      }
      value = default(TV);
      return false;
    }

    public ICollection<TV> Values => _dict.Values;

    public TV this[TK key]
    {
      get => _mapper(_keyMapper(key));
      set
      {
        _dict[key] = value;
        if (!_inBatch)
          RegenerateCompiledFunctions();
      }
    }

    public void Add(KeyValuePair<TK, TV> item)
    {
      Add(item.Key, item.Value);
      if (!_inBatch)
        RegenerateCompiledFunctions();
    }

    public void Clear()
    {
      _dict.Clear();
      if (!_inBatch)
        RegenerateCompiledFunctions();
    }

    public bool Contains(KeyValuePair<TK, TV> item)
    {
      return ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
    {
      throw new NotImplementedException();
      //_dict.CopyTo(array, arrayIndex);
    }

    public int Count => _dict.Count;

    public bool IsReadOnly => false;

    public bool Remove(KeyValuePair<TK, TV> item)
    {
      return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
      return _dict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}