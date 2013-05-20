#region Copyright (c) 2007 by Dan Shechter

////////////////////////////////////////////////////////////////////////////////////////
////
//  IBNet, an Interactive Brokers TWS .NET Client & Server implmentation
//  by Dan Shechter
////////////////////////////////////////////////////////////////////////////////////////
//  License: MPL 1.1/GPL 2.0/LGPL 2.1
//  
//  The contents of this file are subject to the Mozilla Public License Version 
//  1.1 (the "License"); you may not use this file except in compliance with 
//  the License. You may obtain a copy of the License at 
//  http://www.mozilla.org/MPL/
//  
//  Software distributed under the License is distributed on an "AS IS" basis,
//  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
//  for the specific language governing rights and limitations under the
//  License.
//  
//  The Original Code is any part of this file that is not marked as a contribution.
//  
//  The Initial Developer of the Original Code is Dan Shecter.
//  Portions created by the Initial Developer are Copyright (C) 2007
//  the Initial Developer. All Rights Reserved.
//  
//  Contributor(s): None.
//  
//  Alternatively, the contents of this file may be used under the terms of
//  either the GNU General Public License Version 2 or later (the "GPL"), or
//  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
//  in which case the provisions of the GPL or the LGPL are applicable instead
//  of those above. If you wish to allow use of your version of this file only
//  under the terms of either the GPL or the LGPL, and not to allow others to
//  use your version of this file under the terms of the MPL, indicate your
//  decision by deleting the provisions above and replace them with the notice
//  and other provisions required by the GPL or the LGPL. If you do not delete
//  the provisions above, a recipient may use your version of this file under
//  the terms of any one of the MPL, the GPL or the LGPL.
////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System;

namespace Daemaged.IBNet.Client
{
  public class TWSClientEventArgs : EventArgs
  {
    public TWSClientEventArgs(TWSClient client)
    {
      Client = client;
    }

    public TWSClient Client { get; internal set; }
  }

  public class TWSClientStatusEventArgs : TWSClientEventArgs
  {
    public TWSClientStatusEventArgs(TWSClient client, TWSClientStatus status) : base(client)
    {
      Status = status;
    }

    public TWSClientStatus Status { get; internal set; }
  }

  public class TWSClientErrorEventArgs : TWSClientEventArgs
  {
    public TWSClientErrorEventArgs(TWSClient client) : base(client) {}

    public IBContract Contract { get; internal set; }

    /// <summary>
    /// The ticker Id that was specified previously in the call to reqMktData().
    /// </summary>

    public int RequestId { get; internal set; }
    /// <summary>
    /// This is the textual description of the error, also documented in the Error Codes topic.
    /// </summary>
    /// <seealso cref="TWSError"/>
    public TWSError Error { get; internal set; }

    public string Message { get; set; }
  }

  /// <summary>
  /// Tick Price Event Args
  /// </summary>
  public class TWSTickPriceEventArgs : TWSClientEventArgs
  {
    public TWSTickPriceEventArgs(TWSClient client) : base(client) {}

    /// <summary>
    /// The ticker Id that was specified previously in the call to reqMktData().
    /// </summary>
    public int TickerId { get; internal set; }
    /// <summary>
    /// Specifies the type of price.
    /// </summary>
    public IBTickType TickType { get; internal set; }
    /// <summary>
    /// Specifies the price for the specified field.
    /// </summary>
    public double Price { get; internal set; }
    /// <summary>
    /// Specifies the size for the specified field.
    /// </summary>
    public int Size { get; internal set; }
    /// <summary>
    /// specifies whether the price tick is available for automatic execution.
    /// </summary>
    /// <remarks>Possible values are:
    /// 0 = not eligible for automatic execution
    /// 1 = eligible for automatic execution</remarks>
    public int CanAutoExecute { get; internal set; }
  }

  public class TWSTickSizeEventArgs : TWSClientEventArgs
  {
    public TWSTickSizeEventArgs(TWSClient client) : base(client) {}

    /// <summary>
    /// The ticker Id that was specified previously in the call to reqMktData().
    /// </summary>
    public int TickerId { get; internal set; }
    /// <summary>
    /// Specifies the type of price.
    /// </summary>
    public IBTickType TickType { get; internal set; }
    /// <summary>
    /// Specifies the size for the specified field.
    /// </summary>
    public int Size { get; internal set; }
  }

  public class TWSTickGenericEventArgs : TWSClientEventArgs
  {
    public TWSTickGenericEventArgs(TWSClient client) : base(client) {}
    /// <summary>
    /// The ticker Id that was specified previously in the call to reqMktData().
    /// </summary>
    public int TickerId { get; internal set; }
    /// <summary>
    /// Specifies the type of price.
    /// </summary>
    public IBTickType TickType { get; internal set; }
    /// <summary>
    /// The value of the specified field.
    /// </summary>
    public double Value { get; internal set; }
  }

  public class TWSTickStringEventArgs : TWSClientEventArgs
  {
    public TWSTickStringEventArgs(TWSClient client) : base(client) {}

    public int RequestId { get; internal set; }
    public IBTickType TickType { get; internal set; }
    public string Value { get; internal set; }
  }

  /// <summary>
  /// Exchange For Physical Event Args
  /// </summary>
  public class TWSTickEFPEventArgs : TWSClientEventArgs
  {
    public TWSTickEFPEventArgs(TWSClient client) : base(client) {}

    /// <summary>
    /// The ticker Id that was specified previously in the call to reqMktData().
    /// </summary>
    public int TickerId { get; internal set; }
    /// <summary>
    /// Specifies the type of price.
    /// </summary>
    /// <seealso cref="TickType"/>
    public IBTickType TickType { get; internal set; }
    /// <summary>
    /// Annualized basis points, which is representative of the
    /// financing rate that can be directly compared to broker rates.
    /// </summary>
    public double BasisPoints { get; internal set; }
    /// <summary>
    /// Annualized basis points as a formatted string that depicts them in percentage form.
    /// </summary>
    public string FormattedBasisPoints { get; internal set; }
    /// <summary>
    /// Implied futures price.
    /// </summary>
    public double ImpliedFuturesPrice { get; internal set; }
    /// <summary>
    /// Number of “hold days” until the expiry of the EFP.
    /// </summary>
    public int HoldDays { get; internal set; }
    /// <summary>
    /// Expiration date of the single stock future.
    /// </summary>
    public string FutureExpiry { get; internal set; }
    /// <summary>
    /// The “dividend impact” upon the annualized basis points interest rate.
    /// </summary>
    public double DividendImpact { get; internal set; }
    /// <summary>
    /// The dividends expected until the expiration of the single stock future.
    /// </summary>
    public double DividendsToExpiry { get; internal set; }
  }


  public class TWSTickOptionComputationEventArgs : TWSClientEventArgs
  {
    public TWSTickOptionComputationEventArgs(TWSClient client) : base(client) {}

    public int RequestId { get; internal set; }
    public IBTickType TickType { get; internal set; }
    public double ImpliedVol { get; internal set; }
    public double Delta { get; internal set; }
    public double OptionPrice { get; internal set; }
    public double PVDividend { get; internal set; }

    public double Gamma { get; internal set; }

    public double Vega { get; internal set; }

    public double Theta { get; internal set; }

    public double UnderlyingPrice { get; internal set; }
  }

  public class TWSCurrentTimeEventArgs : TWSClientEventArgs
  {
    public TWSCurrentTimeEventArgs(TWSClient client) : base(client) {}

    public DateTime Time { get; internal set; }
  }

  public class TWSMarketDataEventArgs : TWSClientEventArgs
  {
    public TWSMarketDataEventArgs(TWSClient client) : base(client) {}

    public IBTickType TickType { get; internal set; }
    public TWSMarketDataSnapshot Snapshot { get; internal set; }
  }

  /// <summary>
  /// Order Status Event Args
  /// </summary>
  public class TWSOrderStatusEventArgs : TWSClientEventArgs
  {
    public TWSOrderStatusEventArgs(TWSClient client) : base(client) {}

    /// <summary>
    /// The order Id that was specified previously in the call to PlaceOrder().
    /// </summary>
    /// <value>
    /// The order id.
    /// </value>
    public int OrderId { get; internal set; }
    /// <summary>
    /// The order status.
    /// </summary>
    /// <remarks>Possible values include:
    /// <list type="table">
    /// <listheader>
    /// <term>Status</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>PendingSubmit</term>
    /// <description>indicates that you have transmitted the order, but have not yet received confirmation that it has been accepted by the order destination. This order status is not sent by TWS and should be explicitly set by the API developer when an order is submitted.</description>
    /// </item>
    /// <item>
    /// <term>PendingCancel</term>
    /// <description>Indicates that you have sent a request to cancel the order but have not yet received cancel confirmation from the order destination. At this point, your order is not confirmed canceled. You may still receive an execution while your cancellation request is pending. This order status is not sent by TWS and should be explicitly set by the API developer when an order is canceled.</description>
    /// </item>
    /// <item>
    /// <term>PreSubmitted</term>
    /// <description>Indicates that a simulated order type has been accepted by the IB system and that this order has yet to be elected. The order is held in the IB system (and the status remains DARK BLUE) until the election criteria are met. At that time the order is transmitted to the order destination as specified (and the order status color will change).</description>
    /// </item>
    /// <item>
    /// <term>Submitted</term>
    /// <description>Indicates that your order has been accepted at the order destination and is working.</description>
    /// </item>
    /// <item>
    /// <term>Cancelled</term>
    /// <description>Indicates that the balance of your order has been confirmed canceled by the IB system. This could occur unexpectedly when IB or the destination has rejected your order.</description>
    /// </item>
    /// <item>
    /// <term>Filled</term>
    /// <description>The order has been completely filled.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="OrderStatus"/>
    public string Status { get; internal set; }

    /// <summary>
    /// Specifies the number of shares that have been executed.
    /// </summary>
    public int Filled { get; internal set; }
    /// <summary>
    /// Specifies the number of shares still outstanding.
    /// </summary>
    public int Remaining { get; internal set; }
    /// <summary>
    /// The average price of the shares that have been executed.
    /// This parameter is valid only if the filled parameter value
    /// is greater than zero. Otherwise, the price parameter will be zero.
    /// </summary>
    public double AvgFillPrice { get; internal set; }
    /// <summary>
    /// The TWS id used to identify orders. Remains the same over TWS sessions.
    /// </summary>
    public int PermId { get; internal set; }
    /// <summary>
    /// The order ID of the parent order, used for bracket and auto trailing stop orders.
    /// </summary>
    public int ParentId { get; internal set; }
    /// <summary>
    /// The last price of the shares that have been executed. This parameter is valid
    /// only if the filled parameter value is greater than zero. Otherwise, the price parameter will be zero.
    /// </summary>
    public double LastFillPrice { get; internal set; }
    /// <summary>
    /// The Id of the client (or TWS) that placed the order.
    /// The TWS orders have a fixed clientId and orderId of 0 that distinguishes them from API orders.
    /// </summary>
    public int ClientId { get; internal set; }
    /// <summary>
    /// This field is used to identify an order held when TWS is trying to locate shares for a short sell.
    /// The value used to indicate this is 'locate'.
    /// </summary>
    /// <remarks>This field is supported starting with TWS release 872.</remarks>

    public string WhyHeld { get; internal set; }
  }

  public class TWSOpenOrderEventArgs : TWSClientEventArgs
  {
    public TWSOpenOrderEventArgs(TWSClient client) : base(client) {}

    /// <summary>
    /// Gets the order id.
    /// </summary>
    /// <value>
    /// The order id.
    /// </value>
    public int OrderId { get; internal set; }
    public IBOrder Order { get; internal set; }
    public IBContract Contract { get; internal set; }
  }

  public class TWSContractDetailsEventArgs : TWSClientEventArgs
  {
    public TWSContractDetailsEventArgs(TWSClient client) : base(client) {}

    public IBContractDetails ContractDetails { get; internal set; }

    public int RequestId { get; set; }
  }

  public class TWSUpdatePortfolioEventArgs : TWSClientEventArgs
  {
    public TWSUpdatePortfolioEventArgs(TWSClient client) : base(client) {}

    public IBContract Contract { get; internal set; }
    public int Position { get; internal set; }
    public double MarketPrice { get; internal set; }
    public double MarketValue { get; internal set; }
    public double AverageCost { get; internal set; }
    public double UnrealizedPnL { get; internal set; }
    public double RealizedPnL { get; internal set; }
    public string AccountName { get; internal set; }
  }

  public class TWSExecDetailsEventArgs : TWSClientEventArgs
  {
    public TWSExecDetailsEventArgs(TWSClient client) : base(client) {}

    public int OrderId { get; internal set; }
    public IBContract Contract { get; internal set; }
    public IBExecution Execution { get; internal set; }
  }

  public class TWSMarketDepthEventArgs : TWSClientEventArgs
  {
    public TWSMarketDepthEventArgs(TWSClient client) : base(client) {}

    public int RequestId { get; internal set; }
    public int Position { get; internal set; }
    public string MarketMaker { get; internal set; }
    public IBOperation Operation { get; internal set; }
    public IBSide Side { get; internal set; }
    public double Price { get; internal set; }
    public int Size { get; internal set; }
  }

  public class TWSHistoricalDataEventArgs : TWSClientEventArgs
  {
    public TWSHistoricalDataEventArgs(TWSClient client) : base(client) {}

    public int RequestId { get; internal set; }
    public TWSHistoricState State { get; internal set; }
    public DateTime Date { get; internal set; }
    public double Open { get; internal set; }
    public double Low { get; internal set; }
    public double High { get; internal set; }
    public double Close { get; internal set; }
    public int Volume { get; internal set; }
    public double WAP { get; internal set; }
    public bool HasGaps { get; internal set; }
  }

  public class TWSRealtimeBarEventArgs : TWSClientEventArgs
  {
    public TWSRealtimeBarEventArgs(TWSClient client) : base(client) {}
    public int RequestId { get; internal set; }
    public long Time { get; internal set; }
    public double Open { get; internal set; }
    public double High { get; internal set; }
    public double Low { get; internal set; }
    public double Close { get; internal set; }
    public long Volume { get; internal set; }
    public double Wap { get; internal set; }
    public int Count { get; internal set; }
  }
  public class TWSScannerDataEventArgs : TWSClientEventArgs
  {
    public TWSScannerDataEventArgs(TWSClient client) : base(client) {}
    public int RequestId { get; internal set; }
    public IBContractDetails Contract { get; internal set; }
    public int Rank { get; internal set; }
    public string Distance { get; internal set; }
    public string Benchmark { get; internal set; }
    public string Projection { get; internal set; }
  }

  public class TWSScannerParametersEventArgs : TWSClientEventArgs
  {
    public TWSScannerParametersEventArgs(TWSClient client) : base(client) { }
    public string Xml { get; internal set; }
  }


}