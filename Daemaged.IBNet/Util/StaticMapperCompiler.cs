using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IBNet.Util
{
  public static class StaticMapperCompiler
  {
    private static bool IsNativeInt(Type t)
    {
      return (t == typeof (IntPtr) || t == typeof (UIntPtr));
    }

    private enum KeyType
    {
      Integer,
      String,
      Unsuppoeted
    }

    private static KeyType GetKeyType(Type keyType)
    {
      var typeCode = Type.GetTypeCode(keyType);
      if (IsNativeInt(keyType))
        return KeyType.Integer;

      switch (typeCode)
      {
        case TypeCode.Char:
        case TypeCode.SByte:
        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return KeyType.Integer;

        case TypeCode.String:
          return KeyType.String;
        default:
          return KeyType.Unsuppoeted;         
      }           
    }

    public static Func<TK, bool> CompileHashSetFunc<TK>(IEnumerable<TK> data)
    {
      switch (GetKeyType(typeof(TK)))
      {
        case KeyType.Integer:
          return CompileIntegerMapper(data.Select(x => new KeyValuePair<TK, bool>(x, true)), false, false);
        case KeyType.String:
          return CompileStringMapper<TK, bool>(data.Cast<string>().Select(x => new KeyValuePair<string, bool>(x, true)), false, false);
        default:
          throw new NotSupportedException("The type is not supported for switches");
      }
    }

    public static Func<TK, TV> CompileDictionaryFunc<TK, TV>(IEnumerable<KeyValuePair<TK, TV>> data)
    {
      switch (GetKeyType(typeof(TK))) {
        case KeyType.Integer:
          return CompileIntegerMapper(data);          
        case KeyType.String:
          return CompileStringMapper<TK, TV>(data.Cast<KeyValuePair<string, TV>>());
        default:
          throw new NotSupportedException("The type is not supported for switches");
      }
    }

    private static Func<TK, TV> CompileIntegerMapper<TK, TV>(IEnumerable<KeyValuePair<TK, TV>> data, bool throwWhenNotFound = true, TV notFoundValue = default(TV))
    {
      var handleIntPtr = IsNativeInt(typeof (TK));
      
      var param = Expression.Parameter(typeof(TK));
      var realT = typeof(TK);
      Expression realParam = param;
      Func<object, object> keyUnwrapper = (k) => k;
      if (handleIntPtr)
      {
        //realT = Type.GetType("System.NativeInt");
        if (typeof(TK) == typeof(IntPtr))
        {
          if (IntPtr.Size == 8)
          {
            realT = typeof(long);
            keyUnwrapper = (k) => ((IntPtr)k).ToInt64();
          }
          else
          {
            realT = typeof(int);
            keyUnwrapper = (k) => ((IntPtr)k).ToInt32();
          }
        }
        else
        { // UIntPtr        
          //Type.GetType("System.NativeUInt");
          if (UIntPtr.Size == 8)
          {
            realT = typeof(ulong);
            keyUnwrapper = (k) => ((UIntPtr)k).ToUInt64();
          }
          else
          {
            realT = typeof(uint);
            keyUnwrapper = (k) => ((UIntPtr)k).ToUInt32();
          }
        }
        realParam = Expression.Convert(param, realT);
      }
      //var t = typeof (void*);     
      //Console.WriteLine(t.ToString());

      var defaultExpr = throwWhenNotFound ? 
        (Expression) Expression.Block(
          Expression.Throw(GenerateKeyNotFoundException(param)),
          Expression.Constant(notFoundValue, typeof (TV))) : 
        Expression.Constant(notFoundValue);

      var sw = Expression.Switch(realParam, defaultExpr,
        data.GroupBy(kv => kv.Value).Select(g =>
          Expression.SwitchCase(Expression.Constant(g.Key),
                                g.Select(x => Expression.Constant(keyUnwrapper(x.Key))).ToArray())).ToArray());


      return Expression.Lambda<Func<TK, TV>>(sw, new[] { param }).Compile();
    }

    private static Expression GenerateKeyNotFoundException(ParameterExpression param)
    {
      return Expression.New(typeof(KeyNotFoundException).GetConstructor(new[] { typeof(String)}),
            Expression.Call(typeof(String).GetMethod("Format", new[] { typeof(String), typeof(object) }), Expression.Constant("The key was not found: {0}"), Expression.Convert(param, typeof(object))));
    }

    // ReSharper disable StaticFieldInGenericType
    private static readonly MethodInfo _stringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string) });
    private static readonly MethodInfo _stringIndex = typeof(string).GetMethod("get_Chars");
    private static readonly MethodInfo _stringLength = typeof(string).GetMethod("get_Length");
    // ReSharper restore StaticFieldInGenericType


    private static Func<TK, TV> CompileStringMapper<TK, TV>(IEnumerable<KeyValuePair<string, TV>> dict, bool throwWhenNotFound = true, TV notFoundValue = default(TV))
    {
      var cases = dict.Select(pair => new SwitchCase<TV>(pair.Key, pair.Value)).ToList();
      var keyParameter = Expression.Parameter(typeof(string), "key");
      
      var defaultExpr = throwWhenNotFound ?
        (Expression)Expression.Block(
          Expression.Throw(GenerateKeyNotFoundException(keyParameter)),
          Expression.Constant(notFoundValue, typeof(TV))) :
        Expression.Constant(notFoundValue);
      var expr = Expression.Lambda<Func<TK, TV>>(
        SwitchOnLength(keyParameter, defaultExpr, cases.OrderBy(switchCase => switchCase.Key.Length).ToArray(), 0, cases.Count - 1), new[] { keyParameter });
      var del = expr.Compile();
      return del;      
    }


    private static Expression SwitchOnLength<T>(ParameterExpression keyParameter, Expression defaultExpr, SwitchCase<T>[] switchCases, int lower, int upper)
    {
      if (switchCases[lower].Key.Length == switchCases[upper].Key.Length)
        return SwitchOnChar(keyParameter, defaultExpr, switchCases.Skip(lower).Take(upper - lower + 1).ToArray(), 0, 0, upper - lower);
      var middle = GetIndexOfFirstDifferentCaseFromUp(switchCases, lower, MidPoint(lower, upper), upper,
                                                      switchCase => switchCase.Key.Length);
      if (middle == -1)
        throw new InvalidOperationException();
      return Expression.Condition(
        Expression.LessThan(Expression.Call(keyParameter, _stringLength),
                            Expression.Constant(switchCases[middle + 1].Key.Length)),
        SwitchOnLength(keyParameter, defaultExpr, switchCases, lower, middle),
        SwitchOnLength(keyParameter, defaultExpr, switchCases, middle + 1, upper));
    }

    private static Expression SwitchOnChar<T>(ParameterExpression keyParameter, Expression defaultExpr, SwitchCase<T>[] switchCases, int index, int lower, int upper)
    {
      if (lower == upper)
      {
        return Expression.Condition(
          Expression.Call(_stringEquals, keyParameter, Expression.Constant(switchCases[lower].Key)),
          Expression.Convert(Expression.Constant(switchCases[lower].Value), typeof(T)), defaultExpr);
      }

      if (index == switchCases[upper].Key.Length)
        return null;

      switchCases = switchCases.Skip(lower).Take(upper - lower + 1)
        .OrderBy(switchCase => switchCase.Key, StaticStringDictionaryComparer.For(index)).ToArray();

      upper = upper - lower;
      lower = 0;

      var middle = MidPoint(lower, upper);

      if (switchCases[lower].Key[index] == switchCases[middle].Key[index])
      {
        var result = SwitchOnChar(keyParameter, defaultExpr, switchCases, index + 1, lower, upper);
        if (result != null)
          return result;
      }

      middle = GetIndexOfFirstDifferentCaseFromUp(switchCases, lower, middle, upper, switchCase => switchCase.Key[index]);
      if (middle == -1)
        return null;

      var trueBranch = SwitchOnChar(keyParameter, defaultExpr, switchCases, index, lower, middle);
      if (trueBranch == null)
        return null;

      var falseBranch = SwitchOnChar(keyParameter, defaultExpr, switchCases, index, middle + 1, upper);
      if (falseBranch == null)
        return null;

      return Expression.Condition(
        Expression.LessThan(Expression.Call(keyParameter, _stringIndex, Expression.Constant(index)),
                            Expression.Constant(switchCases[middle + 1].Key[index])),
        trueBranch,
        falseBranch);
    }

    private static int MidPoint(int lower, int upper)
    {
      return ((upper - lower + 1) / 2) + lower;
    }

    private static int GetIndexOfFirstDifferentCaseFromUp<T, K>(SwitchCase<T>[] cases, int lower, int middle, int upper,
                                                             Func<SwitchCase<T>, K> selector)
    {
      var firstValue = selector(cases[middle]);
      for (var i = middle - 1; i >= lower; --i)
        if (!firstValue.Equals(selector(cases[i])))
          return i;
      for (var i = middle + 1; i <= upper; ++i)
        if (!firstValue.Equals(selector(cases[i])))
          return i - 1;
      return -1;
    }

    #region Nested type: SwitchCase

    private struct SwitchCase<T>
    {
      public readonly string Key;
      public readonly T Value;

      public SwitchCase(string key, T value)
      {
        Key = key;
        Value = value;
      }

      public override string ToString() { return Key + " " + Value; }
    }

    #endregion
  }
}