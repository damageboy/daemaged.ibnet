using System;
using System.Collections.Generic;

namespace IBNet.Util
{
  internal class StaticStringDictionaryComparer : IComparer<string>
  {
    static readonly Dictionary<int, IComparer<string>> _comparers = new Dictionary<int, IComparer<string>>();
    readonly int _startIndex;

    public StaticStringDictionaryComparer(int startIndex)
    {
      this._startIndex = startIndex;
    }

    #region IComparer<string> Members

    public int Compare(string x, string y)
    {
      if (x.Length != y.Length)
        throw new InvalidOperationException();

      for (var i = _startIndex; i < x.Length; i++) {
        if (x[i] > y[i])
          return 1;
        if (x[i] < y[i])
          return -1;
      }

      return 0;
    }

    #endregion

    public static IComparer<string> For(int startIndex)
    {
      if (!_comparers.TryGetValue(startIndex, out var comparer))
      {
        comparer = new StaticStringDictionaryComparer(startIndex);
        _comparers.Add(startIndex, comparer);
      }
      return comparer;
    }
  }
}