using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Daemaged.IBNet.Client;

namespace Daemaged.IBNet.Dsl
{
  internal class TWSMessage<T> : List<TWSField>
  {
    public void Add(Expression<Func<T, bool>> selector, int minVersion = 0)
    {
      Add(new TWSField<T, bool> { Type = TWSType.Boolean, Selector = selector, SupportedSince = minVersion });
    }

    public void Add(Expression<Func<T, int>> selector, int minVersion = 0)
    {
      Add(new TWSField<T, int> {Type = TWSType.Int, Selector = selector, SupportedSince = minVersion});
    }

    public void Add(Expression<Func<T, string>> selector, int minVersion = 0)
    {
      Add(new TWSField<T, string> {Type = TWSType.String, Selector = selector, SupportedSince = minVersion });
    }

    public void Add(Expression<Func<T, double>> selector, int minVersion = 0)
    {
      Add(new TWSField<T, double> {Type = TWSType.Double, Selector = selector, SupportedSince = minVersion});
    }

    public void Add(Expression<Func<T, Enum>> selector, int minVersion = 0)
    {
      Add(new TWSField<T, Enum> { Type = TWSType.Enum, Selector = selector, SupportedSince = minVersion });
    }

    public void Add(Expression<Func<T, DateTime?>> selector, int minVersion = 0)
    {
      Add(new TWSField<T, DateTime?> { Type = TWSType.ExpiryDate, Selector = selector, SupportedSince = minVersion });
    }

  }

  internal abstract class TWSField
  {
    internal string Name { get; set; }
    internal TWSType Type { get; set; }
    internal int SupportedSince { get; set; }
    internal abstract Expression AbstractSelector { get; }

  }

  internal class TWSField<TMessage, TField> : TWSField
  {
    public Expression<Func<TMessage, TField>> Selector;

    internal override Expression AbstractSelector
    {
      get { return Selector; }
    }
  }

  internal enum TWSType
  {
    Boolean,
    Int,
    Double,
    String,
    ExpiryDate,
    Enum,
  }

  internal static class TWSMsgDefs
  {
    internal static TWSMessage<IBContract> ReqContractDetailsMsg = new TWSMessage<IBContract> {      
      { _ => ServerMessage.RequestContractData},
      { _ => 6 },
      { c => c.RequestId, 40},
      { c => c.ContractId, 37 },
      { c => c.Symbol },
      { c => c.SecurityType},
      { c => c.Expiry},
      { c => c.Strike},
      { c => c.Right },
      { c => c.Multiplier, 15},
      { c => c.Exchange},
      { c => c.Currency},
      { c => c.LocalSymbol},
      { c => c.IncludeExpired, 31},
      { c => c.SecurityIdType, TWSServerInfo.MIN_SERVER_VER_SEC_ID_TYPE},
      { c => c.SecurityId, TWSServerInfo.MIN_SERVER_VER_SEC_ID_TYPE},
    };  
  }

  internal class TWSEncoderGenerator
  {
    public static Action<TWSClient, T> GetEncoderFunc<T>(TWSMessage<T> template)
    {
      var clientParam = Expression.Parameter(typeof(TWSClient));
      var tParam = Expression.Parameter(typeof(T));

      BlockExpression block = Expression.Block(template.Select(f => GenerateFieldEncoder<T>(f, clientParam, tParam)));
      return Expression.Lambda<Action<TWSClient, T>>(block, new[] { clientParam, tParam}).Compile();
    }

    private static Expression GenerateFieldEncoder<T>(TWSField field, ParameterExpression clientParam, ParameterExpression tParam)
    {
      var encoder = Expression.PropertyOrField(clientParam, "Encoding");
      var op = Expression.Call(encoder, 
                               GetEncodeMethodInfoForField(field), 
                               encoder,
                               GetEncodeParam(tParam, field));

      if (field.SupportedSince > 0)
        return
          Expression.IfThen(
            Expression.GreaterThanOrEqual(Expression.PropertyOrField(clientParam, "ServerVersion"),
                                          Expression.Constant(field.SupportedSince)), op);
      
      return op;
    }

    private static Expression GetEncodeParam(ParameterExpression tParam, TWSField field)
    {
      var lmbd = (LambdaExpression)field.AbstractSelector;
      var me = (MemberExpression) lmbd.Body;
      return Expression.PropertyOrField(tParam, me.Member.Name);
    }

    private static MethodInfo GetEncodeMethodInfoForField(TWSField field)
    {
      switch (field.Type)
      {
        case TWSType.Boolean:
          return SymbolExtensions.GetMethodInfo<ITWSEncoding>(e => e.Encode(true));
        case TWSType.Int:
          return SymbolExtensions.GetMethodInfo<ITWSEncoding>(e => e.Encode(666));
        case TWSType.Double:
          return SymbolExtensions.GetMethodInfo<ITWSEncoding>(e => e.Encode(666.6));
        case TWSType.String:
          return SymbolExtensions.GetMethodInfo<ITWSEncoding>(e => e.Encode("666"));
        case TWSType.ExpiryDate:
          return SymbolExtensions.GetMethodInfo<ITWSEncoding>(e => e.EncodeExpiryDate(DateTime.MinValue));
        case TWSType.Enum:
          var lmbd = (LambdaExpression) field.AbstractSelector;
          var member = (MemberExpression) lmbd.Body;
          return typeof (ITWSEncoding).GetMethod("Encode", new[] {member.Type});
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }

  public static class SymbolExtensions
  {
    /// <summary>
    /// Given a lambda expression that calls a method, returns the method info.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo GetMethodInfo(Expression<Action> expression)
    {
      return GetMethodInfo((LambdaExpression)expression);
    }

    /// <summary>
    /// Given a lambda expression that calls a method, returns the method info.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
    {
      return GetMethodInfo((LambdaExpression)expression);
    }

    /// <summary>
    /// Given a lambda expression that calls a method, returns the method info.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
    {
      return GetMethodInfo((LambdaExpression)expression);
    }

    /// <summary>
    /// Given a lambda expression that calls a method, returns the method info.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo GetMethodInfo(LambdaExpression expression)
    {
      var outermostExpression = expression.Body as MethodCallExpression;

      if (outermostExpression == null)
      {
        throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
      }

      return outermostExpression.Method;
    }
  }


  internal static class TWSMessageExtensions
  {
    internal static void Encode<T>(this TWSMessage<T> messageTemplate, TWSClient client, T t)
    {
      try {
        var f = TWSEncoderGenerator.GetEncoderFunc<T>(messageTemplate);
        f(client, t);
      }

      catch (Exception) {
        client.Disconnect();
        throw;
      }
    }
  }
}