using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

  internal class TWSField
  {
    public string Name { get; set; }
    public TWSType Type { get; set; }
    public int SupportedSince { get; set; }
    
  }

  internal class TWSField<TMessage, TField> : TWSField
  {
    public Expression<Func<TMessage, TField>> Selector;
    public Expression<Func<TWSClient, TMessage, TField>> Selector2;
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

  internal class TWSMessageDefinitions
  {
    internal TWSMessage<IBContract> RequestContractDetailsMessage = new TWSMessage<IBContract> {      
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
    
  }
}