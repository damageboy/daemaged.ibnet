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
using System.ComponentModel;

namespace Daemaged.IBNet
{
  public class StringSerializableAttribute : Attribute { }
  public class StringSerializerAttribute : Attribute
  {
    public StringSerializerAttribute(string s)
    {
      Value = s;
    }

    public string Value { get; private set; }
  }

  /// <summary>
  /// Time frame for Volatility
  /// </summary>
  [Serializable]
  public enum IBVolatilityType
  {
    /// <summary>
    /// Undefined Volatility
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Daily Average Volatility
    /// </summary>
    Daily = 1,
    /// <summary>
    /// Annual Average Volatility
    /// </summary>
    Annual = 2
  }

  /// <summary>
  /// Used for the set server log level
  /// </summary>
  [Serializable]
  public enum LogLevel
  {
    /// <summary>
    /// Undefined Log Level
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// System Messages
    /// </summary>
    System = 1,
    /// <summary>
    /// Error Messages
    /// </summary>
    Error = 2,
    /// <summary>
    /// Warning Messages
    /// </summary>
    Warning = 3,
    /// <summary>
    /// Information Messages
    /// </summary>
    Information = 4,
    /// <summary>
    /// Detail Messages
    /// </summary>
    Detail = 5
  }

  [Serializable]
  public enum IBOrderOrigin
  {
    /// <summary>
    /// Order originated from the customer
    /// </summary>
    Customer = 0,
    /// <summary>
    /// Order originated from teh firm
    /// </summary>
    Firm = 1
  }

  /// <summary>
  /// Used for Rule 80A describes the type of trader.
  /// </summary>
  [StringSerializable]
  public enum IBAgentDescription
  {
    /// <summary>
    /// No Description Provided
    /// </summary>
    [StringSerializer("")]
    None,

    /// <summary>
    /// An individual
    /// </summary>
    [StringSerializer("I")]
    Individual,
    /// <summary>
    /// An Agency
    /// </summary>
    [StringSerializer("A")]
    Agency,
    /// <summary>
    /// An Agent or Other Member
    /// </summary>
    [StringSerializer("W")]
    AgentOtherMember,
    /// <summary>
    /// Individual PTIA
    /// </summary>
    [StringSerializer("J")]
    IndividualPTIA,
    /// <summary>
    /// Agency PTIA
    /// </summary>
    [StringSerializer("U")]
    AgencyPTIA,
    /// <summary>
    /// Agether or Other Member PTIA
    /// </summary>
    [StringSerializer("M")]
    AgentOtherMemberPTIA,
    /// <summary>
    /// Individual PT
    /// </summary>
    [StringSerializer("K")]
    IndividualPT,
    /// <summary>
    /// Agency PT
    /// </summary>
    [StringSerializer("Y")]
    AgencyPT,
    /// <summary>
    /// Agent Other Member PT
    /// </summary>
    [StringSerializer("N")]
    AgentOtherMemberPT,
  }

  /// <summary>
  /// Financial Advisor Allocation Method
  /// </summary>
  [StringSerializable]
  public enum IBFinancialAdvisorAllocationMethod
  {
    /// <summary>
    /// Percent Change
    /// </summary>
    [StringSerializer("PctChange")]
    PercentChange,
    /// <summary>
    /// Available Equity
    /// </summary>
    [StringSerializer("AvailableEquity")]
    AvailableEquity,
    /// <summary>
    /// Net Liquidity
    /// </summary>
    [StringSerializer("NetLiq")]
    NetLiquidity,
    /// <summary>
    /// Equal Quantity
    /// </summary>
    [StringSerializer("EqualQuantity")]
    EqualQuantity,
    /// <summary>
    /// No Allocation Method
    /// </summary>
    [StringSerializer("")]
    None
  }

  /// <summary>
  /// Describes wether a security was bought or sold in an execution.
  /// The past tense equivalent of ActionSide.
  /// </summary>
  [StringSerializable]
  public enum IBExecutionSide
  {
    /// <summary>
    /// Securities were bought.
    /// </summary>
    [StringSerializer("BOT")]
    Bought,
    /// <summary>
    /// Securities were sold.
    /// </summary>
    [StringSerializer("SLD")]
    Sold
  }


  [StringSerializable]
  public enum IBOrderStatus
  {
    /// <summary>
    /// indicates that you have transmitted the order, but have not yet received
    /// confirmation that it has been accepted by the order destination.
    /// This order status is not sent by TWS and should be explicitly set by the API developer when an order is submitted.
    /// </summary>
    [StringSerializer("PendingSubmit")]
    PendingSubmit,
    /// <summary>
    /// PendingCancel - indicates that you have sent a request to cancel the order
    /// but have not yet received cancel confirmation from the order destination.
    /// At this point, your order is not confirmed canceled. You may still receive
    /// an execution while your cancellation request is pending.
    /// This order status is not sent by TWS and should be explicitly set by the API developer when an order is canceled.
    /// </summary>
    [StringSerializer("PendingCancel")]
    PendingCancel,
    /// <summary>
    /// indicates that a simulated order type has been accepted by the IB system and
    /// that this order has yet to be elected. The order is held in the IB system
    /// (and the status remains DARK BLUE) until the election criteria are met.
    /// At that time the order is transmitted to the order destination as specified
    /// (and the order status color will change).
    /// </summary>
    [StringSerializer("PreSubmitted")]
    PreSubmitted,
    /// <summary>
    /// indicates that your order has been accepted at the order destination and is working.
    /// </summary>
    [StringSerializer("Submitted")]
    Submitted,
    /// <summary>
    /// indicates that the balance of your order has been confirmed canceled by the IB system.
    /// This could occur unexpectedly when IB or the destination has rejected your order.
    /// </summary>
    [StringSerializer("Cancelled")]
    Canceled,
    /// <summary>
    /// The order has been completely filled.
    /// </summary>
    [StringSerializer("Filled")]
    Filled,
    /// <summary>
    /// The Order is inactive
    /// </summary>
    [StringSerializer("Inactive")]
    Inactive,
    /// <summary>
    /// The order is Partially Filled
    /// </summary>
    [StringSerializer("PartiallyFilled")]
    PartiallyFilled,
    /// <summary>
    /// Api Pending
    /// </summary>
    [StringSerializer("ApiPending")]
    ApiPending,
    /// <summary>
    /// Api Cancelled
    /// </summary>
    [StringSerializer("ApiCancelled")]
    ApiCancelled,
    /// <summary>
    /// Indicates that there is an error with this order
    /// This order status is not sent by TWS and should be explicitly set by the API developer when an error has occured.
    /// </summary>
    [StringSerializer("Error")]
    Error,
    /// <summary>
    /// No Order Status
    /// </summary>
    [StringSerializer("")]
    None
  }

  /// <summary>
  /// Option Right Type (Put or Call)
  /// </summary>
  [StringSerializable]
  public enum IBRightType
  {    
    /// <summary>
    /// Option type is not defined (contract is not an option).
    /// </summary>
    [StringSerializer("")]
    Undefined,
    /// <summary>
    /// Option type is a Put (Right to sell)
    /// </summary>
    /// Description tag used to be "PUT"
    [StringSerializer("P")]
    Put,
    /// <summary>
    /// Option type is a Call (Right to buy)
    /// </summary>
    /// Description tag used to be "CALL"
    [StringSerializer("C")]
    Call,
  }


  /// <summary>
  /// Historical Bar Size Requests
  /// </summary>
  [StringSerializable]
  public enum IBSecurityIdType
  {
    /// <summary>
    /// No Security Id Type
    /// </summary>
    [StringSerializer("")]
    None,
    /// <summary>
    /// Example: Apple: US0378331005
    /// </summary>
    [StringSerializer("ISIN")]
    ISIN,
    /// <summary>
    /// Example: Apple: 037833100
    /// </summary>
    [StringSerializer("CUSIP")]
    CUSIP,
    /// <summary>
    /// Consists of 6-AN + check digit. Example: BAE: 0263494
    /// </summary>
    [StringSerializer("SEDOL")]
    SEDOL,
    /// <summary>
    /// Consists of exchange-independent RIC Root and a suffix identifying the exchange. Example: AAPL.O for Apple on NASDAQ.
    /// </summary>
    [StringSerializer("RIC")]
    RIC
  }


  // IMPORTANT: The numeric values here must stay synchronized with those in
  // IMarketDataProducer!
  [Serializable]
  public enum IBTickType
  {
    Unknown = -1,

    /// <summary>
    /// Bid Size
    /// </summary>
    BidSize = 0,

    /// <summary>
    /// Bid Price
    /// </summary>
    BidPrice = 1,

    /// <summary>
    /// Ask Price
    /// </summary>
    AskPrice = 2,

    /// <summary>
    /// Ask Size
    /// </summary>
    AskSize = 3,

    /// <summary>
    /// Last Price
    /// </summary>
    LastPrice = 4,

    /// <summary>
    /// Last Size
    /// </summary>
    LastSize = 5,

    /// <summary>
    /// High Price
    /// </summary>
    HighPrice = 6,

    /// <summary>
    /// Low Price
    /// </summary>
    LowPrice = 7,

    /// <summary>
    /// Volume
    /// </summary>
    Volume = 8,

    /// <summary>
    /// Close Price
    /// </summary>
    ClosePrice = 9,

    /// <summary>
    /// Bid Option
    /// </summary>
    BidOption = 10,

    /// <summary>
    /// Ask Option
    /// </summary>
    AskOption = 11,

    /// <summary>
    /// Last Option
    /// </summary>
    LastOption = 12,

    /// <summary>
    /// Model Option
    /// </summary>
    ModelOption = 13,

    /// <summary>
    /// Open Price
    /// </summary>
    OpenPrice = 14,

    /// <summary>
    /// Low Price over last 13 weeks
    /// </summary>
    Low13Week = 15,

    /// <summary>
    /// High Price over last 13 weeks
    /// </summary>
    High13Week = 16,

    /// <summary>
    /// Low Price over last 26 weeks
    /// </summary>
    Low26Week = 17,

    /// <summary>
    /// High Price over last 26 weeks
    /// </summary>
    High26Week = 18,

    /// <summary>
    /// Low Price over last 52 weeks
    /// </summary>
    Low52Week = 19,

    /// <summary>
    /// High Price over last 52 weeks
    /// </summary>
    High52Week = 20,

    /// <summary>
    /// Average Volume
    /// </summary>
    AverageVolume = 21,

    /// <summary>
    /// Open Interest
    /// </summary>
    OpenInterest = 22,

    /// <summary>
    /// Option Historical Volatility
    /// </summary>
    OptionHistoricalVolatility = 23,

    /// <summary>
    /// Option Implied Volatility
    /// </summary>
    OptionImpliedVolatility = 24,

    /// <summary>
    /// Option Bid Exchange
    /// </summary>
    OptionBidExchange = 25,

    /// <summary>
    /// Option Ask Exchange
    /// </summary>
    OptionAskExchange = 26,

    /// <summary>
    /// Option Call Open Interest
    /// </summary>
    OptionCallOpenInterest = 27,

    /// <summary>
    /// Option Put Open Interest
    /// </summary>
    OptionPutOpenInterest = 28,

    /// <summary>
    /// Option Call Volume
    /// </summary>
    OptionCallVolume = 29,

    /// <summary>
    /// Option Put Volume
    /// </summary>
    OptionPutVolume = 30,

    /// <summary>
    /// Index Future Premium
    /// </summary>
    IndexFuturePremium = 31,

    /// <summary>
    /// Bid Exchange
    /// </summary>
    BidExchange = 32,

    /// <summary>
    /// Ask Exchange
    /// </summary>
    AskExchange = 33,

    /// <summary>
    /// Auction Volume
    /// </summary>
    AuctionVolume = 34,

    /// <summary>
    /// Auction Price
    /// </summary>
    AuctionPrice = 35,

    /// <summary>
    /// Auction Imbalance
    /// </summary>
    AuctionImbalance = 36,

    /// <summary>
    /// Mark Price
    /// </summary>
    MarkPrice = 37,

    /// <summary>
    /// Bid EFP Computation
    /// </summary>
    BidEfpComputation = 38,

    /// <summary>
    /// Ask EFP Computation
    /// </summary>
    AskEfpComputation = 39,

    /// <summary>
    /// Last EFP Computation
    /// </summary>
    LastEfpComputation = 40,

    /// <summary>
    /// Open EFP Computation
    /// </summary>
    OpenEfpComputation = 41,

    /// <summary>
    /// High EFP Computation
    /// </summary>
    HighEfpComputation = 42,

    /// <summary>
    /// Low EFP Computation
    /// </summary>
    LowEfpComputation = 43,

    /// <summary>
    /// Close EFP Computation
    /// </summary>
    CloseEfpComputation = 44,

    /// <summary>
    /// Last Time Stamp
    /// </summary>
    LastTimestamp = 45,

    /// <summary>
    /// Shortable
    /// </summary>
    Shortable = 46,

    /// <summary>
    /// Fundamental Ratios
    /// </summary>
    FundamentalRatios = 47,

    /// <summary>
    /// Real Time Volume
    /// </summary>
    RealTimeVolume = 48,

    /// <summary>
    /// When trading is halted for a contract, TWS receives a special tick: haltedLast=1. When trading is resumed, TWS receives haltedLast=0. A new tick type, HALTED, tick ID = 49, is now available in regular market data via the API to indicate this halted state.
    /// Possible values for this new tick type are:
    /// 0 = Not halted 
    /// 1 = Halted. 
    ///  </summary>
    Halted = 49,

    /// <summary>
    /// Bond Yield for Bid Price
    /// </summary>
    BidYield = 50,

    /// <summary>
    /// Bond Yield for Ask Price
    /// </summary>
    AskYield = 51,

    /// <summary>
    /// Bond Yield for Last Price
    /// </summary>
    LastYield = 52,

    /// <summary>
    /// returns calculated implied volatility as a result of an CalculateImpliedVolatility( ) request.
    /// </summary>
    CustOptionComputation = 53,

    /// <summary>
    /// Trades
    /// </summary>
    TradeCount = 54,

    /// <summary>
    /// Trades per Minute
    /// </summary>
    TradeRate = 55,

    /// <summary>
    /// Volume per Minute
    /// </summary>
    VolumeRate = 56
  }

  [StringSerializable]
  public enum IBGenericTickType
  {
    /// <summary>
    /// Undefined Generic Tick Type
    /// </summary>
    [StringSerializer("")] Undefined = 0,

    /// <summary>
    /// Option Volume
    /// For stocks only.
    /// Returns TickType.OptionCallVolume and TickType.OptionPutVolume 
    /// </summary>
    [StringSerializer("OptionVolume")] OptionVolume = 100,

    /// <summary>
    /// Option Open Interest
    /// For stocks only.
    /// Returns TickType.OptionCallOpenInterest and TickType.OptionPutOpenInterest
    /// </summary>
    [StringSerializer("OptionOpenInterest")] OptionOpenInterest = 101,

    /// <summary>
    /// Historical Volatility
    /// For stocks only.
    /// Returns TickType.OptionHistoricalVol
    /// </summary>
    [StringSerializer("HistoricalVolatility")] HistoricalVolatility = 104,

    /// <summary>
    /// Option Implied Volatility
    /// For stocks only.
    /// Returns TickType.OptionImpliedVol
    /// </summary>
    [StringSerializer("OptionImpliedVolatility")] OptionImpliedVolatility = 106,

    /// <summary>
    /// Index Future Premium
    /// Returns TickType.IndexFuturePremium
    /// </summary>
    [StringSerializer("IndexFuturePremium")] IndexFuturePremium = 162,

    /// <summary>
    /// Miscellaneous Stats
    /// Returns TickType.Low13Week, TickType.High13Week, TickType.Low26Week, TickType.High26Week, TickType.Low52Week, TickType.High52Week and TickType.AverageVolume
    /// </summary>
    [StringSerializer("MiscellaneousStats")] MiscellaneousStats = 165,

    /// <summary>
    /// Mark Price
    /// Used in TWS P/L Computations
    /// Returns TickType.MarkPrice
    /// </summary>
    [StringSerializer("MarkPrice")] MarkPrice = 221,

    /// <summary>
    /// Auction Price
    /// Auction values (volume, price and imbalance)
    /// Returns TickType.AuctionVolume, TickType.AuctionPrice, TickType.AuctionImbalance
    /// </summary>
    [StringSerializer("AuctionPrice")] AuctionPrice = 225,

    /// <summary>
    /// Shortable Ticks
    /// </summary>
    [StringSerializer("Shortable")] Shortable = 236,

    /// <summary>
    /// Real Time Volume Tick Type
    /// </summary>
    [StringSerializer("RTVolume")] RealTimeVolume = 233,
  }

  [Serializable]
  public enum IBSide
  {
    ASK = 0,
    BID = 1,
  }

  [Serializable]
  public enum IBOperation
  {
    INSERT = 0,
    UPDATE = 1,
    DELETE = 2
  }

  [StringSerializable]
  public enum IBOrderType
  {
    /// <summary>
    /// A Market order is an order to buy or sell an asset at the bid or offer price currently available in the marketplace.
    /// Bonds, Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("MKT")] Market,

    /// <summary>
    /// A market order that is submitted to execute as close to the closing price as possible.
    /// Non US Futures, Non US Options, Stocks
    /// </summary>
    //Changed from MKTCLS to MOC based on input from TWS
    [StringSerializer("MOC")] MarketOnClose,

    /// <summary>
    /// A limit order is an order to buy or sell a contract at a specified price or better.
    /// Bonds, Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("LMT")] Limit,

    /// <summary>
    /// An LOC (Limit-on-Close) order that executes at the closing price if the closing price is at or better than the submitted limit price, according to the rules of the specific exchange. Otherwise the order will be cancelled. 
    /// Non US Futures , Stocks
    /// </summary>
    [StringSerializer("LMTCLS")] LimitOnClose,

    /// <summary>
    /// An order that is pegged to buy on the best offer and sell on the best bid.
    /// Your order is pegged to buy on the best offer and sell on the best bid. You can also use an offset amount which is subtracted from the best offer for a buy order, and added to the best bid for a sell order.
    /// Stocks
    /// </summary>
    [StringSerializer("PEGMKT")] PeggedToMarket,

    /// <summary>
    /// A Stop order becomes a market order to buy or sell securities or commodities once the specified stop price is attained or penetrated.
    /// Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("STP")] Stop,

    /// <summary>
    /// A STOP-LIMIT order is similar to a stop order in that a stop price will activate the order. However, once activated, the stop-limit order becomes a buy limit or sell limit order and can only be executed at a specific price or better. It is a combination of both the stop order and the limit order.
    /// Forex, Futures, Options, Stocks
    /// </summary>
    [StringSerializer("STP LMT")] StopLimit,

    /// <summary>
    /// A trailing stop for a sell order sets the stop price at a fixed amount below the market price. If the market price rises, the stop loss price rises by the increased amount, but if the stock price falls, the stop loss price remains the same. The reverse is true for a buy trailing stop order.
    /// Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("TRAIL")] TrailingStop,

    /// <summary>
    /// A Relative order derives its price from a combination of the market quote and a user-defined offset amount. The order is submitted as a limit order and modified according to the pricing logic until it is executed or you cancel the order.
    /// Options, Stocks
    /// </summary>
    [StringSerializer("REL")] Relative,

    /// <summary>
    /// The VWAP for a stock is calculated by adding the dollars traded for every transaction in that stock ("price" x "number of shares traded") and dividing the total shares traded. By default, a VWAP order is computed from the open of the market to the market close, and is calculated by volume weighting all transactions during this time period. TWS allows you to modify the cut-off and expiration times using the Time in Force and Expiration Date fields, respectively.
    /// Stocks
    /// </summary>
    [StringSerializer("VWAP")] VolumeWeightedAveragePrice,

    /// <summary>
    /// A trailing stop limit for a sell order sets the stop price at a fixed amount below the market price and defines a limit price for the sell order. If the market price rises, the stop loss price rises by the increased amount, but if the stock price falls, the stop loss price remains the same. When the order triggers, a limit order is submitted at the price you defined. The reverse is true for a buy trailing stop limit order.
    /// Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("TRAILLIMIT")] TrailingStopLimit,

    /// <summary>
    /// TWS Version 857 introduced volatility trading of options, and a new order type, "VOL." What happens with VOL orders is that the limit price that is sent to the exchange is computed by TWS as a function of a daily or annualized option volatility provided by the user. VOL orders can be placed for any US option that trades on the BOX exchange. VOL orders are eligible for dynamic management, a powerful new functionality wherein TWS can manage options orders in response to specifications set by the user.
    /// </summary>
    [StringSerializer("VOL")] Volatility,

    /// <summary>
    /// VOL orders only. Enter an order type to instruct TWS to submit a
    /// delta neutral trade on full or partial execution of the VOL order.
    /// For no hedge delta order to be sent, specify NONE.
    /// </summary>
    [StringSerializer("None")] None,

    /// <summary>
    /// Used to initialize the delta Order Field.
    /// </summary>
    [StringSerializer("")] Empty,

    /// <summary>
    /// Default - used for Delta Neutral Order Type
    /// </summary>
    [StringSerializer("Default")] Default,

    /// <summary>
    /// Scale Order.
    /// </summary>
    [StringSerializer("SCALE")] Scale,

    /// <summary>
    /// Market if Touched Order.
    /// </summary>
    [StringSerializer("MIT")] MarketIfTouched,

    /// <summary>
    /// Limit if Touched Order.
    /// </summary>
    [StringSerializer("LIT")] LimitIfTouched
  }

  // IMPORTANT: The values here must stay synchronized with those in
  // IMarketDataProducer!
  [StringSerializable]
  public enum IBSecurityType
  {
    /// <summary>
    /// Undefined Security Type
    /// </summary>
    [StringSerializer("")] Undefined,
    /// <summary>
    /// Stock
    /// </summary>
    [StringSerializer("STK")] Stock,

    /// <summary>
    /// Option
    /// </summary>
    [StringSerializer("OPT")] Option,

    /// <summary>
    /// Future
    /// </summary>
    [StringSerializer("FUT")] Future,

    /// <summary>
    /// Index
    /// </summary>
    [StringSerializer("IND")] Index,

    /// <summary>
    /// FOP = options on futures
    /// </summary>
    [StringSerializer("FOP")] FutureOption,

    /// <summary>
    /// Cash
    /// </summary>
    [StringSerializer("CASH")] Cash,

    /// <summary>
    /// For Combination Orders - must use combo leg details
    /// </summary>
    [StringSerializer("BAG")] Bag,

    /// <summary>
    /// Bond
    /// </summary>
    [StringSerializer("BOND")] Bond,

    /// <summary>
    /// Warrant
    /// </summary>
    [StringSerializer("WAR")] Warrant,
  }

  [StringSerializable]
  public enum IBAction
  {
    /// <summary>
    /// Security is to be bought.
    /// </summary>
    [StringSerializer("BUY")] Buy,

    /// <summary>
    /// Security is to be sold.
    /// </summary>
    [StringSerializer("SELL")] Sell,

    /// <summary>
    /// Undefined
    /// </summary>
    [StringSerializer("")] Undefined,

    /// <summary>
    /// Sell Short as part of a combo leg
    /// </summary>
    [StringSerializer("SSHORT")] SShort,

    /// <summary>
    /// Short Sale Exempt action.
    /// SSHORTX allows some orders to be marked as exempt from the new SEC Rule 201
    /// </summary>
    [StringSerializer("SSHORTX")] SShortX
  }


  [StringSerializable]
  public enum IBTimeInForce
  {
    /// <summary>
    /// Day
    /// </summary>
    [StringSerializer("DAY")] Day,

    /// <summary>
    /// Good Till Cancel
    /// </summary>
    [StringSerializer("GTC")] GoodTillCancel,

    /// <summary>
    /// You can set the time in force for MARKET or LIMIT orders as IOC. This dictates that any portion of the order not executed immediately after it becomes available on the market will be cancelled.
    /// </summary>
    [StringSerializer("IOC")] ImmediateOrCancel,

    /// <summary>
    /// Setting FOK as the time in force dictates that the entire order must execute immediately or be canceled.
    /// </summary>
    [StringSerializer("FOK")] FillOrKill,

    /// <summary>
    /// Good Till Date
    /// </summary>
    [StringSerializer("GTD")] GoodTillDate,

    /// <summary>
    /// Market On Open
    /// </summary>
    [StringSerializer("OPG")] MarketOnOpen,

    /// <summary>
    /// Undefined
    /// </summary>
    [StringSerializer("")] Undefined
  }

  /// <summary>
  /// OCA Type Options
  /// </summary>
  [Serializable]
  public enum IBOcaType
  {
    /// <summary>
    /// Undefined Oca Type
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// 1 = Cancel all remaining orders with block
    /// </summary>
    CancelAll = 1,

    /// <summary>
    /// 2 = Remaining orders are proportionately reduced in size with block
    /// </summary>
    ReduceWithBlock = 2,

    /// <summary>
    /// 3 = Remaining orders are proportionately reduced in size with no block
    /// </summary>
    ReduceWithNoBlock = 3
  }


  internal enum IBPlaybackMessage : uint
  {
    Send = 0xDEADBEAF,
    Receive = 0x12345678,
  }


  public enum ClientMessage
  {
    /// <summary>
    /// Undefined Incoming Message
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Error
    /// </summary>
    Error = -1,

    /// <summary>
    /// Tick Price
    /// </summary>
    TickPrice = 1,

    /// <summary>
    /// Tick Size
    /// </summary>
    TickSize = 2,

    /// <summary>
    /// Order Status
    /// </summary>
    OrderStatus = 3,

    /// <summary>
    /// Error Message
    /// </summary>
    ErrorMessage = 4,

    /// <summary>
    /// Open Order
    /// </summary>
    OpenOrder = 5,

    /// <summary>
    /// Account Value
    /// </summary>
    AccountValue = 6,

    /// <summary>
    /// Portfolio Value
    /// </summary>
    PortfolioValue = 7,

    /// <summary>
    /// Account Update Time
    /// </summary>
    AccountUpdateTime = 8,

    /// <summary>
    /// Next Valid ID
    /// </summary>
    NextValidId = 9,

    /// <summary>
    /// Contract Data
    /// </summary>
    ContractData = 10,

    /// <summary>
    /// Execution Data
    /// </summary>
    ExecutionData = 11,

    /// <summary>
    /// Market Depth
    /// </summary>
    MarketDepth = 12,

    /// <summary>
    /// Market Depth L2
    /// </summary>
    MarketDepthL2 = 13,

    /// <summary>
    /// News Bulletins
    /// </summary>
    NewsBulletins = 14,

    /// <summary>
    /// Managed Accounts
    /// </summary>
    ManagedAccounts = 15,

    /// <summary>
    /// Receive Financial Advice
    /// </summary>
    ReceiveFA = 16,

    /// <summary>
    /// Historical Data
    /// </summary>
    HistoricalData = 17,

    /// <summary>
    /// Bond Contract Data
    /// </summary>
    BondContractData = 18,

    /// <summary>
    /// Scanner Parameters
    /// </summary>
    ScannerParameters = 19,

    /// <summary>
    /// Scanner Data
    /// </summary>
    ScannerData = 20,

    /// <summary>
    /// Tick Option Computation
    /// </summary>
    TickOptionComputation = 21,

    /// <summary>
    /// Tick Generic
    /// </summary>
    TickGeneric = 45,

    /// <summary>
    /// Tick String
    /// </summary>
    TickString = 46,

    /// <summary>
    /// Tick Exchange for Physical(EFP)
    /// </summary>
    TickEfp = 47,

    /// <summary>
    /// Current Time
    /// </summary>
    CurrentTime = 49,

    /// <summary>
    /// Real Time Bars
    /// </summary>
    RealTimeBars = 50,

    /// <summary>
    /// Fundamental Data
    /// </summary>
    FundamentalData = 51,

    /// <summary>
    /// Contract Data End
    /// </summary>
    ContractDataEnd = 52,

    /// <summary>
    /// Received after the last open order message
    /// </summary>
    OpenOrderEnd = 53,

    /// <summary>
    /// Received after the last account download message
    /// </summary>
    AccountDownloadEnd = 54,

    /// <summary>
    /// Received after a complete list of executions
    /// </summary>
    ExecutionDataEnd = 55,

    /// <summary>
    /// Received after a delta neutral validation
    /// </summary>
    DeltaNeutralValidation = 56,

    /// <summary>
    /// End of Tick Snapshot message
    /// </summary>
    TickSnapshotEnd = 57,

    /// <summary>
    /// Market Data Type Message
    /// </summary>
    MarketDataType = 58,

    /// <summary>
    /// Commision Report Message
    /// </summary>
    CommissionReport = 59,
  }


  public enum ServerMessage
  {
    /// <summary>
    /// Undefined Outgoing Message
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Request Market Data
    /// </summary>
    RequestMarketData = 1,

    /// <summary>
    /// Cancel Market Data
    /// </summary>
    CancelMarketData = 2,

    /// <summary>
    /// Place Order
    /// </summary>
    PlaceOrder = 3,

    /// <summary>
    /// Cancel Order
    /// </summary>
    CancelOrder = 4,

    /// <summary>
    /// Request Open Orders
    /// </summary>
    RequestOpenOrders = 5,

    /// <summary>
    /// Request Account Data
    /// </summary>
    RequestAccountData = 6,

    /// <summary>
    /// Request Executions
    /// </summary>
    RequestExecutions = 7,

    /// <summary>
    /// Request IDS
    /// </summary>
    RequestIds = 8,

    /// <summary>
    /// Request Contract Data
    /// </summary>
    RequestContractData = 9,

    /// <summary>
    /// Request Market Depth
    /// </summary>
    RequestMarketDepth = 10,

    /// <summary>
    /// Cancel Market Depth
    /// </summary>
    CancelMarketDepth = 11,

    /// <summary>
    /// Request News Bulletins
    /// </summary>
    RequestNewsBulletins = 12,

    /// <summary>
    /// Cancel News Bulletins
    /// </summary>
    CancelNewsBulletins = 13,

    /// <summary>
    /// Set Server Log Level
    /// </summary>
    SetServerLogLevel = 14,

    /// <summary>
    /// Request Auto Open Orders
    /// </summary>
    RequestAutoOpenOrders = 15,

    /// <summary>
    /// Request All Open Orders
    /// </summary>
    RequestAllOpenOrders = 16,

    /// <summary>
    /// Request Managed Accounts
    /// </summary>
    RequestManagedAccounts = 17,

    /// <summary>
    /// Request Financial Advisor
    /// </summary>
    RequestFA = 18,

    /// <summary>
    /// Replace Financial Advisor
    /// </summary>
    ReplaceFA = 19,

    /// <summary>
    /// Request Historical Data
    /// </summary>
    RequestHistoricalData = 20,

    /// <summary>
    /// Exercise Options
    /// </summary>
    ExerciseOptions = 21,

    /// <summary>
    /// Request Scanner Subscription
    /// </summary>
    RequestScannerSubscription = 22,

    /// <summary>
    /// Cancel Scanner Subscription
    /// </summary>
    CancelScannerSubscription = 23,

    /// <summary>
    /// Request Scanner Parameters
    /// </summary>
    RequestScannerParameters = 24,

    /// <summary>
    /// Cancel Historical Data
    /// </summary>
    CancelHistoricalData = 25,

    /// <summary>
    /// Request Current Time
    /// </summary>
    RequestCurrentTime = 49,

    /// <summary>
    /// Request Real Time Bars
    /// </summary>
    RequestRealTimeBars = 50,

    /// <summary>
    /// Cancel Real Time Bars
    /// </summary>
    CancelRealTimeBars = 51,

    /// <summary>
    /// Request Fundamental Data
    /// </summary>
    RequestFundamentalData = 52,

    /// <summary>
    /// Cancel Fundamental Data
    /// </summary>
    CancelFundamentalData = 53,

    /// <summary>
    /// Request Calculated Implied Volatility
    /// </summary>
    RequestCalcImpliedVolatility = 54,

    /// <summary>
    /// Request Calculated Option Prices
    /// </summary>
    RequestCalcOptionPrice = 55,

    /// <summary>
    /// Cancel Calculated Implied Volatility
    /// </summary>
    CancelCalcImpliedVolatility = 56,

    /// <summary>
    /// Cancel Calculated Option Prices
    /// </summary>
    CancelCalcOptionPrice = 57,

    /// <summary>
    /// Globally Cancel All Orders
    /// </summary>
    RequestGlobalCancel = 58,

    /// <summary>
    /// Request market data type
    /// </summary>
    RequestMarketDataType = 59,

  }

  public enum TWSHistoricState
  {
    Starting,
    Downloading,
    Finished
  }

  public enum IBMarketDataType
  { 
    RealTime   = 1,
    Frozer     = 2,
  }

  [Serializable]
  public enum IBShortSaleSlot
  {
    /// <summary>
    /// e.g. retail customer or not SSHORT leg
    /// </summary>
    Unapplicable = 0,
    /// <summary>
    /// Clearing Broker
    /// </summary>
    ClearingBroker = 1,
    /// <summary>
    /// Third Party
    /// </summary>
    ThirdParty = 2
  }

  /// <summary>
  /// Retail Customers are restricted to "SAME"
  /// Institutional Customers may use "SAME", "OPEN", "CLOSE", "UNKNOWN"
  /// </summary>
  [Serializable]
  public enum IBComboOpenClose
  {
    /// <summary>
    /// open/close leg value is same as combo
    /// This value is always used for retail accounts
    /// </summary>    
    Same = 0,
    /// <summary>
    /// Institutional Accounts Only
    /// </summary>    
    Open = 1,
    /// <summary>
    /// Institutional Accounts Only
    /// </summary>
    Close = 2,
    /// <summary>
    /// Institutional Accounts Only
    /// </summary>
    Unknown = 3
  }



}