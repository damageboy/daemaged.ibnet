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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Daemaged.IBNet.Client
{
  public class TWSClient
  {
    public const string DEFAULT_HOST = "127.0.0.1";
    public const int DEFAULT_PORT = 7496;
    private const int DEFAULT_WAIT_TIMEOUT = 10000;
    private const string IB_DATE_FORMAT = "yyyyMMdd  HH:mm:ss";
    private const string IB_EXPIRY_DATE_FORMAT = "yyyyMMdd";
    private const string IB_HISTORICAL_COMPLETED = "finished";
    private readonly Dictionary<IBContract, KeyValuePair<AutoResetEvent, IBContractDetails>> _internalDetailRequests;
    private readonly Dictionary<int, OrderRecord> _orderRecords;
    private int _clientId;
    private bool _doWork;
    protected ITWSEncoding _enc;
    private IPEndPoint _endPoint;
    protected Dictionary<int, TWSMarketDataSnapshot> _marketDataRecords;
    private int _nextValidId;
    private Dictionary<string, int> _orderIds;
    private bool _reconnect;
    private bool _recordForPlayback;
    private Stream _recordStream;
    private TWSClientSettings _settings;
    private Stream _stream;
    private TcpClient _tcpClient;
    private Thread _thread;
    private string _twsTime;
    private string NUMBER_DECIMAL_SEPARATOR;
    

    #region Constructors

    public TWSClient()
    {
      _tcpClient = null;
      _stream = null;
      _thread = null;
      Status = TWSClientStatus.Unknown;
      _twsTime = String.Empty;
      _nextValidId = 0;

      //_historicalDataRecords = new Dictionary<int, TWSMarketDataSnapshot>();
      _marketDataRecords = new Dictionary<int, TWSMarketDataSnapshot>();
      //_marketDepthRecords = new Dictionary<int, TWSMarketDataSnapshot>();
      _orderRecords = new Dictionary<int, OrderRecord>();
      _internalDetailRequests = new Dictionary<IBContract, KeyValuePair<AutoResetEvent, IBContractDetails>>();
      _orderIds = new Dictionary<string, int>();

      ClientInfo = new TWSClientInfo();

      NUMBER_DECIMAL_SEPARATOR = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
      EndPoint = new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), DEFAULT_PORT);

      _settings = new TWSClientSettings();
    }

    public TWSClient(IPEndPoint server) : this()
    {
      _endPoint = server;
    }

    public TWSClient(string host, int port) : this()
    {
      _endPoint = new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port);
    }

    #endregion

    #region Connect/Disconnect

    /// <summary>
    /// Connect to the specified IB Trader Workstation Endpoint, auto-calculating the client id
    /// </summary>
    /// <remarks>
    /// Using this method may result in the connection being reject for duplicate client id, since the
    /// client id is automatically calculated from the local end point (ip address & port)
    /// </remarks>
    public void Connect()
    {
      Connect(-1);
    }

    /// <summary>
    /// Connect to the specified IB Trader Workstation Endpoint
    /// </summary>
    /// <param name="clientId">The client id to use when connecting</param>
    public void Connect(int clientId)
    {
      lock (this) {
        if (IsConnected) {
          OnError(TWSErrors.ALREADY_CONNECTED);
          return;
        }
        try {
          _tcpClient = new TcpClient();
          _tcpClient.Connect(_endPoint);
          _tcpClient.NoDelay = true;

          if (RecordForPlayback) {
            if (_recordStream == null)
              _recordStream = SetupDefaultRecordStream();

            _enc = new TWSPlaybackRecorderEncoding(new BufferedReadStream(_tcpClient.GetStream()), _recordStream);
          }
          else
            _enc = new TWSEncoding(new BufferedReadStream(_tcpClient.GetStream()));


          _enc.Encode(ClientInfo);
          _doWork = true;

          // Only create a reader thread if this Feed IS NOT reconnecting
          if (!_reconnect) {
            _thread = new Thread(ProcessMessages)
              {
                IsBackground = true, 
                Name = "IBReader"
              };
          }
          // Get the server version
          ServerInfo = _enc.DecodeServerInfo();
          if (ServerInfo.Version >= 20) {
            _twsTime = _enc.DecodeString();
          }

          // Send the client id
          if (ServerInfo.Version >= 3) {
            if (clientId == -1) {
              if (_tcpClient.Client.LocalEndPoint is IPEndPoint) {
                var p = _tcpClient.Client.LocalEndPoint as IPEndPoint;
                byte[] ab = p.Address.GetAddressBytes();
                clientId = ab[ab.Length - 1] << 16 | p.Port;
              }
              else
                clientId = new Random().Next();
            }
            var id = new TWSClientId(clientId);
            _enc.Encode(id);
          }

          // Only start the thread if this Feed IS NOT reconnecting
          if (!_reconnect)
            _thread.Start();

          _clientId = clientId;
          OnStatusChanged(Status = TWSClientStatus.Connected);
        }
        catch (Exception e) {
          OnError(e.Message);
          OnError(TWSErrors.CONNECT_FAIL);
        }
      }
    }

    /// <summary>
    /// Disconnect from the IB Trader Workstation endpoint
    /// </summary>
    public void Disconnect()
    {
      if (!IsConnected)
        return;

      lock (this) {
        _doWork = false;
        if (_tcpClient != null)
          _tcpClient.Close();
        _thread = null;
        _tcpClient = null;
        _stream = null;
        if (RecordStream != null) {
          RecordStream.Flush();
          RecordStream.Close();
        }
        OnStatusChanged(Status = TWSClientStatus.Disconnected);
      }
    }

    /// <summary>
    /// Reconnect to the IB Trader Workstation, re-register all markert data requests
    /// </summary>
    public void Reconnect()
    {
      if (!IsConnected)
        return;

      lock (this) {
        _reconnect = true;
        Disconnect();
        Connect(_clientId);
      }
    }

    #endregion

    #region Cancel Messages

    /// <summary>
    /// Cancel a registered scanner subscription
    /// </summary>
    /// <param name="reqId">The scanner subscription request id</param>
    public void CancelScannerSubscription(int reqId)
    {
      lock (this) {
        // not connected?
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return;
        }

        if (ServerInfo.Version < 24) {
          OnError(TWSErrors.UPDATE_TWS);
          return;
        }

        const int reqVersion = 1;

        // Send cancel mkt data msg
        try {
          _enc.Encode(ServerMessage.CancelScannerSubscription);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception e) {
          OnError(reqId, TWSErrors.FAIL_SEND_CANSCANNER);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    /// <summary>
    /// Cancel historical data subscription
    /// </summary>
    /// <param name="reqId">The historical data subscription request id</param>
    public void CancelHistoricalData(int reqId)
    {
      lock (this) {
        // not connected?
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return;
        }

        if (ServerInfo.Version < 24) {
          OnError(TWSErrors.UPDATE_TWS);
          return;
        }

        const int reqVersion = 1;

        // Send cancel mkt data msg
        try {
          _enc.Encode(ServerMessage.CancelHistoricalData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception e) {
          OnError(reqId, TWSErrors.FAIL_SEND_CANHISTDATA);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    public void CancelMarketData(int reqId)
    {
      lock (this) {
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return;
        }
        const int reqVersion = 1;
        try {
          _enc.Encode(ServerMessage.CancelMarketData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception e) {
          OnError(reqId, TWSErrors.FAIL_SEND_CANMKT);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    public void CancelMarketDepth(int reqId)
    {
      lock (this) {
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return;
        }
        if (ServerInfo.Version < 6) {
          OnError(TWSErrors.UPDATE_TWS);
          return;
        }
        const int reqVersion = 1;
        try {
          _enc.Encode(ServerMessage.CancelMarketDepth);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_CANMKTDEPTH);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    public void CancelNewsBulletins()
    {
      lock (this) {
        // not connected?
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return;
        }

        const int reqVersion = 1;

        // Send cancel order msg
        try {
          _enc.Encode(ServerMessage.CancelNewsBulletins);
          _enc.Encode(reqVersion);
        }
        catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_CORDER);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    public void CancelOrder(int orderId)
    {
      lock (this) {
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return;
        }
        const int reqVersion = 1;
        try {
          _enc.Encode(ServerMessage.CancelOrder);
          _enc.Encode(reqVersion);
          _enc.Encode(orderId);
        }
        catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_CORDER);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    public void CancelRealTimeBars(int reqId)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      if (ServerInfo.Version < 34) {
        OnError(TWSErrors.UPDATE_TWS);
        return;
      }

      const int reqVersion = 1;

      // send cancel mkt data msg
      try {
        _enc.Encode(ServerMessage.CancelHistoricalData);
        _enc.Encode(reqVersion);
        _enc.Encode(reqId);
      }
      catch (Exception e) {
        OnError(reqId, TWSErrors.FAIL_SEND_CANRTBARS);
        OnError(e.Message);
        Disconnect();
      }
    }

    #endregion

    #region Event Notifiers

    protected virtual void OnStatusChanged(TWSClientStatus status)
    {
      if (StatusChanged == null) return;
      StatusChanged(this, new TWSClientStatusEventArgs(this, status));
    }

    protected virtual void OnError(TWSError error)
    {
      OnError(TWSErrors.NO_VALID_ID, error);
    }

    protected virtual void OnError(int reqId, TWSError error)
    {
      if (Error == null) return;
      TWSMarketDataSnapshot snapshot;
      IBContract contract = null;

      if (_marketDataRecords.TryGetValue(reqId, out snapshot))
        contract = snapshot.Contract;

      Error(this, new TWSClientErrorEventArgs(this) {
        RequestId = reqId, 
        Contract = contract, 
        Error = error
      });
    }

    protected void OnError(string message)
    {
      OnError(new TWSError(TWSErrors.NO_VALID_CODE, message));
    }

    protected void OnMarketData(TWSMarketDataSnapshot snapshot, IBTickType tickType)
    {
      if (MarketData != null)
        MarketData(this, new TWSMarketDataEventArgs(this)
          {
            Snapshot =  snapshot,
            TickType = tickType
          });
    }

    protected void OnTickPrice(int reqId, IBTickType tickType, double price, int size, int canAutoExecute)
    {
      if (TickPrice != null)
        TickPrice(this, new TWSTickPriceEventArgs(this) {
          TickerId = reqId, 
          TickType = tickType, 
          Price = price, 
          Size = size, 
          CanAutoExecute = canAutoExecute
        });

      TWSMarketDataSnapshot record;
      if (!_marketDataRecords.TryGetValue(reqId, out record)) {
        OnError(String.Format("OnTickPrice: Error request id {0}", reqId));
        return;
      }
      Console.WriteLine("Client: Received tick price msg for reqid " + reqId + ", symbol " + record.Contract.Symbol +
                        ", price: " + price);
      // Crap?
      if (record.Contract.SecurityType != IBSecurityType.Index &&
          record.Contract.SecurityType != IBSecurityType.Cash &&
          size == 0)
        return;

      switch (tickType) {
        case IBTickType.BidPrice:
          record.Bid = price;
          if (!_settings.IgnoreSizeInPriceTicks)
            record.BidSize = size;
          record.BidTimeStamp = DateTime.Now;
          break;
        case IBTickType.AskPrice:
          record.Ask = price;
          if (!_settings.IgnoreSizeInPriceTicks)
            record.AskSize = size;
          record.AskTimeStamp = DateTime.Now;
          break;
        case IBTickType.LastPrice:
          // Make sure we are allowed to generate trades from this event type
          if ((_settings.TradeGeneration & TradeGeneration.LastSizePrice) == 0)
            return;
          record.Last = price;
          if (!_settings.IgnoreSizeInPriceTicks)
            record.LastSize = size;
          record.TradeTimeStamp = DateTime.Now;
          break;
        case IBTickType.HighPrice:
          record.High = price;
          break;
        case IBTickType.LowPrice:
          record.Low = price;
          break;
        case IBTickType.ClosePrice:
          record.Close = price;
          break;
        case IBTickType.OpenPrice:
          record.Open = price;
          break;

        default:
          throw new ArgumentException("Error tick type - " + tickType);
      }
      OnMarketData(record, tickType);
    }

    protected void OnTickSize(int reqId, IBTickType tickType, int size)
    {
      if (size == 0)
        return;

      if (TickSize != null)
        TickSize(this, new TWSTickSizeEventArgs(this) {
          TickerId = reqId, 
          TickType = tickType, 
          Size = size
        });

      TWSMarketDataSnapshot record;
      if (!_marketDataRecords.TryGetValue(reqId, out record)) {
        OnError(String.Format("OnTickPrice: Error request id {0}", reqId));
        return;
      }

      int recordSize = size;
      bool lastDupHit = false;

      switch (tickType) {
        case IBTickType.BidSize:
          if (record.BidSize == size && FilterDups(record.BidTimeStamp)) {
            lastDupHit = true;
            record.BidDups++;
            break;
          }
          record.BidSize = size;
          break;
        case IBTickType.AskSize:
          if (record.AskSize == size && FilterDups(record.AskTimeStamp)) {
            lastDupHit = true;
            record.AskDups++;
            break;
          }
          record.AskSize = size;
          break;
        case IBTickType.LastSize:
          // Make sure we are allowed to generate trades from this event type
          if ((_settings.TradeGeneration & TradeGeneration.LastSize) == 0)
            return;
          if (record.LastSize == size && FilterDups(record.TradeTimeStamp)) {
            lastDupHit = true;
            record.TradeDups++;
            break;
          }
          record.LastSize = size;
          break;
        case IBTickType.Volume:
          record.Volume = size;
          if ((_settings.TradeGeneration & TradeGeneration.Volume) != 0) {
            // Synthetic volume matches reported volume
            if (record.SyntheticVolume == size)
              break;

            // This is just plain wrong... we may want to raise some sort
            // of red flag here..?!?
            if (record.SyntheticVolume > size)
              break;

            // If we got to here, it means we need to generate a trade from volume changes
            // with the last price using the volume difference between
            // the reported volume and the synthetic one...
            record.LastSize = (size - record.SyntheticVolume);
          }
          break;
        default:
          throw new ArgumentException("Error tick type - " + tickType);
      }
      if (lastDupHit == false)
        OnMarketData(record, tickType);
    }

    private bool FilterDups(DateTime dateTime)
    {
      return _settings.UseDupFilter &&
             (DateTime.Now.Subtract(dateTime) < _settings.DupDetectionTimeout);
    }

    protected void OnTickOptionComputation(int reqId, IBTickType tickType,
                                           double impliedVol, double delta, double modelPrice, double pvDividend)
    {
      if (TickOptionComputation != null)
        TickOptionComputation(this, new TWSTickOptionComputationEventArgs(this) {
          RequestId = reqId, 
          TickType = tickType,
          ImpliedVol = impliedVol, 
          Delta = delta, 
          ModelPrice = modelPrice, 
          PVDividend = pvDividend,
        });

      TWSMarketDataSnapshot record;
      if (!_marketDataRecords.TryGetValue(reqId, out record)) {
        OnError(String.Format("OnTickPrice: Error request id {0}", reqId));
        return;
      }

      switch (tickType) {
        case IBTickType.BidOption:
          record.BidImpliedVol = impliedVol;
          record.BidDelta = delta;
          break;
        case IBTickType.AskOption:
          record.AskImpliedVol = impliedVol;
          record.AskDelta = delta;
          break;
        case IBTickType.LastOption:
          record.ImpliedVol = impliedVol;
          record.Delta = delta;
          break;
        case IBTickType.ModelOption:
          record.ImpliedVol = impliedVol;
          record.Delta = delta;
          record.PVDividend = pvDividend;
          record.ModelPrice = modelPrice;
          break;
        default:
          throw new ArgumentException("Error tick type - " + tickType);
      }

      OnMarketData(record, tickType);
    }

    private void OnTickEFP(int reqId, IBTickType tickType, double basisPoints, string formattedBasisPoints,
                           double impliedFuturesPrice, int holdDays, string futureExpiry,
                           double dividendImpact, double dividendsToExpiry)
    {
      if (TickEFP != null)
        TickEFP(this, new TWSTickEFPEventArgs(this) {
          TickerId = reqId, 
          TickType = tickType, 
          BasisPoints = basisPoints, 
          FormattedBasisPoints = formattedBasisPoints,
          ImpliedFuturesPrice = impliedFuturesPrice, 
          HoldDays = holdDays, 
          FutureExpiry = futureExpiry,
          DividendImpact = dividendImpact, 
          DividendsToExpiry = dividendsToExpiry    
        });
    }

    private void OnTickString(int reqId, IBTickType tickType, string value)
    {
      if (TickString != null)
        TickString(this, new TWSTickStringEventArgs(this) {
          RequestId = reqId, 
          TickType = tickType, 
          Value = value
        });
    }

    private void OnCurrentTime(long time)
    {
      if (CurrentTime != null)
        CurrentTime(this, new TWSCurrentTimeEventArgs(this) { Time = DateTime.FromFileTimeUtc(time) });
    }

    private void OnRealtimeBar(int reqId, long time, double open, double high, double low, double close, long volume,
                               double wap, int count)
    {
      if (RealtimeBar != null)
        RealtimeBar(this, new TWSRealtimeBarEventArgs(this) {
          RequestId = reqId,
          Time = time, 
          Open = open, 
          High = high,
          Low = low, 
          Close = close, 
          Volume = volume, 
          Wap = wap, 
          Count = count
        });                      
    }


    private void OnTickGeneric(int reqId, IBTickType tickType, double value)
    {
      if (TickGeneric != null)
        TickGeneric(this, new TWSTickGenericEventArgs(this) {
          TickerId = reqId, 
          TickType = tickType, 
          Value = value,
        });


      TWSMarketDataSnapshot record;
      if (!_marketDataRecords.TryGetValue(reqId, out record)) {
        OnError(String.Format("OnTickPrice: Error request id {0}", reqId));
        return;
      }

      bool tickRecognized = false;
      switch (tickType) {
        case IBTickType.LastTimestamp:
          record.LastTimeStamp = DateTime.FromFileTime((long) value);
          tickRecognized = true;
          break;
      }

      if (tickRecognized)
        OnMarketData(record, tickType);
    }

    protected void OnOrderStatus(int orderId, string status, int filled, int remaining,
                                 double avgFillPrice, int permId, int parentId,
                                 double lastFillPrice, int clientId, string whyHeld)
    {
      if (OrderStatus != null)
        OrderStatus(this, new TWSOrderStatusEventArgs(this) {
           OrderId = orderId, 
           Status = status, 
           Filled = filled, 
           Remaining = remaining,
           AvgFillPrice = avgFillPrice, 
           PermId = permId, 
           ParentId = parentId, 
           LastFillPrice = lastFillPrice,
           ClientId = clientId,
           WhyHeld = whyHeld
        });
    }

    protected void OnOpenOrder(int orderId, IBOrder order, IBContract contract)
    {
      if (OpenOrder != null)
        OpenOrder(this, new TWSOpenOrderEventArgs(this) {
          OrderId = orderId, 
          Order = order, 
          Contract = contract            
        });
    }

    protected void OnBondContractDetails(IBContractDetails contract)
    {
      if (BondContractDetails != null)
        BondContractDetails(this, new TWSContractDetailsEventArgs(this) { ContractDetails = contract });
    }

    protected void OnContractDetails(IBContractDetails contract)
    {
      if (ContractDetails != null)
        ContractDetails(this, new TWSContractDetailsEventArgs(this) { ContractDetails = contract });
    }

    protected void OnManagedAccounts(string accountList) {}
    protected void OnReceiveFA(int faDataType, string xml) {}

    protected void OnScannerData(int reqId, int rank, IBContractDetails contract,
                                 string distance, string benchmark, string projection)
    {
      if (ScannerData == null)
        return;
      ScannerData(this, new TWSScannerDataEventArgs(this) {
          RequestId = reqId,
          Contract = contract, 
          Rank = rank,
          Distance = distance,          
          Benchmark = benchmark,
          Projection = projection,
        });
    }

    protected void OnScannerParameters(string xml)
    {
      if (ScannerParameters == null)
        return;
      ScannerParameters(this, new TWSScannerParametersEventArgs(this) { Xml = xml });
    }
    protected void OnUpdateAccountTime(string timestamp) {}
    protected void OnUpdateAccountValue(string key, string val, string cur, string accountName) {}
    protected void OnUpdateNewsBulletin(int newsMsgId, int newsMsgType, string newsMessage, string originatingExch) {}

    protected void OnUpdatePortfolio(IBContract contract, int position, double marketPrice, double marketValue,
                                     double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
    {
      if (UpdatePortfolio != null)
        UpdatePortfolio(this, new TWSUpdatePortfolioEventArgs(this) {
          Contract = contract, 
          Position = position, 
          MarketPrice = marketPrice, 
          MarketValue = marketValue, 
          AverageCost = averageCost,
          UnrealizedPnL = unrealizedPNL, 
          RealizedPnL = realizedPNL, 
          AccountName = accountName,
      });
    }

    protected void OnExecDetails(int orderId, IBContract contract, IBExecution execution)
    {
      if (ExecDetails != null)
        ExecDetails(this, new TWSExecDetailsEventArgs(this) {
          OrderId = orderId,
          Contract = contract,
          Execution = execution
        });
    }

    protected void OnMarketDepth(int reqId, int position, IBOperation operation, IBSide side, double price, int size)
    {
      if (MarketDepth != null)
        MarketDepth(this, new TWSMarketDepthEventArgs(this) {
          RequestId = reqId,
          Position = position,
          Operation = operation,
          Side = side,
          Price = price,
          Size = size,
          MarketMaker = String.Empty
        });
    }

    protected void OnMarketDepthL2(int reqId, int position, string marketMaker, IBOperation operation, IBSide side,
                                   double price, int size)
    {
      if (MarketDepthL2 != null)
        MarketDepth(this, new TWSMarketDepthEventArgs(this) {
          RequestId = reqId,
          Position = position,
          Operation = operation,
          Side = side,
          Price = price,
          Size = size,
          MarketMaker = marketMaker,
        });
    }

    protected void OnHistoricalData(int tickerId, TWSHistoricState state, DateTime date, double open, double high,
                                    double low, double close, int volume, double wap, bool hasGaps)
    {
      if (HistoricalData != null)
        HistoricalData(this, new TWSHistoricalDataEventArgs(this) {
          RequestId = tickerId,
          State = state,
          Date = date,
          Open = open,
          High = high,
          Low = low,
          Close = close,
          Volume = volume,
          WAP = wap,
          HasGaps = hasGaps,
        });
    }

    private void OnFundamentalData(int reqId, string data)
    {
    }
    private void OnContractDetailsEnd(int reqId)
    {
    }
    private void OnOpenOrderEnd()
    {
    }
    private void OnAccountDownloadEnd(string accountName)
    {
    }
    private void OnExecutionDataEnd(int reqId)
    {
    }
    private void OnTickSnapshotEnd(int reqId)
    {
    }
    private void OnDeltaNuetralValidation(int reqId, UnderliyingComponent underComp)
    {
    }


    #endregion

    #region Synchronized Request Wrappers

    public IBContractDetails GetContractDetails(IBContract contract)
    {
      var are = new AutoResetEvent(false);
      _internalDetailRequests.Add(contract, new KeyValuePair<AutoResetEvent, IBContractDetails>(are, null));
      RequestContractDetails(contract);
      WaitHandle.WaitAny(new WaitHandle[] {are}, DEFAULT_WAIT_TIMEOUT, false);
      var ret = _internalDetailRequests[contract].Value;
      _internalDetailRequests.Remove(contract);
      return ret;
    }

    #endregion

    #region Raw Server Mesage Processing

    private void ProcessTickPrice()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var tickType = (IBTickType) _enc.DecodeInt();
      var price = _enc.DecodeDouble();
      var size = (version >= 2) ? _enc.DecodeInt() : 0;
      var canAutoExecute = (version >= 3) ? _enc.DecodeInt() : 0;
      OnTickPrice(reqId, tickType, price, size, canAutoExecute);

      // Contorary to standard IB socket implementation
      // I will no go on with the supitidy of simulating TickSize
      // events when this client library is obviously written
      // to support the combined tick price + size messages
    }

    private void ProcessTickSize()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var tickType = _enc.DecodeEnum<IBTickType>();
      var size = _enc.DecodeInt();
      OnTickSize(reqId, tickType, size);
    }

    private void ProcessOrderStatus()
    {
      var version = _enc.DecodeInt();
      var orderId = _enc.DecodeInt();
      var status = _enc.DecodeString();
      var filled = _enc.DecodeInt();
      var remaining = _enc.DecodeInt();
      var avgFillPrice = _enc.DecodeDouble();

      var permId = (version >= 2) ? _enc.DecodeInt() : 0;
      var parentId = (version >= 3) ? _enc.DecodeInt() : 0;
      var lastFillPrice = (version >= 4) ? _enc.DecodeDouble() : 0;
      var clientId = (version >= 5) ? _enc.DecodeInt() : 0;
      var whyHeld = (version >= 6) ? _enc.DecodeString() : null;
      OnOrderStatus(orderId, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld);
    }

    private void ProcessErrMsg()
    {
      int version = _enc.DecodeInt();
      if (version < 2) {
        string message = _enc.DecodeString();
        OnError(message);
      }
      else {
        int id = _enc.DecodeInt();
        int errorCode = _enc.DecodeInt();
        string message = _enc.DecodeString();
        OnError(id, new TWSError(errorCode, message));
      }
    }

    private void ProcessOpenOrder()
    {     
      // read version
      var version = _enc.DecodeInt();
      var order = new IBOrder { OrderId = _enc.DecodeInt() };
      

      // read contract fields
      var contract = new IBContract {
          Symbol = _enc.DecodeString(),
          SecurityType = _enc.DecodeEnum<IBSecurityType>(),
          Expiry = DateTime.ParseExact(_enc.DecodeString(), IB_EXPIRY_DATE_FORMAT, CultureInfo.InvariantCulture),
          Strike = _enc.DecodeDouble(),
          Right = _enc.DecodeString(),
          Exchange = _enc.DecodeString(),
          Currency = _enc.DecodeString(),
          LocalSymbol = (version >= 2) ? _enc.DecodeString() : null
        };

      // read other order fields
      order.Action = _enc.DecodeEnum<IBAction>();
      order.TotalQuantity = _enc.DecodeInt();
      order.OrderType = _enc.DecodeEnum<IBOrderType>();
      order.LimitPrice = _enc.DecodeDouble();
      order.AuxPrice = _enc.DecodeDouble();
      order.Tif = _enc.DecodeEnum<IBTimeInForce>();
      order.OcaGroup = _enc.DecodeString();
      order.Account = _enc.DecodeString();
      order.OpenClose = _enc.DecodeString();
      order.Origin = _enc.DecodeInt();
      order.OrderRef = _enc.DecodeString();

      if (version >= 3)
        order.ClientId = _enc.DecodeInt();

      if (version >= 4) {
        order.PermId = _enc.DecodeInt();
        order.IgnoreRth = _enc.DecodeInt() == 1;
        order.Hidden = _enc.DecodeInt() == 1;
        order.DiscretionaryAmt = _enc.DecodeDouble();
      }

      if (version >= 5)
        order.GoodAfterTime = _enc.DecodeString();

      if (version >= 6)
        order.SharesAllocation = _enc.DecodeString();

      if (version >= 7) {
        order.FaGroup = _enc.DecodeString();
        order.FaMethod = _enc.DecodeString();
        order.FaPercentage = _enc.DecodeString();
        order.FaProfile = _enc.DecodeString();
      }

      if (version >= 8)
        order.GoodTillDate = _enc.DecodeString();

      if (version >= 9) {
        order.Rule80A = _enc.DecodeString();
        order.PercentOffset = _enc.DecodeDouble();
        order.SettlingFirm = _enc.DecodeString();
        order.ShortSaleSlot = _enc.DecodeInt();
        order.DesignatedLocation = _enc.DecodeString();
        order.AuctionStrategy = _enc.DecodeInt();
        order.StartingPrice = _enc.DecodeDouble();
        order.StockRefPrice = _enc.DecodeDouble();
        order.Delta = _enc.DecodeDouble();
        order.StockRangeLower = _enc.DecodeDouble();
        order.StockRangeUpper = _enc.DecodeDouble();
        order.DisplaySize = _enc.DecodeInt();
        order.RthOnly = _enc.DecodeBool();
        order.BlockOrder = _enc.DecodeBool();
        order.SweepToFill = _enc.DecodeBool();
        order.AllOrNone = _enc.DecodeBool();
        order.MinQty = _enc.DecodeInt();
        order.OcaType = _enc.DecodeEnum<IBOcaType>();
        order.ETradeOnly = _enc.DecodeBool();
        order.FirmQuoteOnly = _enc.DecodeBool();
        order.NbboPriceCap = _enc.DecodeDouble();
      }

      if (version >= 10) {
        order.ParentId = _enc.DecodeInt();
        order.TriggerMethod = _enc.DecodeInt();
      }

      if (version >= 11) {
        order.Volatility = _enc.DecodeDouble();
        order.VolatilityType = _enc.DecodeInt();
        if (version == 11) {
          int receivedInt = _enc.DecodeInt();
          order.DeltaNeutralOrderType = ((receivedInt == 0) ? IBOrderType.None : IBOrderType.Market);
        }
        else {
          // version 12 and up
          order.DeltaNeutralOrderType = _enc.DecodeEnum<IBOrderType>();
          order.DeltaNeutralAuxPrice = _enc.DecodeDouble();
        }
        order.ContinuousUpdate = _enc.DecodeInt();
        if (ServerInfo.Version == 26) {
          order.StockRangeLower = _enc.DecodeDouble();
          order.StockRangeUpper = _enc.DecodeDouble();
        }
        order.ReferencePriceType = _enc.DecodeInt();
      }

      if (version >= 13)
        order.TrailStopPrice = _enc.DecodeDouble();

      if (version >= 14) {
        order.BasisPoints = _enc.DecodeDouble();
        order.BasisPointsType = _enc.DecodeInt();
        contract.ComboLegsDescrip = _enc.DecodeString();
      }

      OnOpenOrder(order.OrderId, order, contract);
    }

    private void ProcessAccountValue()
    {
      var version = _enc.DecodeInt();
      var key = _enc.DecodeString();
      var val = _enc.DecodeString();
      var cur = _enc.DecodeString();
      string accountName;
      accountName = (version >= 2) ? _enc.DecodeString() : null;
      OnUpdateAccountValue(key, val, cur, accountName);
    }

    private void ProcessPortfolioValue()
    {
      var version = _enc.DecodeInt();
      var contractDetails = new IBContract {
          Symbol = _enc.DecodeString(),
          SecurityType = _enc.DecodeEnum<IBSecurityType>(),
          Expiry = DateTime.ParseExact(_enc.DecodeString(), IB_EXPIRY_DATE_FORMAT, CultureInfo.InvariantCulture),
          Strike = _enc.DecodeDouble(),
          Right = _enc.DecodeString(),
          Currency = _enc.DecodeString()
        };
      if (version >= 2)
        contractDetails.LocalSymbol = _enc.DecodeString();

      var position = _enc.DecodeInt();
      var marketPrice = _enc.DecodeDouble();
      var marketValue = _enc.DecodeDouble();
      var averageCost = 0.0;
      var unrealizedPnl = 0.0;
      var realizedPnl = 0.0;
      if (version >= 3) {
        averageCost = _enc.DecodeDouble();
        unrealizedPnl = _enc.DecodeDouble();
        realizedPnl = _enc.DecodeDouble();
      }

      string accountName = null;
      if (version >= 4)
        accountName = _enc.DecodeString();
      OnUpdatePortfolio(contractDetails, position, marketPrice, marketValue, averageCost, unrealizedPnl, realizedPnl,
                        accountName);
    }

    private void ProcessAcctUpdateTime()
    {
      var version = _enc.DecodeInt();
      var timeStamp = _enc.DecodeString();
      OnUpdateAccountTime(timeStamp);
    }

    private void ProcessNextValidId()
    {
      var version = _enc.DecodeInt();
      _nextValidId = _enc.DecodeInt();
    }

    private void ProcessContractData()
    {
      var version = _enc.DecodeInt();
      var contractDetails = new IBContractDetails
        {
          Summary = {
              Symbol = _enc.DecodeString(),
              SecurityType = _enc.DecodeEnum<IBSecurityType>(),
              Expiry = DateTime.ParseExact(_enc.DecodeString(), IB_EXPIRY_DATE_FORMAT, CultureInfo.InvariantCulture),
              Strike = _enc.DecodeDouble(),
              Right = _enc.DecodeString(),
              Exchange = _enc.DecodeString(),
              Currency = _enc.DecodeString(),
              LocalSymbol = _enc.DecodeString()
            },
          MarketName = _enc.DecodeString(),
          TradingClass = _enc.DecodeString(),
          Conid = _enc.DecodeInt(),
          MinTick = _enc.DecodeDouble(),
          Multiplier = _enc.DecodeString(),
          OrderTypes = _enc.DecodeString(),
          ValidExchanges = _enc.DecodeString()
        };
      if (version >= 2)
        contractDetails.PriceMagnifier = _enc.DecodeInt();
      OnContractDetails(contractDetails);
    }

    private void ProcessExecutionData()
    {
      var version = _enc.DecodeInt();
      var orderId = _enc.DecodeInt();
      var contract = new IBContract {
        Symbol = _enc.DecodeString(),
        SecurityType = _enc.DecodeEnum<IBSecurityType>(),
        Expiry = DateTime.ParseExact(_enc.DecodeString(), IB_EXPIRY_DATE_FORMAT, CultureInfo.InvariantCulture),
        Strike = _enc.DecodeDouble(),
        Right = _enc.DecodeString(),
        Exchange = _enc.DecodeString(),
        Currency = _enc.DecodeString(),
        LocalSymbol = _enc.DecodeString()
      };
      var execution = new IBExecution { 
        OrderID = orderId,
        ExecID = _enc.DecodeString(),
        Time = _enc.DecodeString(),
        AcctNumber = _enc.DecodeString(),
        Exchange = _enc.DecodeString(),
        Side = _enc.DecodeString(),
        Shares = _enc.DecodeInt(),
        Price = _enc.DecodeDouble()
      };
      if (version >= 2)
        execution.PermID = _enc.DecodeInt();
      if (version >= 3)
        execution.ClientID = _enc.DecodeInt();
      if (version >= 4)
        execution.Liquidation = _enc.DecodeInt();

      OnExecDetails(orderId, contract, execution);
    }

    private void ProcessMarketDepth()
    {
      var version = _enc.DecodeInt();
      var id = _enc.DecodeInt();
      var position = _enc.DecodeInt();
      var operation = _enc.DecodeEnum<IBOperation>();
      var side = _enc.DecodeEnum<IBSide>();
      var price = _enc.DecodeDouble();
      var size = _enc.DecodeInt();
      OnMarketDepth(id, position, operation, side, price, size);
    }

    private void ProcessMarketDepthL2()
    {
      var version = _enc.DecodeInt();
      var id = _enc.DecodeInt();
      var position = _enc.DecodeInt();
      var marketMaker = _enc.DecodeString();
      var operation = _enc.DecodeEnum<IBOperation>();
      var side = _enc.DecodeEnum<IBSide>();
      var price = _enc.DecodeDouble();
      var size = _enc.DecodeInt();
      OnMarketDepthL2(id, position, marketMaker, operation, side, price, size);
    }

    private void ProcessNewsBulletins()
    {
      var version = _enc.DecodeInt();
      var newsMsgId = _enc.DecodeInt();
      var newsMsgType = _enc.DecodeInt();
      var newsMessage = _enc.DecodeString();
      var originatingExch = _enc.DecodeString();
      OnUpdateNewsBulletin(newsMsgId, newsMsgType, newsMessage, originatingExch);
    }

    private void ProcessManagedAccts()
    {
      var version = _enc.DecodeInt();
      var accountsList = _enc.DecodeString();

      OnManagedAccounts(accountsList);
    }

    private void ProcessReceiveFA()
    {
      var version = _enc.DecodeInt();
      var faDataType = _enc.DecodeInt();
      var xml = _enc.DecodeString();

      OnReceiveFA(faDataType, xml);
    }

    private void ProcessHistoricalData()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var startDateTime = DateTime.Now;
      var endDateTime = startDateTime;
      if (version >= 2) {
        var startDateStr = _enc.DecodeString();
        var endDateStr = _enc.DecodeString();
        startDateTime = DateTime.ParseExact(startDateStr, IB_DATE_FORMAT, CultureInfo.InvariantCulture);
        endDateTime = DateTime.ParseExact(endDateStr, IB_DATE_FORMAT, CultureInfo.InvariantCulture);
      }
      OnHistoricalData(reqId, TWSHistoricState.Starting, startDateTime, -1, -1, -1, -1, -1, -1, false);
      var itemCount = _enc.DecodeInt();
      for (var i = 0; i < itemCount; i++) {
        var date = _enc.DecodeString();
        var dateTime = DateTime.ParseExact(date, IB_DATE_FORMAT, CultureInfo.InvariantCulture);
        var open = _enc.DecodeDouble();
        var high = _enc.DecodeDouble();
        var low = _enc.DecodeDouble();
        var close = _enc.DecodeDouble();
        var volume = _enc.DecodeInt();
        var WAP = _enc.DecodeDouble();
        var hasGaps = _enc.DecodeString();
        OnHistoricalData(reqId, TWSHistoricState.Downloading, dateTime, open, high, low, close, volume, WAP,
                         Boolean.Parse(hasGaps));
      }
      // Send end of dataset marker
      OnHistoricalData(reqId, TWSHistoricState.Finished, endDateTime, -1, -1, -1, -1, -1, -1, false);
    }

    private void ProcessBondContractData()
    {
      int version = _enc.DecodeInt();
      var contract = new IBContractDetails();

      contract.Summary.Symbol = _enc.DecodeString();
      contract.Summary.SecurityType = _enc.DecodeEnum<IBSecurityType>();
      contract.Summary.Cusip = _enc.DecodeString();
      contract.Summary.Coupon = _enc.DecodeDouble();
      contract.Summary.Maturity = _enc.DecodeString();
      contract.Summary.IssueDate = _enc.DecodeString();
      contract.Summary.Ratings = _enc.DecodeString();
      contract.Summary.BondType = _enc.DecodeString();
      contract.Summary.CouponType = _enc.DecodeString();
      contract.Summary.Convertible = _enc.DecodeBool();
      contract.Summary.Callable = _enc.DecodeBool();
      contract.Summary.Putable = _enc.DecodeBool();
      contract.Summary.DescAppend = _enc.DecodeString();
      contract.Summary.Exchange = _enc.DecodeString();
      contract.Summary.Currency = _enc.DecodeString();
      contract.MarketName = _enc.DecodeString();
      contract.TradingClass = _enc.DecodeString();
      contract.Conid = _enc.DecodeInt();
      contract.MinTick = _enc.DecodeDouble();
      contract.OrderTypes = _enc.DecodeString();
      contract.ValidExchanges = _enc.DecodeString();

      if (version >= 2) {
        contract.Summary.NextOptionDate = _enc.DecodeString();
        contract.Summary.NextOptionType = _enc.DecodeString();
        contract.Summary.NextOptionPartial = _enc.DecodeBool();
        contract.Summary.Notes = _enc.DecodeString();
      }
      OnBondContractDetails(contract);
    }

    private void ProcessScannerParameters()
    {
      var version = _enc.DecodeInt();
      var xml = _enc.DecodeString();
      OnScannerParameters(xml);
    }

    private void ProcessScannerData()
    {
      var contract = new IBContractDetails();
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var numberOfElements = _enc.DecodeInt();
      for (int i = 0; i < numberOfElements; i++) {
        var rank = _enc.DecodeInt();
        contract.Summary.Symbol = _enc.DecodeString();
        contract.Summary.SecurityType = _enc.DecodeEnum<IBSecurityType>();
        contract.Summary.Expiry = DateTime.ParseExact(_enc.DecodeString(), IB_EXPIRY_DATE_FORMAT,
                                                      CultureInfo.InvariantCulture);
        contract.Summary.Strike = _enc.DecodeDouble();
        contract.Summary.Right = _enc.DecodeString();
        contract.Summary.Exchange = _enc.DecodeString();
        contract.Summary.Currency = _enc.DecodeString();
        contract.Summary.LocalSymbol = _enc.DecodeString();
        contract.MarketName = _enc.DecodeString();
        contract.TradingClass = _enc.DecodeString();
        var distance = _enc.DecodeString();
        var benchmark = _enc.DecodeString();
        var projection = _enc.DecodeString();
        OnScannerData(reqId, rank, contract, distance, benchmark, projection);
      }
    }

    private void ProcessTickOptionComputation()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var tickType = _enc.DecodeEnum<IBTickType>();
      var impliedVol = _enc.DecodeDouble();
      // -1 is the "not yet computed" indicator
      if (impliedVol < 0)
        impliedVol = Double.MaxValue;
      var delta = _enc.DecodeDouble();
      // -2 is the "not yet computed" indicator
      if (Math.Abs(delta) > 1)
        delta = Double.MaxValue;
      double modelPrice, pvDividend;
      // introduced in version == 5
      if (tickType == IBTickType.ModelOption) {
        modelPrice = _enc.DecodeDouble();
        pvDividend = _enc.DecodeDouble();
      }
      else
        modelPrice = pvDividend = Double.MaxValue;

      OnTickOptionComputation(reqId, tickType, impliedVol, delta, modelPrice, pvDividend);
    }

    private void ProcessTickEFP()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var tickType = _enc.DecodeEnum<IBTickType>();
      var basisPoints = _enc.DecodeDouble();
      var formattedBasisPoints = _enc.DecodeString();
      var impliedFuturesPrice = _enc.DecodeDouble();
      var holdDays = _enc.DecodeInt();
      var futureExpiry = _enc.DecodeString();
      var dividendImpact = _enc.DecodeDouble();
      var dividendsToExpiry = _enc.DecodeDouble();
      OnTickEFP(reqId, tickType, basisPoints, formattedBasisPoints,
                impliedFuturesPrice, holdDays, futureExpiry, dividendImpact, dividendsToExpiry);
    }


    private void ProcessTickString()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var tickType = _enc.DecodeEnum<IBTickType>();
      var value = _enc.DecodeString();

      OnTickString(reqId, tickType, value);
    }

    private void ProcessTickGeneric()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var tickType = _enc.DecodeEnum<IBTickType>();
      var value = _enc.DecodeDouble();
      OnTickGeneric(reqId, tickType, value);
    }

    private void ProcessCurrentTime()
    {
      var version = _enc.DecodeInt();
      var time = _enc.DecodeLong();
      OnCurrentTime(time);
    }

    private void ProcessRealTimeBars()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var time = _enc.DecodeLong();
      var open = _enc.DecodeDouble();
      var high = _enc.DecodeDouble();
      var low = _enc.DecodeDouble();
      var close = _enc.DecodeDouble();
      var volume = _enc.DecodeLong();
      var wap = _enc.DecodeDouble();
      var count = _enc.DecodeInt();
      OnRealtimeBar(reqId, time, open, high, low, close, volume, wap, count);
    }


    private void ProcessFundamentalData()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var data = _enc.DecodeString();
      OnFundamentalData(reqId, data);
    }

    private void ProcessContractDataEnd()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      OnContractDetailsEnd(reqId);
    }


    private void ProcessOpenOrderEnd()
    {
      var version = _enc.DecodeInt();
      OnOpenOrderEnd();

    }


    private void ProcessAccountDownloadEnd()
    {
      var version = _enc.DecodeInt();
      var accountName = _enc.DecodeString();
      OnAccountDownloadEnd(accountName);
    }


    private void ProcessExecutionDataEnd()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      OnExecutionDataEnd(reqId);

    }


    private void ProcessDeltaNeutralValidation()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();

      var underComp = new UnderliyingComponent {
        ConId = _enc.DecodeInt(),
        Delta = _enc.DecodeDouble(),
        Price = _enc.DecodeDouble()
      };

      OnDeltaNuetralValidation(reqId, underComp);
    }


    private void ProcessTickSnapshotEnd()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();

      OnTickSnapshotEnd(reqId);
    }



    private void ProcessMessages()
    {
      try {
        while (_doWork) {
          if (!ProcessSingleMessage())
            return;
        }
      }
      catch (Exception e) {
        OnError(e.ToString());
        return;
      }
      finally {
        Disconnect();
      }
    }

    protected bool ProcessSingleMessage()
    {
      var msgCode = _enc.DecodeEnum<ClientMessage>();

      // Can't process this
      if (msgCode == ClientMessage.Error)
        return false;

      switch (msgCode) {
        case ClientMessage.TickPrice:              ProcessTickPrice();              break;
        case ClientMessage.TickSize:               ProcessTickSize();               break;
        case ClientMessage.TickOptionComputation:  ProcessTickOptionComputation();  break;
        case ClientMessage.TickGeneric:            ProcessTickGeneric();            break;
        case ClientMessage.TickString:             ProcessTickString();             break;
        case ClientMessage.TickEfp:                ProcessTickEFP();                break;
        case ClientMessage.OrderStatus:            ProcessOrderStatus();            break;
        case ClientMessage.ErrorMessage:           ProcessErrMsg();                 break;
        case ClientMessage.OpenOrder:              ProcessOpenOrder();              break;
        case ClientMessage.AccountValue:           ProcessAccountValue();           break;
        case ClientMessage.PortfolioValue:         ProcessPortfolioValue();         break;
        case ClientMessage.AccountUpdateTime:      ProcessAcctUpdateTime();         break;
        case ClientMessage.NextValidId:            ProcessNextValidId();            break;
        case ClientMessage.ContractData:           ProcessContractData();           break;
        case ClientMessage.ExecutionData:          ProcessExecutionData();          break;
        case ClientMessage.MarketDepth:            ProcessMarketDepth();            break;
        case ClientMessage.MarketDepthL2:          ProcessMarketDepthL2();          break;
        case ClientMessage.NewsBulletins:          ProcessNewsBulletins();          break;
        case ClientMessage.ManagedAccounts:        ProcessManagedAccts();           break;
        case ClientMessage.ReceiveFA:              ProcessReceiveFA();              break;
        case ClientMessage.HistoricalData:         ProcessHistoricalData();         break;
        case ClientMessage.BondContractData:       ProcessBondContractData();       break;
        case ClientMessage.ScannerParameters:      ProcessScannerParameters();      break;
        case ClientMessage.ScannerData:            ProcessScannerData();            break;
        case ClientMessage.CurrentTime:            ProcessCurrentTime();            break;
        case ClientMessage.RealTimeBars:           ProcessRealTimeBars();           break;
        case ClientMessage.FundamentalData:        ProcessFundamentalData();        break;
        case ClientMessage.ContractDataEnd:        ProcessContractDataEnd();        break;
        case ClientMessage.OpenOrderEnd:           ProcessOpenOrderEnd();           break;        
        case ClientMessage.AccountDownloadEnd:     ProcessAccountDownloadEnd();     break;
        case ClientMessage.ExecutionDataEnd:       ProcessExecutionDataEnd();       break;
        case ClientMessage.DeltaNuetralValidation: ProcessDeltaNeutralValidation(); break;
        case ClientMessage.TickSnapshotEnd:        ProcessTickSnapshotEnd();        break;
      }

      // All is well
      return true;
    }


    #endregion

    #region Request Methods

    public virtual int PlaceOrder(IBContract contract, IBOrder order)
    {
      lock (this) {
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return -1;
        }

        const int reqVersion = 20;
        var orderId = NextValidId;
        try {
          _enc.Encode(ServerMessage.PlaceOrder);
          _enc.Encode(reqVersion);
          _enc.Encode(orderId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType.ToString());
          _enc.Encode(contract.Expiry.ToString(IB_EXPIRY_DATE_FORMAT));
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          if (ServerInfo.Version >= 15)
            _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          if (ServerInfo.Version >= 14)
            _enc.Encode(contract.PrimaryExch);
          _enc.Encode(contract.Currency);
          if (ServerInfo.Version >= 2)
            _enc.Encode(contract.LocalSymbol);
          _enc.Encode(order.Action);
          _enc.Encode(order.TotalQuantity);
          _enc.Encode(order.OrderType);
          _enc.Encode(order.LimitPrice);
          _enc.Encode(order.AuxPrice);
          _enc.Encode(order.Tif);
          _enc.Encode(order.OcaGroup);
          _enc.Encode(order.Account);
          _enc.Encode(order.OpenClose);
          _enc.Encode(order.Origin);
          _enc.Encode(order.OrderRef);
          _enc.Encode(order.Transmit);
          if (ServerInfo.Version >= 4)
            _enc.Encode(order.ParentId);
          if (ServerInfo.Version >= 5) {
            _enc.Encode(order.BlockOrder);
            _enc.Encode(order.SweepToFill);
            _enc.Encode(order.DisplaySize);
            _enc.Encode(order.TriggerMethod);
            _enc.Encode(order.IgnoreRth);
          }
          if (ServerInfo.Version >= 7)
            _enc.Encode(order.Hidden);
          if ((ServerInfo.Version >= 8) && (contract.SecurityType == IBSecurityType.Bag)) {
            _enc.Encode(contract.ComboLegs.Count);
            foreach (var leg in contract.ComboLegs) {
              _enc.Encode(leg.ConId);
              _enc.Encode(leg.Ratio);
              _enc.Encode(leg.Action);
              _enc.Encode(leg.Exchange);
              _enc.Encode(leg.OpenClose);
            }
          }
          if (ServerInfo.Version >= 9)
            _enc.Encode(order.SharesAllocation);
          if (ServerInfo.Version >= 10)
            _enc.Encode(order.DiscretionaryAmt);
          if (ServerInfo.Version >= 11)
            _enc.Encode(order.GoodAfterTime);
          if (ServerInfo.Version >= 12)
            _enc.Encode(order.GoodTillDate);
          if (ServerInfo.Version >= 13) {
            _enc.Encode(order.FaGroup);
            _enc.Encode(order.FaMethod);
            _enc.Encode(order.FaPercentage);
            _enc.Encode(order.FaProfile);
          }
          if (ServerInfo.Version >= 18) {
            _enc.Encode(order.ShortSaleSlot);
            _enc.Encode(order.DesignatedLocation);
          }
          if (ServerInfo.Version >= 19) {
            _enc.Encode(order.OcaType);
            _enc.Encode(order.RthOnly);
            _enc.Encode(order.Rule80A);
            _enc.Encode(order.SettlingFirm);
            _enc.Encode(order.AllOrNone);
            _enc.EncodeMax(order.MinQty);
            _enc.EncodeMax(order.PercentOffset);
            _enc.Encode(order.ETradeOnly);
            _enc.Encode(order.FirmQuoteOnly);
            _enc.EncodeMax(order.NbboPriceCap);
            _enc.EncodeMax(order.AuctionStrategy);
            _enc.EncodeMax(order.StartingPrice);
            _enc.EncodeMax(order.StockRefPrice);
            _enc.EncodeMax(order.Delta);
            var stockRangeLower = ((ServerInfo.Version == 26) && (order.OrderType == IBOrderType.Volatility))
                                       ? Double.MaxValue
                                       : order.StockRangeLower;
            var stockRangeUpper = ((ServerInfo.Version == 26) && (order.OrderType == IBOrderType.Volatility))
                                       ? Double.MaxValue
                                       : order.StockRangeUpper;
            _enc.EncodeMax(stockRangeLower);
            _enc.EncodeMax(stockRangeUpper);
            if (ServerInfo.Version >= 22) {
              _enc.Encode(order.OverridePercentageConstraints);
            }
            if (ServerInfo.Version >= 26) {
              _enc.EncodeMax(order.Volatility);
              _enc.EncodeMax(order.VolatilityType);
              if (ServerInfo.Version < 28) {
                _enc.Encode(order.DeltaNeutralOrderType == IBOrderType.Market);
              }
              else {
                _enc.Encode(order.DeltaNeutralOrderType);
                _enc.EncodeMax(order.DeltaNeutralAuxPrice);
              }
              _enc.Encode(order.ContinuousUpdate);
              if (ServerInfo.Version == 26) {
                if (order.OrderType == IBOrderType.Volatility) {
                  _enc.EncodeMax(order.StockRangeLower);
                  _enc.EncodeMax(order.StockRangeUpper);
                }
                else {
                  _enc.EncodeMax(Double.MaxValue);
                  _enc.EncodeMax(Double.MaxValue);
                }
              }
              _enc.EncodeMax(order.ReferencePriceType);
            }
          }
        }
        catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_ORDER);
          OnError(e.Message);
          orderId = -1;
          Disconnect();
        }

        _orderRecords.Add(orderId, new OrderRecord(order, contract));
        return orderId;
      }
    }

    public virtual void ExerciseOptions(int reqId, IBContract contract,
                                        int exerciseAction, int exerciseQuantity,
                                        string account, int overrideOrder)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      try {
        if (ServerInfo.Version < 21) {
          OnError(TWSErrors.UPDATE_TWS);
          return;
        }

        _enc.Encode(ServerMessage.ExerciseOptions);
        _enc.Encode(reqVersion);
        _enc.Encode(reqId);
        _enc.Encode(contract.Symbol);
        _enc.Encode(contract.SecurityType.ToString());
        _enc.Encode(contract.Expiry.ToString(IB_DATE_FORMAT));
        _enc.Encode(contract.Strike);
        _enc.Encode(contract.Right);
        _enc.Encode(contract.Multiplier);
        _enc.Encode(contract.Exchange);
        _enc.Encode(contract.Currency);
        _enc.Encode(contract.LocalSymbol);
        _enc.Encode(exerciseAction);
        _enc.Encode(exerciseQuantity);
        _enc.Encode(account);
        _enc.Encode(overrideOrder);
      }
      catch (Exception e) {
        OnError(reqId, TWSErrors.FAIL_SEND_REQMKT);
        OnError(e.Message);
        Disconnect();
      }
    }


    public virtual void RequestServerLogLevelChange(int logLevel)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      // send the set server logging level message
      try {
        _enc.Encode(ServerMessage.SetServerLogLevel);
        _enc.Encode(reqVersion);
        _enc.Encode(logLevel);
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_SERVER_LOG_LEVEL);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual int RequestHistoricalData(IBContract contract, string endDateTime,
                                             string durationStr, int barSizeSetting,
                                             string whatToShow, int useRTH, int formatDate)
    {
      lock (this) {
        // not connected?
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return -1;
        }

        const int reqVersion = 3;
        var reqId = NextValidId;

        try {
          if (ServerInfo.Version < 16) {
            OnError(TWSErrors.UPDATE_TWS);
            return -1;
          }

          _enc.Encode(ServerMessage.RequestHistoricalData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.ToString(IB_EXPIRY_DATE_FORMAT));
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.PrimaryExch);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version >= 20) {
            _enc.Encode(endDateTime);
            _enc.Encode(barSizeSetting);
          }
          _enc.Encode(durationStr);
          _enc.Encode(useRTH);
          _enc.Encode(whatToShow);
          if (ServerInfo.Version > 16) {
            _enc.Encode(formatDate);
          }
          if (IBSecurityType.Bag == contract.SecurityType) {
            if (contract.ComboLegs == null) {
              _enc.Encode(0);
            }
            else {
              _enc.Encode(contract.ComboLegs.Count);

              IBComboLeg comboLeg;
              for (int i = 0; i < contract.ComboLegs.Count; i++) {
                comboLeg = contract.ComboLegs[i];
                _enc.Encode(comboLeg.ConId);
                _enc.Encode(comboLeg.Ratio);
                _enc.Encode(comboLeg.Action);
                _enc.Encode(comboLeg.Exchange);
              }
            }
          }
        }
        catch (Exception e) {
          OnError(reqId, TWSErrors.FAIL_SEND_REQHISTDATA);
          OnError(e.Message);
          Disconnect();
        }
        return reqId;
      }
    }

    public virtual int RequestMarketData(IBContract contract, IList<IBGenericTickType> genericTickList)
    {
      lock (this) {
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return -1;
        }
        const int reqVersion = 5;
        var reqId = NextValidId;
        Debug.WriteLine(String.Format("REQ: {0}", reqId));
        try {
          _enc.Encode(ServerMessage.RequestMarketData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.ToString(IB_EXPIRY_DATE_FORMAT));
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          if (ServerInfo.Version >= 15)
            _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          if (ServerInfo.Version >= 14)
            _enc.Encode(contract.PrimaryExch);
          _enc.Encode(contract.Currency);
          if (ServerInfo.Version >= 2)
            _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version >= 8 && (contract.SecurityType == IBSecurityType.Bag)) {
            if (contract.ComboLegs == null)
              _enc.Encode(0);
            else {
              _enc.Encode(contract.ComboLegs.Count);
              foreach (IBComboLeg leg in contract.ComboLegs) {
                _enc.Encode(leg.ConId);
                _enc.Encode(leg.Ratio);
                _enc.Encode(leg.Action);
                _enc.Encode(leg.Exchange);
              }
            }
          }
          if (ServerInfo.Version >= 31) {
            var sb = new StringBuilder();
            if (genericTickList != null) {
              foreach (IBGenericTickType tick in genericTickList)
                sb.Append((int) tick).Append(',');
              sb.Remove(sb.Length - 2, 1);
            }
            _enc.Encode(sb.ToString());
          }

          // If we got to here without choking on something
          // we update the request registry
          _marketDataRecords.Add(reqId, new TWSMarketDataSnapshot(contract));
        }
        catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_REQMKT);
          OnError(e.Message);
          reqId = -1;
          Disconnect();
        }
        return reqId;
      }
    }

    public virtual void RequestRealTimeBars(int reqId, IBContract contract,
                                            int barSize, string whatToShow, bool useRTH)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }
      if (ServerInfo.Version < 34) {
        OnError(TWSErrors.UPDATE_TWS);
        return;
      }

      const int reqVersion = 1;

      try {
        // send req mkt data msg
        _enc.Encode(ServerMessage.RequestRealTimeBars);
        _enc.Encode(reqVersion);
        _enc.Encode(reqId);

        _enc.Encode(contract.Symbol);
        _enc.Encode(contract.SecurityType);
        _enc.Encode(contract.Expiry.ToString(IB_EXPIRY_DATE_FORMAT));
        _enc.Encode(contract.Strike);
        _enc.Encode(contract.Right);
        _enc.Encode(contract.Multiplier);
        _enc.Encode(contract.Exchange);
        _enc.Encode(contract.PrimaryExch);
        _enc.Encode(contract.Currency);
        _enc.Encode(contract.LocalSymbol);
        _enc.Encode(barSize);
        _enc.Encode(whatToShow);
        _enc.Encode(useRTH);
      }
      catch (Exception e) {
        OnError(reqId, TWSErrors.FAIL_SEND_REQRTBARS);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual int RequestMarketDepth(IBContract contract, int numRows)
    {
      lock (this) {
        if (!IsConnected) {
          OnError(TWSErrors.NOT_CONNECTED);
          return -1;
        }
        if (ServerInfo.Version < 6) {
          OnError(TWSErrors.UPDATE_TWS);
          return -1;
        }
        const int reqVersion = 3;
        var reqId = NextValidId;
        try {
          _enc.Encode(ServerMessage.RequestMarketDepth);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.ToString(IB_EXPIRY_DATE_FORMAT));
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          if (ServerInfo.Version >= 15)
            _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version >= 19)
            _enc.Encode(numRows);
        }
        catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_REQMKTDEPTH);
          OnError(e.Message);
          reqId = -1;
          Disconnect();
        }
        return reqId;
      }
    }

    public virtual void RequestAutoOpenOrders(bool autoBind)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      // send req open orders msg
      try {
        _enc.Encode(ServerMessage.RequestAutoOpenOrders);
        _enc.Encode(reqVersion);
        _enc.Encode(autoBind);
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_OORDER);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestIds(int numIds)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      try {
        _enc.Encode(ServerMessage.RequestIds);
        _enc.Encode(reqVersion);
        _enc.Encode(numIds);
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_CORDER);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestOpenOrders()
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      // send cancel order msg
      try {
        _enc.Encode(ServerMessage.RequestOpenOrders);
        _enc.Encode(reqVersion);
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_OORDER);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestAccountUpdates(bool subscribe, string acctCode)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 2;

      // send cancel order msg
      try {
        _enc.Encode(ServerMessage.RequestAccountData);
        _enc.Encode(reqVersion);
        _enc.Encode(subscribe);

        // Send the account code. This will only be used for FA clients
        if (ServerInfo.Version >= 9) {
          _enc.Encode(acctCode);
        }
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_ACCT);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestAllOpenOrders()
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      // send req all open orders msg
      try {
        _enc.Encode(ServerMessage.RequestAllOpenOrders);
        _enc.Encode(reqVersion);
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_OORDER);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestExecutions(IBExecutionFilter filter)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 2;

      // send cancel order msg
      try {
        _enc.Encode(ServerMessage.RequestExecutions);
        _enc.Encode(reqVersion);

        // Send the execution rpt filter data
        if (ServerInfo.Version >= 9) {
          _enc.Encode(filter.ClientId);
          _enc.Encode(filter.AcctCode);

          // Note that the valid format for m_time is "yyyymmdd-hh:mm:ss"
          _enc.Encode(filter.DateTime.ToString(IB_DATE_FORMAT));
          _enc.Encode(filter.Symbol);
          _enc.Encode(filter.SecurityType);
          _enc.Encode(filter.Exchange);
          _enc.Encode(filter.Side);
        }
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_EXEC);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestNewsBulletins(bool allMsgs)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      const int reqVersion = 1;

      try {
        _enc.Encode(ServerMessage.RequestNewsBulletins);
        _enc.Encode(reqVersion);
        _enc.Encode(allMsgs);
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_CORDER);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestContractDetails(IBContract contract)
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      // This feature is only available for versions of TWS >=4
      if (ServerInfo.Version < 4) {
        OnError(TWSErrors.UPDATE_TWS);
        return;
      }

      const int reqVersion = 3;

      try {
        // send req mkt data msg
        _enc.Encode(ServerMessage.RequestContractData);
        _enc.Encode(reqVersion);

        _enc.Encode(contract.Symbol);
        _enc.Encode(contract.SecurityType);
        _enc.Encode(contract.Expiry.ToString(IB_EXPIRY_DATE_FORMAT));
        _enc.Encode(contract.Strike);
        _enc.Encode(contract.Right);
        if (ServerInfo.Version >= 15) {
          _enc.Encode(contract.Multiplier);
        }
        _enc.Encode(contract.Exchange);
        _enc.Encode(contract.Currency);
        _enc.Encode(contract.LocalSymbol);
        if (ServerInfo.Version >= 31) {
          _enc.Encode(contract.IncludeExpired);
        }
      }
      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_REQCONTRACT);
        OnError(e.Message);
        Disconnect();
      }
    }

    public virtual void RequestCurrentTime()
    {
      // not connected?
      if (!IsConnected) {
        OnError(TWSErrors.NOT_CONNECTED);
        return;
      }

      // This feature is only available for versions of TWS >= 33
      if (ServerInfo.Version < 33) {
        OnError(TWSErrors.UPDATE_TWS);
        return;
      }

      const int reqVersion = 1;

      try {
        _enc.Encode(ServerMessage.RequestCurrentTime);
        _enc.Encode(reqVersion);
      }

      catch (Exception e) {
        OnError(TWSErrors.FAIL_SEND_REQCURRTIME);
        OnError(e.Message);
        Disconnect();
      }
    }

    #endregion

    #region Utilities

    private Stream SetupDefaultRecordStream()
    {
      var count = 1;
      Stream s = null;
      while (true) {
        try {
          string name = "ib-log-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + count + ".log";
          s = File.Open(name, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
          break;
        }
        catch (IOException e) {
          count++;
        }
      }
      return s;
    }

    #endregion

    private int NextValidId
    {
      get { return _nextValidId++; }
    }

    public bool IsConnected
    {
      get { return Status == TWSClientStatus.Connected; }
    }

    public TWSClientStatus Status { get; private set; }

    public bool RecordForPlayback
    {
      get { return _recordForPlayback; }
      set
      {
        if (IsConnected && _recordForPlayback != value)
          throw new InvalidOperationException("Cannot set the RecordForPlayback property while connected");

        _recordForPlayback = value;
      }
    }

    public Stream RecordStream
    {
      get { return _recordStream; }
      set
      {
        if (IsConnected && _recordStream != value)
          throw new InvalidOperationException("Cannot set the RecordForPlayback property while connected");

        _recordStream = value;
      }
    }

    public IPEndPoint EndPoint
    {
      get { return _endPoint; }
      set
      {
        if (IsConnected)
          throw new Exception("Client already connected, cannot set the EndPoint");
        _endPoint = value;
      }
    }

    public TWSClientSettings Settings
    {
      get { return _settings; }
      set
      {
        if (!IsConnected)
          _settings = value;
      }
    }

    public TWSClientInfo ClientInfo { get; private set; }
    public TWSServerInfo ServerInfo { get; private set; }
    public event EventHandler<TWSClientStatusEventArgs> StatusChanged;
    public event EventHandler<TWSClientErrorEventArgs> Error;
    public event EventHandler<TWSTickPriceEventArgs> TickPrice;
    public event EventHandler<TWSTickSizeEventArgs> TickSize;
    public event EventHandler<TWSTickStringEventArgs> TickString;
    public event EventHandler<TWSTickGenericEventArgs> TickGeneric;
    public event EventHandler<TWSTickOptionComputationEventArgs> TickOptionComputation;
    public event EventHandler<TWSTickEFPEventArgs> TickEFP;
    public event EventHandler<TWSCurrentTimeEventArgs> CurrentTime;
    public event EventHandler<TWSOrderStatusEventArgs> OrderStatus;
    public event EventHandler<TWSOpenOrderEventArgs> OpenOrder;
    public event EventHandler<TWSContractDetailsEventArgs> BondContractDetails;
    public event EventHandler<TWSContractDetailsEventArgs> ContractDetails;
    public event EventHandler<TWSScannerDataEventArgs> ScannerData;
    public event EventHandler<TWSScannerParametersEventArgs> ScannerParameters;
    public event EventHandler<TWSUpdatePortfolioEventArgs> UpdatePortfolio;
    public event EventHandler<TWSExecDetailsEventArgs> ExecDetails;
    public event EventHandler<TWSMarketDepthEventArgs> MarketDepth;
    public event EventHandler<TWSMarketDepthEventArgs> MarketDepthL2;
    public event EventHandler<TWSHistoricalDataEventArgs> HistoricalData;
    public event EventHandler<TWSMarketDataEventArgs> MarketData;
    public event EventHandler<TWSRealtimeBarEventArgs> RealtimeBar;
    
  }
}