using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IBNet.Util
{
  public static class Enum<T>
  {
    public static T Parse(int value)
    { return (T)Enum.ToObject(typeof(T), value); }

    public static string Description(T value)
    {
      var da =
        (DescriptionAttribute[])
        (typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false));
      return da.Length > 0 ? da[0].Description : value.ToString();
    }

    public static T Parse(string value)
    { return (T)Enum.Parse(typeof(T), value); }

    public static IList<T> GetValues()
    {
      IList<T> list = new List<T>();
      foreach (var value in Enum.GetValues(typeof(T)))
      {
        list.Add((T)value);
      }
      return list;
    }
  }  
}
