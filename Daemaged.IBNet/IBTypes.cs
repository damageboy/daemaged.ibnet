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
using System.Collections.Generic;

namespace Daemaged.IBNet
{
  public class TWSClientInfo
  {
    // Client version history
    //
    // 	6 = Added parentId to orderStatus
    // 	7 = The new execDetails event returned for an order filled status and reqExecDetails
    //     Also market depth is available.
    // 	8 = Added lastFillPrice to orderStatus() event and permId to execution details
    //  9 = Added 'averageCost', 'unrealizedPNL', and 'unrealizedPNL' to updatePortfolio event
    // 10 = Added 'serverId' to the 'open order' & 'order status' events.
    //      We Send back all the API open orders upon connection.
    //      Added new methods reqAllOpenOrders, reqAutoOpenOrders()
    //      Added FA support - reqExecution has filter.
    //                       - reqAccountUpdates takes acct code.
    // 11 = Added permId to openOrder event.
    // 12 = requsting open order attributes ignoreRth, hidden, and discretionary
    // 13 = added goodAfterTime
    // 14 = always Send size on bid/ask/last tick
    // 15 = Send allocation description string on openOrder
    // 16 = can receive account name in account and portfolio updates, and fa params in openOrder
    // 17 = can receive liquidation field in exec reports, and notAutoAvailable field in mkt data
    // 18 = can receive good till date field in open order messages, and request intraday backfill
    // 19 = can receive rthOnly flag in OrderStatus
    // 20 = expects TWS time string on connection after server version >= 20.
    // 21 = can receive bond contract details.
    // 22 = can receive price magnifier in version 2 contract details message
    // 23 = support for scanner
    // 24 = can receive volatility order parameters in open order messages
    // 25 = can receive HMDS query start and end times
    // 26 = can receive option vols in option market data messages
    // 27 = can receive delta neutral order type and delta neutral aux price in place order version 20: API 8.85
    // 28 = can receive option model computation ticks: API 8.9
    // 29 = can receive trail stop limit price in open order and can place them: API 8.91
    // 30 = can receive extended bond contract def, new ticks, and trade count in bars
    // 31 = can receive EFP extensions to scanner and market data, and combo legs on open orders
    //    ; can receive RT bars 
    // 32 = can receive TickType.LAST_TIMESTAMP
    //    ; can receive "whyHeld" in order status messages 

    //
    // 33 = can receive ScaleNumComponents and ScaleComponentSize is open order messages 
    // 34 = can receive whatIf orders / order state
    // 35 = can receive contId field for Contract objects
    // 36 = can receive outsideRth field for Order objects
    // 37 = can receive clearingAccount and clearingIntent for Order objects
    // 38 = can receive multiplier and primaryExchange in portfolio updates
    //    ; can receive cumQty and avgPrice in execution
    //    ; can receive fundamental data
    //    ; can receive underComp for Contract objects
    //    ; can receive reqId and end marker in contractDetails/bondContractDetails
    //    ; can receive ScaleInitComponentSize and ScaleSubsComponentSize for Order objects
    // 39 = can receive underConId in contractDetails
    // 40 = can receive algoStrategy/algoParams in openOrder
    // 41 = can receive end marker for openOrder
    //    ; can receive end marker for account download
    //    ; can receive end marker for executions download
    // 42 = can receive deltaNeutralValidation
    // 43 = can receive longName(companyName)
    //    ; can receive listingExchange
    //    ; can receive RTVolume tick
    // 44 = can receive end market for ticker snapshot
    // 45 = can receive notHeld field in openOrder
    // 46 = can receive contractMonth, industry, category, subcategory fields in contractDetails
    //    ; can receive timeZoneId, tradingHours, liquidHours fields in contractDetails
    // 47 = can receive gamma, vega, theta, undPrice fields in TICK_OPTION_COMPUTATION
    // 48 = can receive exemptCode in openOrder
    // 49 = can receive hedgeType and hedgeParam in openOrder
    // 50 = can receive optOutSmartRouting field in openOrder
    // 51 = can receive smartComboRoutingParams in openOrder
    // 52 = can receive deltaNeutralConId, deltaNeutralSettlingFirm, deltaNeutralClearingAccount and deltaNeutralClearingIntent in openOrder
    // 53 = can receive orderRef in execution
    // 54 = can receive scale order fields (PriceAdjustValue, PriceAdjustInterval, ProfitOffset, AutoReset, 
    //      InitPosition, InitFillQty and RandomPercent) in openOrder
    // 55 = can receive orderComboLegs (price) in openOrder
    // 56 = can receive trailingPercent in openOrder
    // 57 = can receive commissionReport message
    // 58 = can receive CUSIP/ISIN/etc. in contractDescription/bondContractDescription
    // 59 = can receive evRule, evMultiplier in contractDescription/bondContractDescription/executionDetails
    //      can receive multiplier in executionDetails
    // 60 = can receive deltaNeutralOpenClose, deltaNeutralShortSale, deltaNeutralShortSaleSlot and deltaNeutralDesignatedLocation in openOrder


    private const int CLIENT_VERSION = 60;



    public TWSClientInfo(int version)
    {
      Version = version;
    }

    public TWSClientInfo()
    {
      Version = CLIENT_VERSION;
    }

    public int Version { get; private set; }
  }

  public class TWSServerInfo
  {
    internal const int MIN_SERVER_VER_REAL_TIME_BARS = 34;
    internal const int MIN_SERVER_VER_SCALE_ORDERS = 35;
    internal const int MIN_SERVER_VER_SNAPSHOT_MKT_DATA = 35;
    internal const int MIN_SERVER_VER_SSHORT_COMBO_LEGS = 35;
    internal const int MIN_SERVER_VER_WHAT_IF_ORDERS = 36;
    internal const int MIN_SERVER_VER_CONTRACT_CONID = 37;
    internal const int MIN_SERVER_VER_PTA_ORDERS = 39;
    internal const int MIN_SERVER_VER_FUNDAMENTAL_DATA = 40;
    internal const int MIN_SERVER_VER_UNDER_COMP = 40;
    internal const int MIN_SERVER_VER_CONTRACT_DATA_CHAIN = 40;
    internal const int MIN_SERVER_VER_SCALE_ORDERS2 = 40;
    internal const int MIN_SERVER_VER_ALGO_ORDERS = 41;
    internal const int MIN_SERVER_VER_EXECUTION_DATA_CHAIN = 42;
    internal const int MIN_SERVER_VER_NOT_HELD = 44;
    internal const int MIN_SERVER_VER_SEC_ID_TYPE = 45;
    internal const int MIN_SERVER_VER_PLACE_ORDER_CONID = 46;
    internal const int MIN_SERVER_VER_REQ_MKT_DATA_CONID = 47;
    internal const int MIN_SERVER_VER_REQ_CALC_IMPLIED_VOLAT = 49;
    internal const int MIN_SERVER_VER_REQ_CALC_OPTION_PRICE = 50;
    internal const int MIN_SERVER_VER_CANCEL_CALC_IMPLIED_VOLAT = 50;
    internal const int MIN_SERVER_VER_CANCEL_CALC_OPTION_PRICE = 50;
    internal const int MIN_SERVER_VER_SSHORTX_OLD = 51;
    internal const int MIN_SERVER_VER_SSHORTX = 52;
    internal const int MIN_SERVER_VER_REQ_GLOBAL_CANCEL = 53;
    internal const int MIN_SERVER_VER_HEDGE_ORDERS = 54;
    internal const int MIN_SERVER_VER_REQ_MARKET_DATA_TYPE = 55;
    internal const int MIN_SERVER_VER_OPT_OUT_SMART_ROUTING = 56;
    internal const int MIN_SERVER_VER_SMART_COMBO_ROUTING_PARAMS = 57;
    internal const int MIN_SERVER_VER_DELTA_NEUTRAL_CONID = 58;
    internal const int MIN_SERVER_VER_SCALE_ORDERS3 = 60;
    internal const int MIN_SERVER_VER_ORDER_COMBO_LEGS_PRICE = 61;
    internal const int MIN_SERVER_VER_TRAILING_PERCENT = 62;
    internal const int MIN_SERVER_VER_DELTA_NEUTRAL_OPEN_CLOSE = 66;
    internal const int SERVER_VERSION = 38;
    public TWSServerInfo(int version)
    {
      Version = version;
    }

    public int Version { get; private set; }
  }

  public class TWSClientId
  {
    public TWSClientId(int id)
    {
      Id = id;
    }

    public int Id { get; private set; }
  }


  public class IBExecutionFilter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="IBExecutionFilter"/> class.
    /// </summary>
    public IBExecutionFilter()
    {
      ClientId = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IBExecutionFilter"/> class.
    /// </summary>
    /// <param name="clientId">Filter the results of the ReqExecutions() method based on the clientId.</param>
    /// <param name="acctCode">Filter the results of the ReqExecutions() method based on an account code.</param>
    /// <param name="time">Filter the results of the ReqExecutions() method based on execution reports received after the specified time.</param>
    /// <param name="symbol">Filter the results of the ReqExecutions() method based on the order symbol.</param>
    /// <param name="securityType">Refer to the Contract struct for the list of valid security types.</param>
    /// <param name="exchange">Filter the results of the ReqExecutions() method based on the order exchange.</param>
    /// <param name="side">Filter the results of the ReqExecutions() method based on the order action.</param>
    public IBExecutionFilter(int clientId, string acctCode, DateTime time,
                             string symbol, IBSecurityType securityType, string exchange, string side)
    {
      ClientId = clientId;
      AcctCode = acctCode;
      DateTime = time;
      Symbol = symbol;
      SecurityType = securityType;
      Exchange = exchange;
      Side = side;
    }

    /// <summary>
    /// Filter the results of the ReqExecutions() method based on the clientId.
    /// </summary>
    public int ClientId { get; set; }
    /// <summary>
    /// Filter the results of the ReqExecutions() method based on an account code.
    /// </summary>
    /// <remarks>This is only relevant for Financial Advisor (FA) accounts.</remarks>
    public string AcctCode { get; set; }
    /// <summary>
    /// Filter the results of the ReqExecutions() method based on execution reports received after the specified time. 
    /// </summary>
    /// <remarks>The format for timeFilter is "yyyymmdd-hh:mm:ss"</remarks>
    public DateTime DateTime { get; set; }
    /// <summary>
    /// Filter the results of the ReqExecutions() method based on the order symbol.
    /// </summary>
    public string Symbol { get; set; }
    /// <summary>
    /// Filter the results of the ReqExecutions() method based on the order security type. 
    /// </summary>
    /// <remarks>Refer to the Contract structure for the list of valid security types.</remarks>
    public IBSecurityType SecurityType { get; set; }
    /// <summary>
    /// Filter the results of the ReqExecutions() method based on the order exchange.
    /// </summary>
    public string Exchange { get; set; }
    /// <summary>
    /// Filter the results of the ReqExecutions() method based on the order action. 
    /// </summary>
    /// <remarks>Refer to the Order structure for the list of valid order actions.</remarks>
    public string Side { get; set; }
  }

  public class IBComboLeg
  {
    /// <summary>
    /// The unique contract identifier specifying the security.
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// Select the relative number of contracts for the leg you are constructing.
    /// To help determine the ratio for a specific combination order, refer to the
    /// Interactive Analytics section of the User's Guide.
    /// </summary>
    public int Ratio { get; set; }

    /// <summary>
    /// The side (buy or sell) for the leg you are constructing.
    /// </summary>
    public IBAction Action { get; set; }

    /// <summary>
    /// The exchange to which the complete combination order will be routed.
    /// </summary>
    public string Exchange { get; set; }

    /// <summary>
    /// Specifies whether the order is an open or close order.
    /// Retail customers must use <see cref="ComboOpenClose.Same"/>.
    /// </summary>
    /// <seealso cref="ComboOpenClose"/>
    public IBComboOpenClose OpenClose { get; set; }

    /// <summary>
    /// ShortSaleSlot of Third Party requires DesignatedLocation to be specified. Non-empty DesignatedLocation values for all other cases will cause orders to be rejected.
    /// </summary>
    public IBShortSaleSlot ShortSaleSlot { get; set; }

    /// <summary>
    /// Use only when shortSaleSlot value = 2.
    /// </summary>
    public string DesignatedLocation { get; set; }

    /// <summary>
    /// Short Sale Exempt Code
    /// </summary>
    public int ExemptCode { get; set; }

    public IBComboLeg()
    {
    }
  }

  public class IBContract
  {
    public IBContract()
    {
      ComboLegs = new List<IBComboLeg>();
    }

    /// <summary>The client-generated request ID for this contract. Used serverside only.</summary>
    public int RequestId { get; set; }

    public string Symbol { get; set; }
    public IBSecurityType SecurityType { get; set; }
    public DateTime Expiry { get; set; }
    public double Strike { get; set; }
    public string Right { get; set; }
    public string Multiplier { get; set; }
    public string Exchange { get; set; }

    public string Currency { get; set; }
    public string LocalSymbol { get; set; }
    // pick an actual (ie non-aggregate) exchange that the contract trades on.  DO NOT SET TO SMART.
    public string PrimaryExchange { get; set; }
    public bool IncludeExpired { get; set; }

    // received in open order version 14 and up for all combos
    public String ComboLegsDescription { get; set; }
    public List<IBComboLeg> ComboLegs { get; set; }


    // BOND values
    public string Cusip { get; set; }
    public string Ratings { get; set; }
    public string DescAppend { get; set; }
    public string BondType { get; set; }
    public string CouponType { get; set; }
    public bool Callable { get; set; }
    public bool Putable { get; set; }
    public double Coupon { get; set; }
    public bool Convertible { get; set; }
    public string Maturity { get; set; }
    public string IssueDate { get; set; }

    public string NextOptionDate { get; set; }
    public string NextOptionType { get; set; }
    public bool NextOptionPartial { get; set; }
    public string Notes { get; set; }

    public int ContractId { get; set; }

    public IBUnderlyinhComponent UnderlyingComponent { get; set; }

    public string SecurityId { get; set; }

    public IBSecurityType SecurityIdType { get; set; }

    public override bool Equals(object obj)
    {
      if ((obj == null) ||
          !(obj is IBContract))
        return false;


      var other = obj as IBContract;

      if (other.ComboLegs.Count != ComboLegs.Count)
        return false;

      return (other.Exchange == Exchange) &&
             (other.Symbol == Symbol) &&
             (other.Currency == Currency) &&
             (other.Expiry == Expiry) &&
             (other.SecurityType == SecurityType) &&
             (other.Strike == Strike) &&
             (other.Multiplier == Multiplier);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
      //return Exchange.GetHashCode() +
      //       Symbol.GetHashCode() +
      //       Currency.GetHashCode() +
      //       Expiry.GetHashCode() +
      //       SecurityType.GetHashCode() +
      //       Strike.GetHashCode() +
      //       Multiplier.GetHashCode();
    }
  }

  public class IBContractDetails
  {
    public IBContractDetails()
    {
      Summary = new IBContract();
      ValidExchanges = OrderTypes = Multiplier = TradingClass = MarketName = null;
      PriceMagnifier = Conid = 0;
      MinTick = 0;
    }

    public IBContract Summary { get; set; }
    public string MarketName { get; set; }
    public string TradingClass { get; set; }
    public int Conid { get; set; }
    public double MinTick { get; set; }
    public string Multiplier { get; set; }
    public int PriceMagnifier { get; set; }
    public string OrderTypes { get; set; }
    public string ValidExchanges { get; set; }

    public List<IBTagValue> SecIdList { get; set; }

    public string ContractMonth { get; set; }

    public string Industry { get; set; }

    public string Category { get; set; }

    public string Subcategory { get; set; }

    public string TimeZoneId { get; set; }

    public string TradingHours { get; set; }

    public string LiquidHours { get; set; }

    public string EvRule { get; set; }

    public double EvMultiplier { get; set; }

    public string LongName { get; set; }

    public int UnderlyingContractId { get; set; }
  }

  public class IBExecution
  {
    public IBExecution()
    {
      ClientId = OrderID = Shares = Liquidation = 0;
      Price = 0;
      ExecID = Time = AcctNumber = Exchange = Side = null;
    }

    public string AcctNumber { get; set; }
    public int ClientId { get; set; }
    public string Exchange { get; set; }
    public string ExecID { get; set; }
    public int Liquidation { get; set; }
    public int OrderID { get; set; }
    public int PermId { get; set; }
    public double Price { get; set; }
    public int Shares { get; set; }
    public string Side { get; set; }
    public string Time { get; set; }

    public int CumQty { get; set; }

    public double AveragePrice { get; set; }

    public string OrderRef { get; set; }

    public string EvRule { get; set; }

    public double EvMultiplier { get; set; }
  }

  public class IBOrder
  {
    internal const int AUCTION_IMPROVEMENT = 2;
    internal const int AUCTION_MATCH = 1;
    internal const int AUCTION_TRANSPARENT = 3;
    internal const int CUSTOMER = 0;
    internal const string EMPTY_STR = "";
    internal const int FIRM = 1;
    internal const char OPT_BROKER_DEALER = 'b';
    internal const char OPT_CUSTOMER = 'c';
    internal const char OPT_FARMM = 'n';
    internal const char OPT_FIRM = 'f';
    internal const char OPT_ISEMM = 'm';
    internal const char OPT_SPECIALIST = 'y';
    internal const char OPT_UNKNOWN = '?';

    /// <summary>
    /// The order id
    /// </summary>
    public int OrderId;
    /// <summary>
    /// The TWS id used to identify orders, remains permanent over TWS sessions
    /// </summary>
    public int PermId;
    /// <summary>
    /// The client id
    /// </summary>
    public int ClientId;
    /// <summary>
    /// The account
    /// </summary>
    public string Account;


    /// <summary>
    /// Identifies the side. (e.g. Buy/Sell/SellShort)
    /// </summary>
    public IBAction Action;

    /// <summary>
    /// The total order quantity
    /// </summary>
    public int TotalQuantity;

    /// <summary>
    /// The order type (e.g. Market/Limit etc.)
    /// </summary>
    public IBOrderType OrderType;

    /// <summary>
    /// The limit price, used for limit, stop-limit and relative orders.
    /// In all other cases specify zero. For relative orders with no limit price,
    /// also specify zero.
    /// </summary>
    public double LimitPrice;

    /// <summary>
    /// The aux price, used as STOP price for stop + stop-limit orders, and the offset amount for
    /// relative orders. In all other cases, specify zero
    /// </summary>
    public double AuxPrice;

    /// <summary>
    /// The Time-In-Force (e.g. GTC etc.)
    /// </summary>
    public IBTimeInForce Tif; // "Time in Force" - DAY, GTC, etc.

    /// <summary>
    /// The One-Cancels-All group name
    /// </summary>
    public string OcaGroup;

    /// <summary>
    /// The One-Cancels-All type
    /// </summary>
    public IBOcaType OcaType;
    
    public bool AllOrNone;
    public int AuctionStrategy; // 1=AUCTION_MATCH, 2=AUCTION_IMPROVEMENT, 3=AUCTION_TRANSPARENT
    
    public double BasisPoints; // EFP orders only
    public int BasisPointsType; // EFP orders only

    // extended order fields
    public bool BlockOrder;
    
    public int ContinuousUpdate;
    public double Delta;
    public double DeltaNeutralAuxPrice;
    public IBOrderType DeltaNeutralOrderType;
    public string DesignatedLocation; // set when slot=2 only.

    // SMART routing only
    public double DiscretionaryAmt;
    public int DisplaySize;
    public bool ETradeOnly;

    // Financial advisors only 
    public string FaGroup;
    public string FaMethod;
    public string FaPercentage;
    public string FaProfile;
    public bool FirmQuoteOnly;
    public string GoodAfterTime; // FORMAT: 20060505 08:00:00 {time zone}
    public string GoodTillDate; // FORMAT: 20060505 08:00:00 {time zone}
    public bool Hidden;
    [Obsolete]
    public bool IgnoreRth;
    
    public int MinQty;
    public double NbboPriceCap;
    
    

    // Institutional orders only
    public string OpenClose; // O=Open, C=Close
    
    public string OrderRef;
    
    public int Origin; // 0=Customer, 1=Firm
    public bool OverridePercentageConstraints;
    public int ParentId; // Parent order Id, to associate Auto STP or TRAIL orders with the original order.
    public double PercentOffset; // REL orders only
    
    public int ReferencePriceType; // 1=Average, 2 = BidOrAsk
    [Obsolete]
    public bool RthOnly;

    public string Rule80A;
                  // Individual = 'I', Agency = 'A', AgentOtherMember = 'W', IndividualPTIA = 'J', AgencyPTIA = 'U', AgentOtherMemberPTIA = 'M', IndividualPT = 'K', AgencyPT = 'Y', AgentOtherMemberPT = 'N'

    public string SettlingFirm;
    [Obsolete]
    public string SharesAllocation; // deprecated

    public int ShortSaleSlot;
               // 1 if you hold the shares, 2 if they will be delivered from elsewhere.  Only for Action="SSHORT

    // BOX ORDERS ONLY
    public double StartingPrice;

    // pegged to stock or VOL orders
    public double StockRangeLower;
    public double StockRangeUpper;
    public double StockRefPrice;
    public bool SweepToFill;
    
    
    public double TrailStopPrice; // for TRAILLIMIT orders only
    public bool Transmit; // if false, order will be created but not transmited

    public int TriggerMethod;
               // 0=Default, 1=Double_Bid_Ask, 2=Last, 3=Double_Last, 4=Bid_Ask, 7=Last_or_Bid_Ask, 8=Mid-point

    // VOLATILITY ORDERS ONLY
    public double Volatility;
    public int VolatilityType; // 1=daily, 2=annual

    public IBOrder()
    {
      OpenClose = "O";
      Origin = CUSTOMER;
      Transmit = true;
      DesignatedLocation = EMPTY_STR;
      MinQty = Int32.MaxValue;
      PercentOffset = Double.MaxValue;
      NbboPriceCap = Double.MaxValue;
      StartingPrice = Double.MaxValue;
      StockRefPrice = Double.MaxValue;
      Delta = Double.MaxValue;
      StockRangeLower = Double.MaxValue;
      StockRangeUpper = Double.MaxValue;
      Volatility = Double.MaxValue;      
      VolatilityType = Int32.MaxValue;
      DeltaNeutralAuxPrice = Double.MaxValue;
      ReferencePriceType = Int32.MaxValue;
      TrailStopPrice = Double.MaxValue;
      BasisPoints = Int32.MaxValue;
      BasisPointsType = Int32.MaxValue;
    }

    public int ExemptCode { get; set; }

    public int DeltaNeutralContractId { get; set; }

    public string DeltaNeutralSettlingFirm { get; set; }

    public string DeltaNeutralClearingAccount { get; set; }

    public string DeltaNeutralClearingIntent { get; set; }

    public string DeltaNeutralOpenClose { get; set; }

    public bool DeltaNeutralShortSale { get; set; }

    public int DeltaNeutralShortSaleSlot { get; set; }

    public string DeltaNeutralDesignatedLocation { get; set; }

    public bool OutsideRth { get; set; }

    public double TrailingPercent { get; set; }

    public int ScaleInitLevelSize { get; set; }

    public int ScaleSubsLevelSize { get; set; }

    public double ScalePriceIncrement { get; set; }

    public string ClearingAccount { get; set; }

    public string ClearingIntent { get; set; }

    public bool NotHeld { get; set; }

    public string AlgoStrategy { get; set; }

    public List<IBTagValue> AlgoParams { get; set; }

    public bool OptOutSmartRouting { get; set; }

    public string HedgeType { get; set; }

    public string HedgeParam { get; set; }

    public double ScalePriceAdjustValue { get; set; }

    public int ScalePriceAdjustInterval { get; set; }

    public double ScaleProfitOffset { get; set; }

    public bool ScaleAutoReset { get; set; }

    public int ScaleInitPosition { get; set; }

    public int ScaleInitFillQty { get; set; }

    public bool ScaleRandomPercent { get; set; }

    public List<IBTagValue> SmartComboRoutingParams { get; set; }

    public bool WhatIf { get; set; }

    public List<IBOrderComboLeg> OrderComboLegs { get; set; }
  }

  public class CommissionReport
  {
    public double RealizedPnl { get; set; }
    public string ExecId { get; set; }
    public double Commission { get; set; }
    public string Currency { get; set; }
    public double Yield { get; set; }
    public int YieldRedemptionDate { get; set; }
  }

  public class IBTagValue
  {

    /// <summary>
    /// Tag Name
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Value of Tag
    /// </summary>
    public string Value { get; set; }
  }

  /// <summary>
  /// Underlying Component Class
  /// </summary>
  [Serializable]
  public class IBUnderlyinhComponent
  {   
    #region Public Properties

    /// <summary>
    /// Contract Id
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// Delta Value
    /// </summary>
    public double Delta { get; set; }

    /// <summary>
    /// Price
    /// </summary>
    public double Price { get; set; }

    #endregion
  }

  /// <summary>
  /// The openOrder() callback with the new OrderState() object will now be invoked
  /// each time TWS receives commission information for a trade.
  /// </summary>
  [Serializable]
  public class OrderState
  {
    #region Properties

    /// <summary>
    /// Order Status
    /// </summary>
    public IBOrderStatus Status { get; set; }

    /// <summary>
    /// Initial margin requirement for the order.
    /// </summary>
    public string InitMargin { get; set; }

    /// <summary>
    /// Maintenance margin requirement for the order.
    /// </summary>
    public string MaintMargin { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double Commission { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double MinCommission { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double MaxCommission { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string CommissionCurrency { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string WarningText { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string EquityWithLoan { get; set; }

    #endregion
  }

  public class IBOrderComboLeg
  {
    public double Price { get; set; }
  }

}