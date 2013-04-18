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
  public class StringSerializerAttribute : Attribute
  {
    public StringSerializerAttribute(string s)
    {
      Value = s;
    }

    public string Value { get; private set; }
  }

  // IMPORTANT: The numeric values here must stay synchronized with those in
  // IMarketDataProducer!
  [Serializable]
  public enum IBTickType
  {
    [StringSerializer("Error")]
    Unknown = -1,
    /// <summary>
    /// Bid Size
    /// </summary>
    [StringSerializer("BID_SIZE")]
    BidSize = 0,
    /// <summary>
    /// Bid Price
    /// </summary>
    [StringSerializer("BID")]
    BidPrice = 1,
    /// <summary>
    /// Ask Price
    /// </summary>
    [StringSerializer("ASK")]
    AskPrice = 2,
    /// <summary>
    /// Ask Size
    /// </summary>
    [StringSerializer("ASK_SIZE")]
    AskSize = 3,
    /// <summary>
    /// Last Price
    /// </summary>
    [StringSerializer("LAST")]
    LastPrice = 4,
    /// <summary>
    /// Last Size
    /// </summary>
    [StringSerializer("LAST_SIZE")]
    LastSize = 5,
    /// <summary>
    /// High Price
    /// </summary>
    [StringSerializer("HIGH")]
    HighPrice = 6,
    /// <summary>
    /// Low Price
    /// </summary>
    [StringSerializer("LOW")]
    LowPrice = 7,
    /// <summary>
    /// Volume
    /// </summary>
    [StringSerializer("VOLUME")]
    Volume = 8,
    /// <summary>
    /// Close Price
    /// </summary>
    [StringSerializer("CLOSE")]
    ClosePrice = 9,
    /// <summary>
    /// Bid Option
    /// </summary>
    [StringSerializer("BID_OPTION")]
    BidOption = 10,
    /// <summary>
    /// Ask Option
    /// </summary>
    [StringSerializer("ASK_OPTION")]
    AskOption = 11,
    /// <summary>
    /// Last Option
    /// </summary>
    [StringSerializer("LAST_OPTION")]
    LastOption = 12,
    /// <summary>
    /// Model Option
    /// </summary>
    [StringSerializer("MODEL_OPTION")]
    ModelOption = 13,
    /// <summary>
    /// Open Price
    /// </summary>
    [StringSerializer("OPEN")]
    OpenPrice = 14,
    /// <summary>
    /// Low Price over last 13 weeks
    /// </summary>
    [StringSerializer("LOW_13_WEEK")]
    Low13Week = 15,
    /// <summary>
    /// High Price over last 13 weeks
    /// </summary>
    [StringSerializer("HIGH_13_WEEK")]
    High13Week = 16,
    /// <summary>
    /// Low Price over last 26 weeks
    /// </summary>
    [StringSerializer("LOW_26_WEEK")]
    Low26Week = 17,
    /// <summary>
    /// High Price over last 26 weeks
    /// </summary>
    [StringSerializer("HIGH_26_WEEK")]
    High26Week = 18,
    /// <summary>
    /// Low Price over last 52 weeks
    /// </summary>
    [StringSerializer("LOW_52_WEEK")]
    Low52Week = 19,
    /// <summary>
    /// High Price over last 52 weeks
    /// </summary>
    [StringSerializer("HIGH_52_WEEK")]
    High52Week = 20,
    /// <summary>
    /// Average Volume
    /// </summary>
    [StringSerializer("AVG_VOLUME")]
    AverageVolume = 21,
    /// <summary>
    /// Open Interest
    /// </summary>
    [StringSerializer("OPEN_INTEREST")]
    OpenInterest = 22,
    /// <summary>
    /// Option Historical Volatility
    /// </summary>
    [StringSerializer("OPTION_HISTORICAL_VOL")]
    OptionHistoricalVolatility = 23,
    /// <summary>
    /// Option Implied Volatility
    /// </summary>
    [StringSerializer("OPTION_IMPLIED_VOL")]
    OptionImpliedVolatility = 24,
    /// <summary>
    /// Option Bid Exchange
    /// </summary>
    [StringSerializer("OPTION_BID_EXCH")]
    OptionBidExchange = 25,
    /// <summary>
    /// Option Ask Exchange
    /// </summary>
    [StringSerializer("OPTION_ASK_EXCH")]
    OptionAskExchange = 26,
    /// <summary>
    /// Option Call Open Interest
    /// </summary>
    [StringSerializer("OPTION_CALL_OPEN_INTEREST")]
    OptionCallOpenInterest = 27,
    /// <summary>
    /// Option Put Open Interest
    /// </summary>
    [StringSerializer("OPTION_PUT_OPEN_INTEREST")]
    OptionPutOpenInterest = 28,
    /// <summary>
    /// Option Call Volume
    /// </summary>
    [StringSerializer("OPTION_CALL_VOLUME")]
    OptionCallVolume = 29,
    /// <summary>
    /// Option Put Volume
    /// </summary>
    [StringSerializer("OPTION_PUT_VOLUME")]
    OptionPutVolume = 30,
    /// <summary>
    /// Index Future Premium
    /// </summary>
    [StringSerializer("INDEX_FUTURE_PREMIUM")]
    IndexFuturePremium = 31,
    /// <summary>
    /// Bid Exchange
    /// </summary>
    [StringSerializer("BID_EXCH")]
    BidExchange = 32,
    /// <summary>
    /// Ask Exchange
    /// </summary>
    [StringSerializer("ASK_EXCH")]
    AskExchange = 33,
    /// <summary>
    /// Auction Volume
    /// </summary>
    [StringSerializer("AUCTION_VOLUME")]
    AuctionVolume = 34,
    /// <summary>
    /// Auction Price
    /// </summary>
    [StringSerializer("AUCTION_PRICE")]
    AuctionPrice = 35,
    /// <summary>
    /// Auction Imbalance
    /// </summary>
    [StringSerializer("AUCTION_IMBALANCE")]
    AuctionImbalance = 36,
    /// <summary>
    /// Mark Price
    /// </summary>
    [StringSerializer("MARK_PRICE")]
    MarkPrice = 37,
    /// <summary>
    /// Bid EFP Computation
    /// </summary>
    [StringSerializer("BID_EFP_COMPUTATION")]
    BidEfpComputation = 38,
    /// <summary>
    /// Ask EFP Computation
    /// </summary>
    [StringSerializer("ASK_EFP_COMPUTATION")]
    AskEfpComputation = 39,
    /// <summary>
    /// Last EFP Computation
    /// </summary>
    [StringSerializer("LAST_EFP_COMPUTATION")]
    LastEfpComputation = 40,
    /// <summary>
    /// Open EFP Computation
    /// </summary>
    [StringSerializer("OPEN_EFP_COMPUTATION")]
    OpenEfpComputation = 41,
    /// <summary>
    /// High EFP Computation
    /// </summary>
    [StringSerializer("HIGH_EFP_COMPUTATION")]
    HighEfpComputation = 42,
    /// <summary>
    /// Low EFP Computation
    /// </summary>
    [StringSerializer("LOW_EFP_COMPUTATION")]
    LowEfpComputation = 43,
    /// <summary>
    /// Close EFP Computation
    /// </summary>
    [StringSerializer("CLOSE_EFP_COMPUTATION")]
    CloseEfpComputation = 44,
    /// <summary>
    /// Last Time Stamp
    /// </summary>
    [StringSerializer("LAST_TIMESTAMP")]
    LastTimestamp = 45,
    /// <summary>
    /// Shortable
    /// </summary>
    [StringSerializer("SHORTABLE")]
    Shortable = 46,
    /// <summary>
    /// Fundamental Ratios
    /// </summary>
    [StringSerializer("FUNDAMENTAL_RATIOS")]
    FundamentalRatios = 47,
    /// <summary>
    /// Real Time Volume
    /// </summary>
    [StringSerializer("RTVOLUME")]
    RealTimeVolume = 48,
    /// <summary>
    /// When trading is halted for a contract, TWS receives a special tick: haltedLast=1. When trading is resumed, TWS receives haltedLast=0. A new tick type, HALTED, tick ID = 49, is now available in regular market data via the API to indicate this halted state.
    /// Possible values for this new tick type are:
    /// 0 = Not halted 
    /// 1 = Halted. 
    ///  </summary>
    [StringSerializer("HALTED")]
    Halted = 49,
    /// <summary>
    /// Bond Yield for Bid Price
    /// </summary>
    [StringSerializer("BID_YIELD")]
    BidYield = 50,
    /// <summary>
    /// Bond Yield for Ask Price
    /// </summary>
    [StringSerializer("ASK_YIELD")]
    AskYield = 51,
    /// <summary>
    /// Bond Yield for Last Price
    /// </summary>
    [StringSerializer("LAST_YIELD")]
    LastYield = 52,
    /// <summary>
    /// returns calculated implied volatility as a result of an calculateImpliedVolatility( ) request.
    /// </summary>
    [StringSerializer("CUST_OPTION_COMPUTATION")]
    CustOptionComputation = 53,
    /// <summary>
    /// Trades
    /// </summary>
    [StringSerializer("TRADE_COUNT")]
    TradeCount = 54,
    /// <summary>
    /// Trades per Minute
    /// </summary>
    [StringSerializer("TRADE_RATE")]
    TradeRate = 55,
    /// <summary>
    /// Volume per Minute
    /// </summary>
    [StringSerializer("VOLUME_RATE")]
    VolumeRate = 56
  }

  [Serializable]
  public enum IBGenericTickType
  {
    /// <summary>
    /// Undefined Generic Tick Type
    /// </summary>
    [StringSerializer("")]
    Undefined = 0,
    /// <summary>
    /// Option Volume
    /// For stocks only.
    /// Returns TickType.OptionCallVolume and TickType.OptionPutVolume 
    /// </summary>
    [StringSerializer("OptionVolume")]
    OptionVolume = 100,
    /// <summary>
    /// Option Open Interest
    /// For stocks only.
    /// Returns TickType.OptionCallOpenInterest and TickType.OptionPutOpenInterest
    /// </summary>
    [StringSerializer("OptionOpenInterest")]
    OptionOpenInterest = 101,
    /// <summary>
    /// Historical Volatility
    /// For stocks only.
    /// Returns TickType.OptionHistoricalVol
    /// </summary>
    [StringSerializer("HistoricalVolatility")]
    HistoricalVolatility = 104,
    /// <summary>
    /// Option Implied Volatility
    /// For stocks only.
    /// Returns TickType.OptionImpliedVol
    /// </summary>
    [StringSerializer("OptionImpliedVolatility")]
    OptionImpliedVolatility = 106,
    /// <summary>
    /// Index Future Premium
    /// Returns TickType.IndexFuturePremium
    /// </summary>
    [StringSerializer("IndexFuturePremium")]
    IndexFuturePremium = 162,
    /// <summary>
    /// Miscellaneous Stats
    /// Returns TickType.Low13Week, TickType.High13Week, TickType.Low26Week, TickType.High26Week, TickType.Low52Week, TickType.High52Week and TickType.AverageVolume
    /// </summary>
    [StringSerializer("MiscellaneousStats")]
    MiscellaneousStats = 165,
    /// <summary>
    /// Mark Price
    /// Used in TWS P/L Computations
    /// Returns TickType.MarkPrice
    /// </summary>
    [StringSerializer("MarkPrice")]
    MarkPrice = 221,
    /// <summary>
    /// Auction Price
    /// Auction values (volume, price and imbalance)
    /// Returns TickType.AuctionVolume, TickType.AuctionPrice, TickType.AuctionImbalance
    /// </summary>
    [StringSerializer("AuctionPrice")]
    AuctionPrice = 225,
    /// <summary>
    /// Shortable Ticks
    /// </summary>
    [StringSerializer("Shortable")]
    Shortable = 236,
    /// <summary>
    /// Real Time Volume Tick Type
    /// </summary>
    [StringSerializer("RTVolume")]
    RealTimeVolume = 233,
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

  [Serializable]
  public enum IBOrderType
  {
    /// <summary>
    /// A Market order is an order to buy or sell an asset at the bid or offer price currently available in the marketplace.
    /// Bonds, Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("MKT")]
    Market,
    /// <summary>
    /// A market order that is submitted to execute as close to the closing price as possible.
    /// Non US Futures, Non US Options, Stocks
    /// </summary>
    //Changed from MKTCLS to MOC based on input from TWS
    [StringSerializer("MOC")]
    MarketOnClose,
    /// <summary>
    /// A limit order is an order to buy or sell a contract at a specified price or better.
    /// Bonds, Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("LMT")]
    Limit,
    /// <summary>
    /// An LOC (Limit-on-Close) order that executes at the closing price if the closing price is at or better than the submitted limit price, according to the rules of the specific exchange. Otherwise the order will be cancelled. 
    /// Non US Futures , Stocks
    /// </summary>
    [StringSerializer("LMTCLS")]
    LimitOnClose,
    /// <summary>
    /// An order that is pegged to buy on the best offer and sell on the best bid.
    /// Your order is pegged to buy on the best offer and sell on the best bid. You can also use an offset amount which is subtracted from the best offer for a buy order, and added to the best bid for a sell order.
    /// Stocks
    /// </summary>
    [StringSerializer("PEGMKT")]
    PeggedToMarket,
    /// <summary>
    /// A Stop order becomes a market order to buy or sell securities or commodities once the specified stop price is attained or penetrated.
    /// Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("STP")]
    Stop,
    /// <summary>
    /// A STOP-LIMIT order is similar to a stop order in that a stop price will activate the order. However, once activated, the stop-limit order becomes a buy limit or sell limit order and can only be executed at a specific price or better. It is a combination of both the stop order and the limit order.
    /// Forex, Futures, Options, Stocks
    /// </summary>
    [StringSerializer("STP LMT")]
    StopLimit,
    /// <summary>
    /// A trailing stop for a sell order sets the stop price at a fixed amount below the market price. If the market price rises, the stop loss price rises by the increased amount, but if the stock price falls, the stop loss price remains the same. The reverse is true for a buy trailing stop order.
    /// Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("TRAIL")]
    TrailingStop,
    /// <summary>
    /// A Relative order derives its price from a combination of the market quote and a user-defined offset amount. The order is submitted as a limit order and modified according to the pricing logic until it is executed or you cancel the order.
    /// Options, Stocks
    /// </summary>
    [StringSerializer("REL")]
    Relative,
    /// <summary>
    /// The VWAP for a stock is calculated by adding the dollars traded for every transaction in that stock ("price" x "number of shares traded") and dividing the total shares traded. By default, a VWAP order is computed from the open of the market to the market close, and is calculated by volume weighting all transactions during this time period. TWS allows you to modify the cut-off and expiration times using the Time in Force and Expiration Date fields, respectively.
    /// Stocks
    /// </summary>
    [StringSerializer("VWAP")]
    VolumeWeightedAveragePrice,
    /// <summary>
    /// A trailing stop limit for a sell order sets the stop price at a fixed amount below the market price and defines a limit price for the sell order. If the market price rises, the stop loss price rises by the increased amount, but if the stock price falls, the stop loss price remains the same. When the order triggers, a limit order is submitted at the price you defined. The reverse is true for a buy trailing stop limit order.
    /// Forex, Futures, Future Options, Options, Stocks, Warrants
    /// </summary>
    [StringSerializer("TRAILLIMIT")]
    TrailingStopLimit,
    /// <summary>
    /// TWS Version 857 introduced volatility trading of options, and a new order type, "VOL." What happens with VOL orders is that the limit price that is sent to the exchange is computed by TWS as a function of a daily or annualized option volatility provided by the user. VOL orders can be placed for any US option that trades on the BOX exchange. VOL orders are eligible for dynamic management, a powerful new functionality wherein TWS can manage options orders in response to specifications set by the user.
    /// </summary>
    [StringSerializer("VOL")]
    Volatility,
    /// <summary>
    /// VOL orders only. Enter an order type to instruct TWS to submit a
    /// delta neutral trade on full or partial execution of the VOL order.
    /// For no hedge delta order to be sent, specify NONE.
    /// </summary>
    [StringSerializer("NONE")]
    None,
    /// <summary>
    /// Used to initialize the delta Order Field.
    /// </summary>
    [StringSerializer("")]
    Empty,
    /// <summary>
    /// Default - used for Delta Neutral Order Type
    /// </summary>
    [StringSerializer("Default")]
    Default,
    /// <summary>
    /// Scale Order.
    /// </summary>
    [StringSerializer("SCALE")]
    Scale,
    /// <summary>
    /// Market if Touched Order.
    /// </summary>
    [StringSerializer("MIT")]
    MarketIfTouched,
    /// <summary>
    /// Limit if Touched Order.
    /// </summary>
    [StringSerializer("LIT")]
    LimitIfTouched
  }

  // IMPORTANT: The values here must stay synchronized with those in
  // IMarketDataProducer!
  [Serializable]
  public enum IBSecurityType
  {
    /// <summary>
    /// Stock
    /// </summary>
    [StringSerializer("STK")]
    Stock,
    /// <summary>
    /// Option
    /// </summary>
    [StringSerializer("OPT")]
    Option,
    /// <summary>
    /// Future
    /// </summary>
    [StringSerializer("FUT")]
    Future,
    /// <summary>
    /// Indice
    /// </summary>
    [StringSerializer("IND")]
    Index,
    /// <summary>
    /// FOP = options on futures
    /// </summary>
    [StringSerializer("FOP")]
    FutureOption,
    /// <summary>
    /// Cash
    /// </summary>
    [StringSerializer("CASH")]
    Cash,
    /// <summary>
    /// For Combination Orders - must use combo leg details
    /// </summary>
    [StringSerializer("BAG")]
    Bag,
    /// <summary>
    /// Bond
    /// </summary>
    [StringSerializer("BOND")]
    Bond,
    /// <summary>
    /// Warrant
    /// </summary>
    [StringSerializer("WAR")]
    Warrant,
    /// <summary>
    /// Undefined Security Type
    /// </summary>
    [StringSerializer("")]
    Undefined
  }
  [Serializable]
  public enum IBAction
  {
    /// <summary>
    /// Security is to be bought.
    /// </summary>
    [StringSerializer("BUY")]
    Buy,
    /// <summary>
    /// Security is to be sold.
    /// </summary>
    [StringSerializer("SELL")]
    Sell,
    /// <summary>
    /// Undefined
    /// </summary>
    [StringSerializer("")]
    Undefined,
    /// <summary>
    /// Sell Short as part of a combo leg
    /// </summary>
    [StringSerializer("SSHORT")]
    SShort,
    /// <summary>
    /// Short Sale Exempt action.
    /// SSHORTX allows some orders to be marked as exempt from the new SEC Rule 201
    /// </summary>
    [StringSerializer("SSHORTX")]
    SShortX
  }

  
  [Serializable]
  public enum IBTimeInForce
  {
    /// <summary>
    /// Day
    /// </summary>
    [StringSerializer("DAY")]
    Day,
    /// <summary>
    /// Good Till Cancel
    /// </summary>
    [StringSerializer("GTC")]
    GoodTillCancel,
    /// <summary>
    /// You can set the time in force for MARKET or LIMIT orders as IOC. This dictates that any portion of the order not executed immediately after it becomes available on the market will be cancelled.
    /// </summary>
    [StringSerializer("IOC")]
    ImmediateOrCancel,
    /// <summary>
    /// Setting FOK as the time in force dictates that the entire order must execute immediately or be canceled.
    /// </summary>
    [StringSerializer("FOK")]
    FillOrKill,
    /// <summary>
    /// Good Till Date
    /// </summary>
    [StringSerializer("GTD")]
    GoodTillDate,
    /// <summary>
    /// Market On Open
    /// </summary>
    [StringSerializer("OPG")]
    MarketOnOpen,
    /// <summary>
    /// Undefined
    /// </summary>
    [StringSerializer("")]
    Undefined
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
    DeltaNuetralValidation = 56,
    /// <summary>
    /// End of Tick Snapshot message
    /// </summary>
    TickSnapshotEnd = 57
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
    RequestCalcImpliedVolatility = 54
  }

  public enum TWSHistoricState
  {
    Starting,
    Downloading,
    Finished
  }
}