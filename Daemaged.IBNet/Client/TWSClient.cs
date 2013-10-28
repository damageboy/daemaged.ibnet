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
#endregion Copyright (c) 2007 by Dan Shechter

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#if NET_4_5
using System.Threading.Tasks;
using Daemaged.IBNet.Util;
#endif

namespace Daemaged.IBNet.Client
{
  /// <summary>
  /// Implements a full TWS Api client
  /// </summary>
  public class TWSClient
  {
    public const string DEFAULT_HOST = "127.0.0.1";
    public const int DEFAULT_PORT = 7496;
    private const int DEFAULT_WAIT_TIMEOUT = 10000;
    private const string IB_DATE_FORMAT = "yyyyMMdd  HH:mm:ss";
    private const string IB_EXPIRY_DATE_FORMAT = "yyyyMMdd";
    private const string IB_HISTORICAL_COMPLETED = "finished";
    private readonly Dictionary<int, OrderRecord> _orderRecords = new Dictionary<int, OrderRecord>();
#if NET_4_5
    private readonly IDictionary<int, IFaultable> _asyncCalls = new ConcurrentDictionary<int, IFaultable>();
#endif
    private int _clientId;
    private bool _doWork;
    protected ITWSEncoding _enc;
    private IPEndPoint _endPoint;
    protected Dictionary<int, TWSMarketDataSnapshot> _marketDataRecords = new Dictionary<int, TWSMarketDataSnapshot>();
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
    private object _socketLock = new object();

    #region Constructors

    /// <summary>
    /// Construct a new TWSClient object for connecting to IB's Trader WorkStation
    /// </summary>
    public TWSClient()
    {
      _tcpClient = null;
      _stream = null;
      _thread = null;
      Status = TWSClientStatus.Unknown;
      _twsTime = String.Empty;
      _nextValidId = 0;

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
      var address = Dns.GetHostEntry(host).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
      if (address == null)
        throw new ArgumentException(string.Format("could not resolve host {0}", host), "host");
      _endPoint = new IPEndPoint(address, port);
    }
    #endregion Constructors

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
          _tcpClient.NoDelay = true;
          _tcpClient.Connect(_endPoint);

          lock (_socketLock) {
            if (RecordForPlayback) {
              if (_recordStream == null)
                _recordStream = SetupDefaultRecordStream();

              _enc = new TWSPlaybackRecorderEncoding(new BufferedReadStream(_tcpClient.GetStream()), _recordStream);
            }
            else
              _enc = new TWSEncoding(new BufferedReadStream(_tcpClient.GetStream()));

            _enc.Encode(ClientInfo);
            _enc.Flush();
            _doWork = true;

            // Only create a reader thread if this Feed IS NOT reconnecting
            if (!_reconnect) {
              _thread = new Thread(ProcessMessages) {
                  IsBackground = true,
                  Name = "IBReader"
                };
            }
            // Get the server version
            ServerInfo = _enc.DecodeServerInfo();
            if (ServerInfo.Version >= 20)
            {
              _twsTime = _enc.DecodeString();
            }

            if (ServerInfo.Version < TWSServerInfo.SERVER_VERSION) {
              Disconnect();
              throw new Exception("Server version is too low, please update the TWS server");
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
          }

          // Only start the thread if this Feed IS NOT reconnecting
          if (!_reconnect)
            _thread.Start();

          _clientId = clientId;
          OnStatusChanged(Status = TWSClientStatus.Connected);
        }
        catch (Exception e) {
          Disconnect();
          throw;
        }
      }
    }

    /// <summary>
    /// Disconnect from the IB Trader Workstation endpoint
    /// </summary>
    public void Disconnect()
    {
      lock (this) {
        if (!IsConnected)
          return;
        lock (_socketLock) {
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
    }

    /// <summary>
    /// Reconnect to the IB Trader Workstation, re-register all markert data requests
    /// </summary>
    public void Reconnect()
    {
      lock (this) {
        if (!IsConnected)
          return;

        _reconnect = true;
        Disconnect();
        Connect(_clientId);
      }
    }

    #endregion Connect/Disconnect

    #region Cancel Messages

    /// <summary>
    /// Cancel a registered scanner subscription
    /// </summary>
    /// <param name="reqId">The scanner subscription request id</param>
    public void CancelScannerSubscription(int reqId)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < 24)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        // Send cancel mkt data msg
        try {
          _enc.Encode(ServerMessage.CancelScannerSubscription);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    /// <summary>
    /// Cancel historical data subscription
    /// </summary>
    /// <param name="reqId">The historical data subscription request id</param>
    public void CancelHistoricalData(int reqId)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < 24)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        // Send cancel mkt data msg
        try {
          _enc.Encode(ServerMessage.CancelHistoricalData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelMarketData(int reqId)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();
        const int reqVersion = 1;
        try {
          _enc.Encode(ServerMessage.CancelMarketData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelMarketDepth(int reqId)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();
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
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelNewsBulletins()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        // Send cancel order msg
        try {
          _enc.Encode(ServerMessage.CancelNewsBulletins);
          _enc.Encode(reqVersion);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    /// <summary>
    /// Cancels an order previously submitted through the client to TWS
    /// </summary>
    /// <param name="orderId">The order id, normally obtained with <see cref="PlaceOrder"/></param>
    /// <exception cref="Daemaged.IBNet.Client.NotConnectedException"></exception>
    public void CancelOrder(int orderId)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();
        const int reqVersion = 1;
        try {
          _enc.Encode(ServerMessage.CancelOrder);
          _enc.Encode(reqVersion);
          _enc.Encode(orderId);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelRealTimeBars(int reqId)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_REAL_TIME_BARS) {
          OnError(TWSErrors.UPDATE_TWS);
          return;
        }

        const int reqVersion = 1;

        // send cancel mkt data msg
        try {
          _enc.Encode(ServerMessage.CancelHistoricalData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    #endregion Cancel Messages

    #region Event Notifiers

    protected virtual void OnStatusChanged(TWSClientStatus status)
    {
      if (StatusChanged == null) return;
      StatusChanged(this, new TWSClientStatusEventArgs(this, status));
    }

    protected virtual void OnException(Exception e)
    {
      if (ExceptionOccured == null)
        return;
      ExceptionOccured(this, new TWSClientExceptionEventArgs(this) {
        Exception = e,
      });
    }

    protected virtual void OnError(TWSError error)
    {
      OnError(TWSErrors.NO_VALID_ID, error);
    }

    protected virtual void OnError(int reqId, TWSError error, string extraMessage = null)
    {
      if (Error != null) {
        TWSMarketDataSnapshot snapshot;
        IBContract contract = null;

        if (_marketDataRecords.TryGetValue(reqId, out snapshot))
          contract = snapshot.Contract;

        Error(this, new TWSClientErrorEventArgs(this) {
          RequestId = reqId,
          Contract = contract,
          Error = error,
          Message = extraMessage,
        });
      }

      if (OrderChanged != null) {
        OrderRecord or;
        if (_orderRecords.TryGetValue(reqId, out or)) {
          OrderChanged(this, new TWSOrderChangedEventArgs(this, or) {
            ChangeType = IBOrderChangeType.Error,
            Error = error,
          });
        }
      }
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
      //Console.WriteLine("Client: Received tick price msg for reqid " + reqId + ", symbol " + record.Contract.Symbol +
      //                  ", price: " + price);
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
                                           double impliedVol, double delta, double optionPrice, double pvDividend, double gamma, double vega, double theta, double underlyingPrice)
    {
      if (TickOptionComputation != null)
        TickOptionComputation(this, new TWSTickOptionComputationEventArgs(this) {
          RequestId = reqId,
          TickType = tickType,
          ImpliedVol = impliedVol,
          Delta = delta,
          OptionPrice = optionPrice,
          PVDividend = pvDividend,
          Gamma = gamma,
          Vega = vega,
          Theta = theta,
          UnderlyingPrice = underlyingPrice,
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
          record.ModelPrice = optionPrice;
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

    protected void OnOrderStatus(int orderId, IBOrderStatusReport status)
    {
      if (OrderStatus != null)
        OrderStatus(this, new TWSOrderStatusEventArgs(this) {
          Status = status,
        });

      if (OrderChanged != null) {
        OrderRecord or;
        if (_orderRecords.TryGetValue(orderId, out or)) {
          OrderChanged(this, new TWSOrderChangedEventArgs(this, or) {
            ChangeType = IBOrderChangeType.OrderStatus,
            Status = status,
          });
        }
      }
    }

    protected void OnOpenOrder(int orderId, IBOrder order, IBContract contract, IBOrderState orderState)
    {
      if (OpenOrder != null)
        OpenOrder(this, new TWSOpenOrderEventArgs(this) {
          OrderId = orderId,
          Order = order,
          Contract = contract
        });

      if (OrderChanged != null) {
        OrderRecord or;
        if (_orderRecords.TryGetValue(orderId, out or))
        {
          OrderChanged(this, new TWSOrderChangedEventArgs(this, or) {
            ChangeType = IBOrderChangeType.OpenOrder,
            ReportedContract = contract,
            OpenOrder = order,
            OpenOrderState = orderState
          });
        }
      }
    }

    protected void OnBondContractDetails(int reqId, IBContractDetails contract)
    {
      if (BondContractDetails != null)
        BondContractDetails(this, new TWSContractDetailsEventArgs(this) { ContractDetails = contract });
    }

    protected void OnContractDetails(int reqId, IBContractDetails contract)
    {
      if (ContractDetails != null)
        ContractDetails(this, new TWSContractDetailsEventArgs(this) {
          RequestId = reqId,
          ContractDetails = contract
        });
    }

    protected void OnManagedAccounts(string accountList) {}

    protected void OnReceiveFA(int faDataType, string xml) {}

    protected void OnScannerData(int reqId, int rank, IBContractDetails contract, string s, string distance, string benchmark, string projection)
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

    protected void OnExecutionDetails(int orderId, IBContract contract, IBExecutionDetails execution)
    {
      if (ExecutionDetails != null)
        ExecutionDetails(this, new TWSExecutionDetailsEventArgs(this) {
          OrderId = orderId,
          Contract = contract,
          Execution = execution
        });

      if (OrderChanged != null) {
        OrderRecord or;
        if (_orderRecords.TryGetValue(orderId, out or)) {
          OrderChanged(this, new TWSOrderChangedEventArgs(this, or) {
            ChangeType = IBOrderChangeType.ExecutionDetails,
            ReportedContract = contract,
            ExecutionDetails =  execution,
          });
        }
      }
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
                                    double low, double close, int volume, int barCount, double wap, bool hasGaps)
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
      if (ContractDetails == null)
        return;

      ContractDetails(this, new TWSContractDetailsEventArgs(this) {
        RequestId = reqId,
        ContractDetails = null,
      });
    }

    private void OnOpenOrderEnd()
    {
      if (OpenOrder == null)
        return;

      OpenOrder(this, new TWSOpenOrderEventArgs(this) {
        Contract = null,
        Order = null,
      });
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

    private void OnCommissionReport(CommissionReport commissionReport)
    {
    }

    private void OnMarketDataType(int reqId, int marketDataType)
    {
    }

    #endregion Event Notifiers

    #region Synchronized Request Wrappers
#if NET_4_5

    public async Task<IList<IBContractDetails>> GetContractDetailsAsync(IBContract c)
    {
      var id = -1;
      var results = new List<IBContractDetails>();
      var completed = new ProgressiveTaskCompletionSource<List<IBContractDetails>> {Value = results};

      id = NextValidId;
      _asyncCalls.Add(id, completed);
      RequestContractDetails(c, id);
      await completed.Task;
      _asyncCalls.Remove(id);
      return results;
    }

#endif
    #endregion Synchronized Request Wrappers

    #region Raw Server Mesage Processing

    private void ProcessTickPrice()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
#if NET_4_5
        ProgressiveTaskCompletionSource<object> completion = null;
        IFaultable tmp;
        if (_asyncCalls.TryGetValue(reqId, out tmp)) {
          completion = (ProgressiveTaskCompletionSource<object>) tmp;
          // Just signal the async version that the subscription is OK for sure
          completion.SetResult(null);
        }
#endif

        var tickType = _enc.DecodeEnum<IBTickType>();
        var price = _enc.DecodeDouble();
        var size = (version >= 2) ? _enc.DecodeInt() : 0;
        var canAutoExecute = (version >= 3) ? _enc.DecodeInt() : 0;
        OnTickPrice(reqId, tickType, price, size, canAutoExecute);

      // Contrary to standard IB socket implementation
      // I will no go on with the stupidity of simulating TickSize
      // events when this client library is obviously written
      // to support the combined tick price + size messages
    }

    private void ProcessTickSize()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
#if NET_4_5
      ProgressiveTaskCompletionSource<object> completion = null;
      IFaultable tmp;
      if (_asyncCalls.TryGetValue(reqId, out tmp)) {
        completion = (ProgressiveTaskCompletionSource<object>)tmp;
        // Just signal the async version that the subscription is OK for sure
        completion.SetResult(null);
      }
#endif
      var tickType = _enc.DecodeEnum<IBTickType>();
      var size = _enc.DecodeInt();
      OnTickSize(reqId, tickType, size);
    }

    private void ProcessOrderStatus()
    {
      var version = _enc.DecodeInt();
      var orderId = _enc.DecodeInt();
      var status = _enc.DecodeEnum<IBOrderStatus>();
      var filled = _enc.DecodeInt();
      var remaining = _enc.DecodeInt();
      var avgFillPrice = _enc.DecodeDouble();

      var permId = (version >= 2) ? _enc.DecodeInt() : 0;
      var parentId = (version >= 3) ? _enc.DecodeInt() : 0;
      var lastFillPrice = (version >= 4) ? _enc.DecodeDouble() : 0;
      var clientId = (version >= 5) ? _enc.DecodeInt() : 0;
      var whyHeld = (version >= 6) ? _enc.DecodeString() : null;

      var newStatus = new IBOrderStatusReport {
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
      };

      OnOrderStatus(orderId, newStatus);
    }

    private void ProcessTickString()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
#if NET_4_5
      ProgressiveTaskCompletionSource<object> completion = null;
      IFaultable tmp;
      if (_asyncCalls.TryGetValue(reqId, out tmp))
      {
        completion = (ProgressiveTaskCompletionSource<object>)tmp;
        // Just signal the async version that the subscription is OK for sure
        completion.SetResult(null);
      }
#endif

      var tickType = _enc.DecodeEnum<IBTickType>();
      var value = _enc.DecodeString();

      OnTickString(reqId, tickType, value);
    }

    private void ProcessTickGeneric()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
#if NET_4_5
      ProgressiveTaskCompletionSource<object> completion = null;
      IFaultable tmp;
      if (_asyncCalls.TryGetValue(reqId, out tmp))
      {
        completion = (ProgressiveTaskCompletionSource<object>)tmp;
        // Just signal the async version that the subscription is OK for sure
        completion.SetResult(null);
      }
#endif

      var tickType = _enc.DecodeEnum<IBTickType>();
      var value = _enc.DecodeDouble();
      OnTickGeneric(reqId, tickType, value);
    }

    private void ProcessErrorMessage()
    {
      var version = _enc.DecodeInt();
      if (version < 2) {
        var message = _enc.DecodeString();
        OnError(message);
      }
      else {
#if NET_4_5
        IFaultable completion = null;

        try {
#endif
          var id = _enc.DecodeInt();
#if NET_4_5

          _asyncCalls.TryGetValue(id, out completion);

#endif
          var errorCode = _enc.DecodeInt();
          var message = _enc.DecodeString();
          var twsError = new TWSError(errorCode, message);
#if NET_4_5
          if (completion != null)
            completion.TrySetException(new TWSServerException(twsError));
          else
#endif
            OnError(id, twsError, String.Empty);
#if NET_4_5
        } catch (Exception e) {
          if (completion != null)
            completion.TrySetException(e);
          throw;
        }
#endif
      }
    }

    private void ProcessOpenOrder()
    {
      // read version
      var version = _enc.DecodeInt();
      var order = new IBOrder { OrderId = _enc.DecodeInt() };

      // read contract fields
      var contract = new IBContract {
        ContractId = version >= 17 ? _enc.DecodeInt() : 0,
        Symbol = _enc.DecodeString(),
        SecurityType = _enc.DecodeEnum<IBSecurityType>(),
        Expiry = DecodeIBExpiry(_enc),
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
      order.LimitPrice = version < 29 ? _enc.DecodeDouble() : _enc.DecodeDoubleMax();
      order.AuxPrice = version < 30 ? _enc.DecodeDouble() : _enc.DecodeDoubleMax();

      order.Tif = _enc.DecodeEnum<IBTimeInForce>();
      order.OcaGroup = _enc.DecodeString();
      order.Account = _enc.DecodeString();
      order.OpenClose = _enc.DecodeString();
      order.Origin = _enc.DecodeEnum<IBOrderOrigin>();
      order.OrderRef = _enc.DecodeString();

      if (version >= 3)
        order.ClientId = _enc.DecodeInt();

      if (version >= 4) {
        order.PermId = _enc.DecodeInt();
        if (version < 18)
          order.IgnoreRth = _enc.DecodeInt() == 1;
        else
          order.OutsideRth = _enc.DecodeInt() == 1;

        order.Hidden = _enc.DecodeBool();
        order.DiscretionaryAmt = _enc.DecodeDouble();
      }

      if (version >= 5)
        order.GoodAfterTime = _enc.DecodeString();

      if (version >= 6)
        order.SharesAllocation = _enc.DecodeString();

      if (version >= 7) {
        order.FaGroup = _enc.DecodeString();
        order.FaMethod = _enc.DecodeEnum<IBFinancialAdvisorAllocationMethod>();
        order.FaPercentage = _enc.DecodeString();
        order.FaProfile = _enc.DecodeString();
      }

      if (version >= 8)
        order.GoodTillDate = _enc.DecodeString();

      if (version >= 9) {
        order.Rule80A = _enc.DecodeEnum<IBAgentDescription>();
        order.PercentOffset = _enc.DecodeDouble();
        order.SettlingFirm = _enc.DecodeString();
        order.ShortSaleSlot = _enc.DecodeInt();
        order.DesignatedLocation = _enc.DecodeString();
        if (ServerInfo.Version == 51)
          _enc.DecodeInt(); // exemptCode
        else if (version >= 23)
          order.ExemptCode = _enc.DecodeInt();

        order.AuctionStrategy = _enc.DecodeInt();
        order.StartingPrice = _enc.DecodeDouble();
        order.StockRefPrice = _enc.DecodeDouble();
        order.Delta = _enc.DecodeDouble();
        order.StockRangeLower = _enc.DecodeDouble();
        order.StockRangeUpper = _enc.DecodeDouble();
        order.DisplaySize = _enc.DecodeInt();
        if (version < 18)
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
        order.VolatilityType = _enc.DecodeEnum<IBVolatilityType>();
        if (version == 11) {
          var receivedInt = _enc.DecodeInt();
          order.DeltaNeutralOrderType = ((receivedInt == 0) ? IBOrderType.None : IBOrderType.Market);
        }
        else {
          // version 12 and up

          order.DeltaNeutralOrderType = _enc.DecodeEnum<IBOrderType>();
          order.DeltaNeutralAuxPrice = _enc.DecodeDoubleMax();

          if (version >= 27 && order.DeltaNeutralOrderType != IBOrderType.Empty) {
            order.DeltaNeutralContractId = _enc.DecodeInt();
            order.DeltaNeutralSettlingFirm = _enc.DecodeString();
            order.DeltaNeutralClearingAccount = _enc.DecodeString();
            order.DeltaNeutralClearingIntent = _enc.DecodeString();
          }

          if (version >= 31 && order.DeltaNeutralOrderType != IBOrderType.Empty) {
            order.DeltaNeutralOpenClose = _enc.DecodeString();
            order.DeltaNeutralShortSale = _enc.DecodeBool();
            order.DeltaNeutralShortSaleSlot = _enc.DecodeInt();
            order.DeltaNeutralDesignatedLocation = _enc.DecodeString();
          }
        }
        order.ContinuousUpdate = _enc.DecodeInt();
        if (ServerInfo.Version == 26) {
          order.StockRangeLower = _enc.DecodeDouble();
          order.StockRangeUpper = _enc.DecodeDouble();
        }
        order.ReferencePriceType = _enc.DecodeInt();
      }

      if (version >= 13)
        order.TrailStopPrice = _enc.DecodeDoubleMax();

      if (version >= 30)
        order.TrailingPercent = _enc.DecodeDoubleMax();

      if (version >= 14) {
        order.BasisPoints = _enc.DecodeDouble();
        order.BasisPointsType = _enc.DecodeInt();
        contract.ComboLegsDescription = _enc.DecodeString();
      }

      if (version >= 29) {
        var comboLegsCount = _enc.DecodeInt();
        if (comboLegsCount > 0) {
          contract.ComboLegs = new List<IBComboLeg>(comboLegsCount);
          for (var i = 0; i < comboLegsCount; ++i)
            contract.ComboLegs.Add(new IBComboLeg {
              ContractId = _enc.DecodeInt(),
              Ratio = _enc.DecodeInt(),
              Action = _enc.DecodeEnum<IBAction>(),
              Exchange = _enc.DecodeString(),
              OpenClose = _enc.DecodeEnum<IBComboOpenClose>(),
              ShortSaleSlot = _enc.DecodeEnum<IBShortSaleSlot>(),
              DesignatedLocation = _enc.DecodeString(),
              ExemptCode = _enc.DecodeInt(),
            });
        }

        var orderComboLegsCount = _enc.DecodeInt();
        if (orderComboLegsCount > 0) {
          order.OrderComboLegs = new List<IBOrderComboLeg>(orderComboLegsCount);
          for (var i = 0; i < orderComboLegsCount; ++i)
            order.OrderComboLegs.Add(new IBOrderComboLeg { Price = _enc.DecodeDoubleMax() });
        }
      }

      if (version >= 26) {
        var smartComboRoutingParamsCount = _enc.DecodeInt();
        if (smartComboRoutingParamsCount > 0) {
          order.SmartComboRoutingParams = new List<IBTagValue>(smartComboRoutingParamsCount);
          for (var i = 0; i < smartComboRoutingParamsCount; ++i)
            order.SmartComboRoutingParams.Add(new IBTagValue {
                Tag = _enc.DecodeString(),
                Value = _enc.DecodeString()
              });
        }
      }

      if (version >= 15)
      {
        if (version >= 20) {
          order.ScaleInitLevelSize = _enc.DecodeIntMax();
          order.ScaleSubsLevelSize = _enc.DecodeIntMax();
        }
        else {
          /* int notSuppScaleNumComponents = */
          _enc.DecodeIntMax();
          order.ScaleInitLevelSize = _enc.DecodeIntMax();
        }
        order.ScalePriceIncrement = _enc.DecodeDoubleMax();
      }

      if (version >= 28 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != Double.MaxValue) {
        order.ScalePriceAdjustValue = _enc.DecodeDoubleMax();
        order.ScalePriceAdjustInterval = _enc.DecodeIntMax();
        order.ScaleProfitOffset = _enc.DecodeDoubleMax();
        order.ScaleAutoReset = _enc.DecodeBool();
        order.ScaleInitPosition = _enc.DecodeIntMax();
        order.ScaleInitFillQty = _enc.DecodeIntMax();
        order.ScaleRandomPercent = _enc.DecodeBool();
      }

      if (version >= 24)
      {
        order.HedgeType = _enc.DecodeString();
        if (!String.IsNullOrEmpty(order.HedgeType))
          order.HedgeParam = _enc.DecodeString();
      }

      if (version >= 25)
        order.OptOutSmartRouting = _enc.DecodeBool();

      if (version >= 19) {
        order.ClearingAccount = _enc.DecodeString();
        order.ClearingIntent = _enc.DecodeString();
      }

      if (version >= 22)
        order.NotHeld = _enc.DecodeBool();

      if (version >= 20)
      {
        if (_enc.DecodeBool()) {
          contract.UnderlyingComponent = new IBUnderlyinhComponent {
            ContractId = _enc.DecodeInt(),
            Delta = _enc.DecodeDouble(),
            Price = _enc.DecodeDouble()
          };
        }
      }

      if (version >= 21)
      {
        order.AlgoStrategy = _enc.DecodeString();
        if (!String.IsNullOrEmpty(order.AlgoStrategy))
        {
          var algoParamsCount = _enc.DecodeInt();
          if (algoParamsCount > 0) {
            order.AlgoParams = new List<IBTagValue>(algoParamsCount);
            for (var i = 0; i < algoParamsCount; ++i)
              order.AlgoParams.Add(new IBTagValue {
                Tag = _enc.DecodeString(),
                Value = _enc.DecodeString(),
              });
          }
        }
      }

      var orderState = new IBOrderState();

      if (version >= 16) {
        order.WhatIf = _enc.DecodeBool();

        orderState.Status = _enc.DecodeEnum<IBOrderStatus>();
        orderState.InitMargin = _enc.DecodeString();
        orderState.MaintMargin = _enc.DecodeString();
        orderState.EquityWithLoan = _enc.DecodeString();
        orderState.Commission = _enc.DecodeDoubleMax();
        orderState.MinCommission = _enc.DecodeDoubleMax();
        orderState.MaxCommission = _enc.DecodeDoubleMax();
        orderState.CommissionCurrency = _enc.DecodeString();
        orderState.WarningText = _enc.DecodeString();
      }

      OnOpenOrder(order.OrderId, order, contract, orderState);
    }

    private void ProcessAccountValue()
    {
      var version = _enc.DecodeInt();
      var key = _enc.DecodeString();
      var val = _enc.DecodeString();
      var cur = _enc.DecodeString();
      var accountName = (version >= 2) ? _enc.DecodeString() : null;
      OnUpdateAccountValue(key, val, cur, accountName);
    }

    private void ProcessPortfolioValue()
    {
      var version = _enc.DecodeInt();
      var contractDetails = new IBContract {
        ContractId = version >= 6 ? _enc.DecodeInt() : 0,
        Symbol = _enc.DecodeString(),
        SecurityType = _enc.DecodeEnum<IBSecurityType>(),
        Expiry = DecodeIBExpiry(_enc),
        Strike = _enc.DecodeDouble(),
        Right = _enc.DecodeString(),
        Multiplier = version >= 7 ? _enc.DecodeString() : null,
        PrimaryExchange = version >= 7 ? _enc.DecodeString() : null,
        Currency = _enc.DecodeString(),
        LocalSymbol = version >= 2 ? _enc.DecodeString() : null,
      };

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

      if (version == 6 && ServerInfo.Version == 39)
        contractDetails.PrimaryExchange = _enc.DecodeString();

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
#if NET_4_5
      ProgressiveTaskCompletionSource<List<IBContractDetails>> completion = null;

      try
      {
#endif
        var version = _enc.DecodeInt();
        var reqId = -1;
        if (version >= 3)
          reqId = _enc.DecodeInt();
#if NET_4_5
        IFaultable tmp;
        if (_asyncCalls.TryGetValue(reqId, out tmp))
          completion = (ProgressiveTaskCompletionSource<List<IBContractDetails>>)tmp;
#endif
        var contractDetails = new IBContractDetails {
          Summary = {
            Symbol = _enc.DecodeString(),
            SecurityType = _enc.DecodeEnum<IBSecurityType>(),
            Expiry = DecodeIBExpiry(_enc),
            Strike = _enc.DecodeDouble(),
            Right = _enc.DecodeString(),
            Exchange = _enc.DecodeString(),
            Currency = _enc.DecodeString(),
            LocalSymbol = _enc.DecodeString()
          },
          MarketName = _enc.DecodeString(),
          TradingClass = _enc.DecodeString(),
          ContractId = _enc.DecodeInt(),
          MinTick = _enc.DecodeDouble(),
          Multiplier = _enc.DecodeString(),
          OrderTypes = _enc.DecodeString(),
          ValidExchanges = _enc.DecodeString()
        };
        if (version >= 2)
          contractDetails.PriceMagnifier = _enc.DecodeInt();

        if (version >= 4)
          contractDetails.UnderlyingContractId = _enc.DecodeInt();

        if (version >= 5) {
          contractDetails.LongName = _enc.DecodeString();
          contractDetails.Summary.PrimaryExchange = _enc.DecodeString();
        }
        if (version >= 6) {
          contractDetails.ContractMonth = _enc.DecodeString();
          contractDetails.Industry = _enc.DecodeString();
          contractDetails.Category = _enc.DecodeString();
          contractDetails.Subcategory = _enc.DecodeString();
          contractDetails.TimeZoneId = _enc.DecodeString();
          contractDetails.TradingHours = _enc.DecodeString();
          contractDetails.LiquidHours = _enc.DecodeString();
        }
        if (version >= 8) {
          contractDetails.EvRule = _enc.DecodeString();
          contractDetails.EvMultiplier = _enc.DecodeDouble();
        }
        if (version >= 7) {
          var secIdListCount = _enc.DecodeInt();
          if (secIdListCount > 0) {
            contractDetails.SecIdList = new List<IBTagValue>(secIdListCount);
            for (var i = 0; i < secIdListCount; ++i)
              contractDetails.SecIdList.Add(new IBTagValue { Tag = _enc.DecodeString(), Value = _enc.DecodeString() });
          }
        }
#if NET_4_5
        if (completion != null)
          completion.Value.Add(contractDetails);
        else
#endif
          OnContractDetails(reqId, contractDetails);
#if NET_4_5
      } catch (Exception e) {
        if (completion != null)
          completion.SetException(e);
        throw;
      }
#endif
    }

    private static DateTime? DecodeIBExpiry(ITWSEncoding enc)
    {
      var v = enc.DecodeString();
      return String.IsNullOrEmpty(v)
               ? (DateTime?) null
               : DateTime.ParseExact(v, IB_EXPIRY_DATE_FORMAT, CultureInfo.InvariantCulture);
    }

    private void ProcessExecutionData()
    {
      var version = _enc.DecodeInt();
      var reqId = -1;
      if (version >= 7)
        reqId = _enc.DecodeInt();

      var orderId = _enc.DecodeInt();
      var contract = new IBContract
        {
          ContractId = version >= 5 ? _enc.DecodeInt() : 0,
          Symbol = _enc.DecodeString(),
          SecurityType = _enc.DecodeEnum<IBSecurityType>(),
          Expiry = DecodeIBExpiry(_enc),
          Strike = _enc.DecodeDouble(),
          Right = _enc.DecodeString()
        };
      if (version >= 9)
        contract.Multiplier = _enc.DecodeString();

      contract.Exchange = _enc.DecodeString();
      contract.Currency = _enc.DecodeString();
      contract.LocalSymbol = _enc.DecodeString();
      var execution = new IBExecutionDetails {
          OrderId = orderId,
          ExecId = _enc.DecodeString(),
          Time = _enc.DecodeString(),
          AcctNumber = _enc.DecodeString(),
          Exchange = _enc.DecodeString(),
          Side = _enc.DecodeEnum<IBExecutionSide>(),
          Shares = _enc.DecodeInt(),
          Price = _enc.DecodeDouble(),
          PermId = version >= 2 ? _enc.DecodeInt() : 0,
          ClientId = version >= 3 ? _enc.DecodeInt() : 0,
          Liquidation = version >= 4 ? _enc.DecodeInt() : 0,
          CumQty = version >= 6 ? _enc.DecodeInt() : 0,
          AveragePrice = version >= 6 ? _enc.DecodeDouble() : 0,
          OrderRef = version >= 8 ? _enc.DecodeString() : null,
          EvRule = version >= 9 ? _enc.DecodeString() : null,
          EvMultiplier = version >= 9 ? _enc.DecodeDouble() : 0
        };

      OnExecutionDetails(reqId, contract, execution);
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
        var barCount = version >= 3 ? _enc.DecodeInt() : -1;
        OnHistoricalData(reqId, TWSHistoricState.Downloading, dateTime, open, high, low, close, volume, barCount, WAP, Boolean.Parse(hasGaps));
      }
      // Send end of dataset marker
      OnHistoricalData(reqId, TWSHistoricState.Finished, endDateTime, -1, -1, -1, -1, -1, -1, -1, false);
    }

    private void ProcessBondContractData()
    {
      var version = _enc.DecodeInt();
      var reqId = version >= 3 ? _enc.DecodeInt() : -1;

      var contract = new IBContractDetails {
        Summary = {
          Symbol = _enc.DecodeString(),
          SecurityType = _enc.DecodeEnum<IBSecurityType>(),
          Cusip = _enc.DecodeString(),
          Coupon = _enc.DecodeDouble(),
          Maturity = _enc.DecodeString(),
          IssueDate = _enc.DecodeString(),
          Ratings = _enc.DecodeString(),
          BondType = _enc.DecodeString(),
          CouponType = _enc.DecodeString(),
          Convertible = _enc.DecodeBool(),
          Callable = _enc.DecodeBool(),
          Putable = _enc.DecodeBool(),
          DescAppend = _enc.DecodeString(),
          Exchange = _enc.DecodeString(),
          Currency = _enc.DecodeString()
        },
        MarketName = _enc.DecodeString(),
        TradingClass = _enc.DecodeString(),
        ContractId = _enc.DecodeInt(),
        MinTick = _enc.DecodeDouble(),
        OrderTypes = _enc.DecodeString(),
        ValidExchanges = _enc.DecodeString()
      };

      if (version >= 2) {
        contract.Summary.NextOptionDate = _enc.DecodeString();
        contract.Summary.NextOptionType = _enc.DecodeString();
        contract.Summary.NextOptionPartial = _enc.DecodeBool();
        contract.Summary.Notes = _enc.DecodeString();
      }

      if (version >= 4)
        contract.LongName = _enc.DecodeString();

      if (version >= 6) {
        contract.EvRule = _enc.DecodeString();
        contract.EvMultiplier = _enc.DecodeDouble();
      }
      if (version >= 5) {
        var secIdListCount = _enc.DecodeInt();
        if (secIdListCount > 0) {
          contract.SecIdList = new List<IBTagValue>(secIdListCount);
          for (var i = 0; i < secIdListCount; ++i)
            contract.SecIdList.Add(new IBTagValue {
                Tag = _enc.DecodeString(),
                Value = _enc.DecodeString(),
              });
          }
      }
      OnBondContractDetails(reqId, contract);
    }

    private void ProcessScannerParameters()
    {
      var version = _enc.DecodeInt();
      var xml = _enc.DecodeString();
      OnScannerParameters(xml);
    }

    private void ProcessScannerData()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var numberOfElements = _enc.DecodeInt();
      for (var i = 0; i < numberOfElements; i++) {
        var rank = _enc.DecodeInt();
        var contract = new IBContractDetails
          {
            Summary =
              {
                ContractId = version >= 3 ? _enc.DecodeInt() : 0,
                Symbol = _enc.DecodeString(),
                SecurityType = _enc.DecodeEnum<IBSecurityType>(),
                Expiry = DecodeIBExpiry(_enc),
                Strike = _enc.DecodeDouble(),
                Right = _enc.DecodeString(),
                Exchange = _enc.DecodeString(),
                Currency = _enc.DecodeString(),
                LocalSymbol = _enc.DecodeString()
              },
            MarketName = _enc.DecodeString(),
            TradingClass = _enc.DecodeString()
          };
        var distance = _enc.DecodeString();
        var benchmark = _enc.DecodeString();
        var projection = _enc.DecodeString();
        string legStr = null;
        if (version >= 2)
          legStr = _enc.DecodeString();
        OnScannerData(reqId, rank, contract, distance, benchmark, projection, legStr);
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
      var optPrice = Double.MaxValue;
      var pvDividend = Double.MaxValue;
      var gamma = Double.MaxValue;
      var vega = Double.MaxValue;
      var theta = Double.MaxValue;
      var undPrice = Double.MaxValue;
      if (version >= 6 || tickType == IBTickType.ModelOption) {
        // introduced in version == 5
        optPrice = _enc.DecodeDouble();
        if (optPrice < 0) {
          // -1 is the "not yet computed" indicator
          optPrice = Double.MaxValue;
        }
        pvDividend = _enc.DecodeDouble();
        if (pvDividend < 0)
        { // -1 is the "not yet computed" indicator
          pvDividend = Double.MaxValue;
        }
      }
      if (version >= 6) {
        gamma = _enc.DecodeDouble();
        if (Math.Abs(gamma) > 1)
        { // -2 is the "not yet computed" indicator
          gamma = Double.MaxValue;
        }
        vega = _enc.DecodeDouble();
        if (Math.Abs(vega) > 1)
        { // -2 is the "not yet computed" indicator
          vega = Double.MaxValue;
        }
        theta = _enc.DecodeDouble();
        if (Math.Abs(theta) > 1)
        { // -2 is the "not yet computed" indicator
          theta = Double.MaxValue;
        }
        undPrice = _enc.DecodeDouble();
        if (undPrice < 0)
        { // -1 is the "not yet computed" indicator
          undPrice = Double.MaxValue;
        }
      }

      OnTickOptionComputation(reqId, tickType, impliedVol, delta, optPrice, pvDividend, gamma, vega, theta, undPrice);
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
#if NET_4_5
      ProgressiveTaskCompletionSource<List<IBContractDetails>> completion = null;

      try
      {
#endif
        var version = _enc.DecodeInt();
        var reqId = _enc.DecodeInt();
#if NET_4_5
        IFaultable tmp;
        if (_asyncCalls.TryGetValue(reqId, out tmp))
          completion = (ProgressiveTaskCompletionSource<List<IBContractDetails>>) tmp;

        if (completion != null)
          completion.SetCompleted();
        else
#endif
        OnContractDetailsEnd(reqId);

#if NET_4_5
      } catch (Exception e) {
        if (completion != null)
          completion.SetException(e);
        throw;
      }
#endif
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

    private void ProcessCommissionReport()
    {
      var version = _enc.DecodeInt();

      var commissionReport = new CommissionReport
      {
        ExecId = _enc.DecodeString(),
        Commission = _enc.DecodeDouble(),
        Currency = _enc.DecodeString(),
        RealizedPnl = _enc.DecodeDouble(),
        Yield = _enc.DecodeDouble(),
        YieldRedemptionDate = _enc.DecodeInt()
      };
      OnCommissionReport(commissionReport);
    }

    private void ProcessMarketDataType()
    {
      var version = _enc.DecodeInt();
      var reqId = _enc.DecodeInt();
      var marketDataType = _enc.DecodeInt();

      OnMarketDataType(reqId, marketDataType);
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
        OnException(e);
#if NET_4_5
        foreach (var f in _asyncCalls.Values)
          f.TrySetException(new TWSDisconnectedException());
#endif
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
        case ClientMessage.ErrorMessage:           ProcessErrorMessage();           break;
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
        case ClientMessage.MarketDataType:         ProcessMarketDataType();         break;
        case ClientMessage.CommissionReport:       ProcessCommissionReport();       break;;
        default:
          return false;
      }

      // All is well
      return true;
    }

    #endregion Raw Server Mesage Processing

    #region Request Methods

    /// <summary>
    /// Places an order for the requested contract through TWS
    /// </summary>
    /// <param name="contract">The contract.</param>
    /// <param name="order">The order.</param>
    /// <returns>the order id of the newly generated order</returns>
    /// <exception cref="Daemaged.IBNet.Client.NotConnectedException"></exception>
    public virtual int PlaceOrder(IBContract contract, IBOrder order)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        CheckServerCompatability(contract, order);

        var orderId = NextValidId;

        var reqVersion = ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_NOT_HELD ? 27 : 39;

        try {
          _enc.Encode(ServerMessage.PlaceOrder);
          _enc.Encode(reqVersion);
          _enc.Encode(orderId);
          // send contract fields
          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_PLACE_ORDER_CONID)
            _enc.Encode(contract.ContractId);

          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          if (ServerInfo.Version >= 15)
            _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          if (ServerInfo.Version >= 14)
            _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          if (ServerInfo.Version >= 2)
            _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version > TWSServerInfo.MIN_SERVER_VER_SEC_ID_TYPE) {
            _enc.Encode(contract.SecurityIdType);
            _enc.Encode(contract.SecurityId);
          }

          _enc.Encode(order.Action);
          _enc.Encode(order.TotalQuantity);
          _enc.Encode(order.OrderType);

          if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_ORDER_COMBO_LEGS_PRICE)
            _enc.Encode(order.LimitPrice == Double.MaxValue ? 0 : order.LimitPrice);
          else
            _enc.EncodeMax(order.LimitPrice);

          if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_TRAILING_PERCENT)
            _enc.Encode(order.AuxPrice == Double.MaxValue ? 0 : order.AuxPrice);
          else
            _enc.EncodeMax(order.AuxPrice);

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
            if (ServerInfo.Version < 38)
              _enc.Encode(false); // Deprecated order.IgnoreRth
            else
              _enc.Encode(order.OutsideRth);
          }
          if (ServerInfo.Version >= 7)
            _enc.Encode(order.Hidden);

          if ((ServerInfo.Version >= 8) && (contract.SecurityType == IBSecurityType.Bag)) {
            _enc.Encode(contract.ComboLegs.Count);
            foreach (var leg in contract.ComboLegs) {
              _enc.Encode(leg.ContractId);
              _enc.Encode(leg.Ratio);
              _enc.Encode(leg.Action);
              _enc.Encode(leg.Exchange);
              _enc.Encode(leg.OpenClose);

              if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SSHORT_COMBO_LEGS) {
                _enc.Encode(leg.ShortSaleSlot);
                _enc.Encode(leg.DesignatedLocation);
              }
              if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SSHORTX_OLD)
                _enc.Encode(leg.ExemptCode);
            }
          }

          // Send order combo legs for BAG requests
          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_ORDER_COMBO_LEGS_PRICE && contract.SecurityType == IBSecurityType.Bag) {
            if (order.OrderComboLegs == null)
              _enc.Encode(0);
            else {
              _enc.Encode(order.OrderComboLegs.Count);
              foreach (var l in order.OrderComboLegs)
                _enc.EncodeMax(l.Price);
            }
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SMART_COMBO_ROUTING_PARAMS && contract.SecurityType == IBSecurityType.Bag) {
            if (order.SmartComboRoutingParams == null || order.SmartComboRoutingParams.Count == 0)
              _enc.Encode(0);
            else {
              _enc.Encode(order.SmartComboRoutingParams.Count);
              foreach (var tv in order.SmartComboRoutingParams) {
                _enc.Encode(tv.Tag);
                _enc.Encode(tv.Value);
              }
            }
          }

          if (ServerInfo.Version >= 9)
            _enc.Encode(String.Empty); // Deprecated order.SharesAllocation
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

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SSHORTX_OLD)
            _enc.Encode(order.ExemptCode);

          if (ServerInfo.Version >= 19) {
            _enc.Encode(order.OcaType);
            if (ServerInfo.Version < 38)
              _enc.Encode(false); // Deprecated order.m_rthOnly

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
              _enc.Encode(order.VolatilityType);
              if (ServerInfo.Version < 28) {
                _enc.Encode(order.DeltaNeutralOrderType == IBOrderType.Market);
              }
              else {
                _enc.Encode(order.DeltaNeutralOrderType);
                _enc.EncodeMax(order.DeltaNeutralAuxPrice);

                if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_DELTA_NEUTRAL_CONID && order.DeltaNeutralOrderType != IBOrderType.Empty) {
                  _enc.Encode(order.DeltaNeutralContractId);
                  _enc.Encode(order.DeltaNeutralSettlingFirm);
                  _enc.Encode(order.DeltaNeutralClearingAccount);
                  _enc.Encode(order.DeltaNeutralClearingIntent);
                }

                if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_DELTA_NEUTRAL_OPEN_CLOSE && order.DeltaNeutralOrderType != IBOrderType.Empty) {
                  _enc.Encode(order.DeltaNeutralOpenClose);
                  _enc.Encode(order.DeltaNeutralShortSale);
                  _enc.Encode(order.DeltaNeutralShortSaleSlot);
                  _enc.Encode(order.DeltaNeutralDesignatedLocation);
                }
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

          if (ServerInfo.Version >= 30) { // TRAIL_STOP_LIMIT stop price
              _enc.EncodeMax( order.TrailStopPrice);
          }

          if( ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_TRAILING_PERCENT){
              _enc.EncodeMax( order.TrailingPercent);
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SCALE_ORDERS) {
        	  if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SCALE_ORDERS2) {
        	   _enc.EncodeMax(order.ScaleInitLevelSize);
        	   _enc.EncodeMax(order.ScaleSubsLevelSize);
        	  }
        	  else {
        	   _enc.Encode("");
        	   _enc.EncodeMax(order.ScaleInitLevelSize);
        	  }
        	  _enc.EncodeMax(order.ScalePriceIncrement);
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SCALE_ORDERS3 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != Double.MaxValue) {
              _enc.EncodeMax(order.ScalePriceAdjustValue);
              _enc.EncodeMax(order.ScalePriceAdjustInterval);
              _enc.EncodeMax(order.ScaleProfitOffset);
              _enc.Encode(order.ScaleAutoReset);
              _enc.EncodeMax(order.ScaleInitPosition);
              _enc.EncodeMax(order.ScaleInitFillQty);
              _enc.Encode(order.ScaleRandomPercent);
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_HEDGE_ORDERS) {
        	  _enc.Encode(order.HedgeType);
        	  if (!String.IsNullOrEmpty(order.HedgeType)) {
        	   _enc.Encode(order.HedgeParam);
        	  }
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_OPT_OUT_SMART_ROUTING) {
              _enc.Encode(order.OptOutSmartRouting);
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_PTA_ORDERS) {
        	  _enc.Encode(order.ClearingAccount);
        	  _enc.Encode(order.ClearingIntent);
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_NOT_HELD)
        	  _enc.Encode(order.NotHeld);

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_UNDER_COMP) {
        	  if (contract.UnderlyingComponent != null) {
        	   var uc = contract.UnderlyingComponent;
        	   _enc.Encode( true);
        	   _enc.Encode(uc.ContractId);
        	   _enc.Encode(uc.Delta);
        	   _enc.Encode(uc.Price);
        	  }
        	  else {
        	   _enc.Encode(false);
        	  }
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_ALGO_ORDERS) {
        	  _enc.Encode( order.AlgoStrategy ?? String.Empty);
        	  if (!String.IsNullOrEmpty(order.AlgoStrategy)) {
        	   if (order.AlgoParams == null || order.AlgoParams.Count == 0)
               _enc.Encode(0);
             else {
        	     _enc.Encode(order.AlgoParams.Count);
        		   foreach (var tv in order.AlgoParams) {
                 _enc.Encode(tv.Tag);
        			   _enc.Encode(tv.Value);
        		   }
        	   }
        	  }
          }

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_WHAT_IF_ORDERS)
        	  _enc.Encode(order.WhatIf);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }

        order.OrderId = orderId;
        _orderRecords.Add(orderId, new OrderRecord {
          OrderId = orderId,
          Order = order,
          Contract = contract,
        });
        return orderId;
      }
    }

    private void CheckServerCompatability(IBContract contract, IBOrder order)
    {
      //Scale Orders Minimum Version is 35
      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SCALE_ORDERS)
        if (order.ScaleInitLevelSize != Int32.MaxValue || order.ScalePriceIncrement != Double.MaxValue)
          throw new TWSOutdatedException();

      //Minimum Sell Short Combo Leg Order is 35
      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SSHORT_COMBO_LEGS)
        if (contract.ComboLegs.Count != 0)
          if (contract.ComboLegs.Any(t => t.ShortSaleSlot != 0 || (!string.IsNullOrEmpty(t.DesignatedLocation))))
            throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_WHAT_IF_ORDERS)
        if (order.WhatIf)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_UNDER_COMP)
        if (contract.UnderlyingComponent != null)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SCALE_ORDERS2)
        if (order.ScaleSubsLevelSize != Int32.MaxValue)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_ALGO_ORDERS)
        if (!string.IsNullOrEmpty(order.AlgoStrategy))
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_NOT_HELD)
        if (order.NotHeld)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SEC_ID_TYPE)
        if (contract.SecurityIdType != IBSecurityIdType.None || !string.IsNullOrEmpty(contract.SecurityId))
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_PLACE_ORDER_CONID)
        if (contract.ContractId > 0)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SSHORTX)
        if (order.ExemptCode != -1)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SSHORTX)
        if (contract.ComboLegs.Count > 0)
          if (contract.ComboLegs.Any(comboLeg => comboLeg.ExemptCode != -1))
            throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_HEDGE_ORDERS)
        if (!String.IsNullOrEmpty(order.HedgeType))
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_OPT_OUT_SMART_ROUTING)
        if (order.OptOutSmartRouting)
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_DELTA_NEUTRAL_CONID)
        if (order.DeltaNeutralContractId > 0 ||
            !String.IsNullOrEmpty(order.DeltaNeutralSettlingFirm) ||
            !String.IsNullOrEmpty(order.DeltaNeutralClearingAccount) ||
            !String.IsNullOrEmpty(order.DeltaNeutralClearingIntent))
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_DELTA_NEUTRAL_OPEN_CLOSE)
        if (!String.IsNullOrEmpty(order.DeltaNeutralOpenClose) ||
            order.DeltaNeutralShortSale || order.DeltaNeutralShortSaleSlot > 0 || !String.IsNullOrEmpty(order.DeltaNeutralDesignatedLocation))
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SCALE_ORDERS3)
        if (order.ScalePriceIncrement > 0 && order.ScalePriceIncrement != Double.MaxValue)
          if (order.ScalePriceAdjustValue != Double.MaxValue ||
              order.ScalePriceAdjustInterval != Int32.MaxValue ||
              order.ScaleProfitOffset != Double.MaxValue ||
              order.ScaleAutoReset ||
              order.ScaleInitPosition != Int32.MaxValue ||
              order.ScaleInitFillQty != Int32.MaxValue ||
              order.ScaleRandomPercent)
            throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_ORDER_COMBO_LEGS_PRICE && contract.SecurityType == IBSecurityType.Bag)
        if (order.OrderComboLegs.Any(c => c.Price != Double.MaxValue))
          throw new TWSOutdatedException();

      if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_TRAILING_PERCENT)
        if (order.TrailingPercent != Double.MaxValue)
          throw new TWSOutdatedException();
    }

    public virtual void ExerciseOptions(int reqId,
      IBContract contract,
      int exerciseAction,
      int exerciseQuantity,
      string account,
      int overrideOrder)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

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
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
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
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestServerLogLevelChange(int logLevel)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        // send the set server logging level message
        try {
          _enc.Encode(ServerMessage.SetServerLogLevel);
          _enc.Encode(reqVersion);
          _enc.Encode(logLevel);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual int RequestHistoricalData(IBContract contract, string endDateTime,
                                             string durationStr, int barSizeSetting,
                                             string whatToShow, int useRTH, int formatDate)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 4;
        var requestId = NextValidId;

        try {
          if (ServerInfo.Version < 16)
            throw new TWSOutdatedException();

          _enc.Encode(ServerMessage.RequestHistoricalData);
          _enc.Encode(reqVersion);
          _enc.Encode(requestId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version >= 31)
            _enc.Encode(contract.IncludeExpired ? 1 : 0);
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
            if (contract.ComboLegs == null || contract.ComboLegs.Count == 0)
              _enc.Encode(0);
            else {
              _enc.Encode(contract.ComboLegs.Count);

              foreach (var comboLeg in contract.ComboLegs) {
                _enc.Encode(comboLeg.ContractId);
                _enc.Encode(comboLeg.Ratio);
                _enc.Encode(comboLeg.Action);
                _enc.Encode(comboLeg.Exchange);
              }
            }
          }
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
        return requestId;
      }
    }

#if NET_4_5

    public async Task<int> RequestMarketDataAsync(IBContract contract, IList<IBGenericTickType> tickList = null, bool snapshot = false)
    {
      var completion = new ProgressiveTaskCompletionSource<object>();
      var id = NextValidId;
      _asyncCalls.Add(id, completion);
      RequestMarketData(contract, tickList, snapshot, id);

      int timeout = 1000;
      if (await Task.WhenAny(completion.Task, Task.Delay(timeout)) == completion.Task) {
        if (completion.Task.IsFaulted)
          throw completion.Task.Exception;
      }

      // We might be here because there was no market-data or error within the timeout
      // At any rate, we consider this to be a success
      // IB is shit, this is the best we can do
      _asyncCalls.Remove(id);
      return id;
    }

#endif

    public virtual int RequestMarketData(IBContract contract, IList<IBGenericTickType> genericTickList = null, bool snapshot = false, int reqId = 0)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SNAPSHOT_MKT_DATA && snapshot)
          throw new TWSOutdatedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_UNDER_COMP)
          if (contract.UnderlyingComponent != null)
            throw new TWSOutdatedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_REQ_MKT_DATA_CONID)
          if (contract.ContractId > 0)
            throw new TWSOutdatedException();

        const int reqVersion = 9;
        if (reqId == 0)
          reqId = NextValidId;

        try {
          _enc.Encode(ServerMessage.RequestMarketData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_REQ_MKT_DATA_CONID)
            _enc.Encode(contract.ContractId);

          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          if (ServerInfo.Version >= 15)
            _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          if (ServerInfo.Version >= 14)
            _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          if (ServerInfo.Version >= 2)
            _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version >= 8 && (contract.SecurityType == IBSecurityType.Bag)) {
            if (contract.ComboLegs == null || contract.ComboLegs.Count == 0)
              _enc.Encode(0);
            else {
              _enc.Encode(contract.ComboLegs.Count);
              foreach (var leg in contract.ComboLegs) {
                _enc.Encode(leg.ContractId);
                _enc.Encode(leg.Ratio);
                _enc.Encode(leg.Action);
                _enc.Encode(leg.Exchange);
              }
            }
          }
          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_UNDER_COMP)
          {
            if (contract.UnderlyingComponent != null) {
              _enc.Encode(true);
              _enc.Encode(contract.UnderlyingComponent.ContractId);
              _enc.Encode(contract.UnderlyingComponent.Delta);
              _enc.Encode(contract.UnderlyingComponent.Price);
            }
            else
              _enc.Encode(false);
            }

          if (ServerInfo.Version >= 31) {
            var sb = new StringBuilder();
            if (genericTickList != null) {
              foreach (var tick in genericTickList)
                sb.Append((int) tick).Append(',');
              sb.Remove(sb.Length - 2, 1);
            }
            _enc.Encode(sb.ToString());
          }

          // If we got to here without choking on something
          // we update the request registry
          _marketDataRecords.Add(reqId, new TWSMarketDataSnapshot(contract, reqId));

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SNAPSHOT_MKT_DATA)
            _enc.Encode(snapshot);

          _enc.Flush();

          return reqId;
        }
        catch (Exception e) {
          Disconnect();
          throw;
        }
      }
    }

    public  void RequestManagedAccounts()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        // send req FA managed accounts msg
        try {
          _enc.Encode(ServerMessage.RequestManagedAccounts);
          _enc.Encode(reqVersion);
        }
        catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void RequestFA(int faDataType)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        // This feature is only available for versions of TWS >= 13
        if (ServerInfo.Version < 13)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          _enc.Encode(ServerMessage.RequestFA);
          _enc.Encode(reqVersion);
          _enc.Encode(faDataType);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void ReplaceFA(int faDataType, String xml)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        // This feature is only available for versions of TWS >= 13
        if (ServerInfo.Version < 13)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          _enc.Encode(ServerMessage.ReplaceFA);
          _enc.Encode(reqVersion);
          _enc.Encode(faDataType);
          _enc.Encode(xml);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void RequestScannerParameters()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();
        if (ServerInfo.Version < 24)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          _enc.Encode(ServerMessage.RequestScannerParameters);
          _enc.Encode(reqVersion);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void RequestScannerSubscription(int tickerId, ScannerSubscription subscription)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();
        if (ServerInfo.Version < 24)
          throw new TWSOutdatedException();

        const int VERSION = 3;

        try {
          _enc.Encode(ServerMessage.RequestScannerSubscription);
          _enc.Encode(VERSION);
          _enc.Encode(tickerId);
          _enc.EncodeMax(subscription.NumberOfRows);
          _enc.Encode(subscription.Instrument);
          _enc.Encode(subscription.LocationCode);
          _enc.Encode(subscription.ScanCode);
          _enc.EncodeMax(subscription.AbovePrice);
          _enc.EncodeMax(subscription.BelowPrice);
          _enc.EncodeMax(subscription.AboveVolume);
          _enc.EncodeMax(subscription.MarketCapAbove);
          _enc.EncodeMax(subscription.MarketCapBelow);
          _enc.Encode(subscription.MoodyRatingAbove);
          _enc.Encode(subscription.MoodyRatingBelow);
          _enc.Encode(subscription.SPRatingAbove);
          _enc.Encode(subscription.SPRatingBelow);
          _enc.Encode(subscription.MaturityDateAbove);
          _enc.Encode(subscription.MaturityDateBelow);
          _enc.EncodeMax(subscription.CouponRateAbove);
          _enc.EncodeMax(subscription.CouponRateBelow);
          _enc.Encode(subscription.ExcludeConvertible);
          if (ServerInfo.Version >= 25) {
            _enc.Encode(subscription.AverageOptionVolumeAbove);
            _enc.Encode(subscription.ScannerSettingPairs);
          }
          if (ServerInfo.Version >= 27)
            _enc.Encode(subscription.StockTypeFilter);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void RequestMarketDataType(int marketDataType)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_REQ_MARKET_DATA_TYPE)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        // send the reqMarketDataType message
        try {
          _enc.Encode(ServerMessage.RequestMarketDataType);
          _enc.Encode(reqVersion);
          _enc.Encode(marketDataType);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void RequestFundamentalData(int reqId, IBContract contract, String reportType)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_FUNDAMENTAL_DATA)
          throw new TWSOutdatedException();

        const int VERSION = 1;

        try {
          // send req fund data msg
          _enc.Encode(ServerMessage.RequestFundamentalData);
          _enc.Encode(VERSION);
          _enc.Encode(reqId);

          // send contract fields
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);

          _enc.Encode(reportType);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelFundamentalData(int reqId)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_FUNDAMENTAL_DATA)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          // send req mkt data msg
          _enc.Encode(ServerMessage.CancelFundamentalData);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CalculateImpliedVolatility(int reqId, IBContract contract, double optionPrice, double underPrice)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_REQ_CALC_IMPLIED_VOLAT)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          // send calculate implied volatility msg
          _enc.Encode(ServerMessage.RequestCalcImpliedVolatility);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);

          // send contract fields
          _enc.Encode(contract.ContractId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);

          _enc.Encode(optionPrice);
          _enc.Encode(underPrice);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelCalculateImpliedVolatility(int reqId)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_CANCEL_CALC_IMPLIED_VOLAT)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          // send cancel calculate implied volatility msg
          _enc.Encode(ServerMessage.CancelCalcImpliedVolatility);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CalculateOptionPrice(int reqId, IBContract contract, double volatility, double underPrice)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_REQ_CALC_OPTION_PRICE)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          // send calculate option price msg
          _enc.Encode(ServerMessage.RequestCalcOptionPrice);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);

          // send contract fields
          _enc.Encode(contract.ContractId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);

          _enc.Encode(volatility);
          _enc.Encode(underPrice);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void CancelCalculateOptionPrice(int reqId)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_CANCEL_CALC_OPTION_PRICE)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          // send cancel calculate option price msg
          _enc.Encode(ServerMessage.CancelCalcOptionPrice);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public void RequestGlobalCancel()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_REQ_GLOBAL_CANCEL)
          throw new TWSOutdatedException();

        const int VERSION = 1;

        // send request global cancel msg
        try {
          _enc.Encode(ServerMessage.RequestGlobalCancel);
          _enc.Encode(VERSION);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestRealTimeBars(int reqId, IBContract contract,
                                            int barSize, string whatToShow, bool useRTH)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();
        if (ServerInfo.Version < 34)
          throw new TWSOutdatedException();

        const int reqVersion = 1;

        try {
          // send req mkt data msg
          _enc.Encode(ServerMessage.RequestRealTimeBars);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);

          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          _enc.Encode(contract.Multiplier);
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.PrimaryExchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);
          _enc.Encode(barSize);
          _enc.Encode(whatToShow);
          _enc.Encode(useRTH);
        } catch (Exception e) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual int RequestMarketDepth(IBContract contract, int numRows)
    {
      lock (_socketLock) {
        if (!IsConnected)
          throw new NotConnectedException();

        if (ServerInfo.Version < 6)
          throw new TWSOutdatedException();

        const int reqVersion = 3;
        var reqId = NextValidId;
        try {
          _enc.Encode(ServerMessage.RequestMarketDepth);
          _enc.Encode(reqVersion);
          _enc.Encode(reqId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
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
        catch (Exception) {
          Disconnect();
          throw;
        }
        return reqId;
      }
    }

    public virtual void RequestAutoOpenOrders(bool autoBind)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        // send req open orders msg
        try {
          _enc.Encode(ServerMessage.RequestAutoOpenOrders);
          _enc.Encode(reqVersion);
          _enc.Encode(autoBind);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestIds(int numIds)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        try {
          _enc.Encode(ServerMessage.RequestIds);
          _enc.Encode(reqVersion);
          _enc.Encode(numIds);
        } catch (Exception e) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestOpenOrders()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        // send cancel order msg
        try {
          _enc.Encode(ServerMessage.RequestOpenOrders);
          _enc.Encode(reqVersion);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestAccountUpdates(bool subscribe, string acctCode)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

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
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestAllOpenOrders()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        // send req all open orders msg
        try {
          _enc.Encode(ServerMessage.RequestAllOpenOrders);
          _enc.Encode(reqVersion);
        } catch (Exception e) {
          OnError(TWSErrors.FAIL_SEND_OORDER);
          OnError(e.Message);
          Disconnect();
        }
      }
    }

    /// <summary>
    /// Requests the executions.
    /// </summary>
    /// <param name="filter">A filter to help narrow down the list of executions</param>
    /// <returns>the request id associated with this request to TWS, to be used later with the <see cref="ExecutionDetails"/> event</returns>
    /// <exception cref="Daemaged.IBNet.Client.NotConnectedException"></exception>
    public virtual int RequestExecutions(IBExecutionFilter filter)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 3;

        // send cancel order msg
        try {
          _enc.Encode(ServerMessage.RequestExecutions);
          _enc.Encode(reqVersion);

          var requestId = NextValidId;

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_EXECUTION_DATA_CHAIN)
            _enc.Encode(requestId);

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
          return requestId;
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestNewsBulletins(bool allMsgs)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        const int reqVersion = 1;

        try {
          _enc.Encode(ServerMessage.RequestNewsBulletins);
          _enc.Encode(reqVersion);
          _enc.Encode(allMsgs);
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    /// <summary>
    /// Request Full Contract Details for the specified partial contract through the <see cref="ContractDetails"/> event
    /// </summary>
    /// <param name="contract">The partial contract details</param>
    /// <param name="requestId">Optional request id</param>
    /// <returns>the request id</returns>
    /// <exception cref="NotConnectedException"></exception>
    /// <exception cref="TWSOutdatedException"></exception>
    public virtual int RequestContractDetails(IBContract contract, int requestId = 0)
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        // This feature is only available for versions of TWS >=4
        if (ServerInfo.Version < 4)
          throw new TWSOutdatedException();
        if (ServerInfo.Version < TWSServerInfo.MIN_SERVER_VER_SEC_ID_TYPE)
          if (contract.SecurityIdType != IBSecurityIdType.None || !String.IsNullOrEmpty(contract.SecurityId))
            throw new TWSOutdatedException();

        const int reqVersion = 6;

        try {
          // send req mkt data msg
          _enc.Encode(ServerMessage.RequestContractData);
          _enc.Encode(reqVersion);

          if (requestId == 0)
            requestId = NextValidId;

          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_CONTRACT_DATA_CHAIN) {
            _enc.Encode(requestId);
          }
          // send contract fields
          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_CONTRACT_CONID)
            _enc.Encode(contract.ContractId);
          _enc.Encode(contract.Symbol);
          _enc.Encode(contract.SecurityType);
          _enc.Encode(contract.Expiry.HasValue ? contract.Expiry.Value.ToString(IB_EXPIRY_DATE_FORMAT) : String.Empty);
          _enc.Encode(contract.Strike);
          _enc.Encode(contract.Right);
          if (ServerInfo.Version >= 15) {
            _enc.Encode(contract.Multiplier);
          }
          _enc.Encode(contract.Exchange);
          _enc.Encode(contract.Currency);
          _enc.Encode(contract.LocalSymbol);
          if (ServerInfo.Version >= 31)
            _enc.Encode(contract.IncludeExpired);
          if (ServerInfo.Version >= TWSServerInfo.MIN_SERVER_VER_SEC_ID_TYPE) {
            _enc.Encode(contract.SecurityIdType);
            _enc.Encode(contract.SecurityId);
          }
          return requestId;
        } catch (Exception) {
          Disconnect();
          throw;
        }
      }
    }

    public virtual void RequestCurrentTime()
    {
      lock (_socketLock) {
        // not connected?
        if (!IsConnected)
          throw new NotConnectedException();

        // This feature is only available for versions of TWS >= 33
        if (ServerInfo.Version < 33) {
          throw new TWSOutdatedException();
        }

        const int reqVersion = 1;

        try {
          _enc.Encode(ServerMessage.RequestCurrentTime);
          _enc.Encode(reqVersion);
        } catch (Exception e) {
          Disconnect();
          throw;
        }
      }
    }

    #endregion Request Methods

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

    #endregion Utilities

    private int NextValidId
    {
      get { return Interlocked.Increment(ref _nextValidId); }
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

    /// <summary>
    /// Gets or sets the end point this client is connected to
    /// </summary>
    /// <value>
    /// The end point.
    /// </value>
    /// <exception cref="System.Exception">Client already connected, cannot set the EndPoint</exception>
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

    /// <summary>
    /// Occurs when TWS sends a server generated error.
    /// </summary>
    public event EventHandler<TWSClientErrorEventArgs> Error;

    /// <summary>
    /// Occurs when an internal exception occurs in the client implementation
    /// </summary>
    public event EventHandler<TWSClientExceptionEventArgs> ExceptionOccured;

    /// <summary>
    /// Occurs when TWS sends a price market-data update/event
    /// </summary>
    public event EventHandler<TWSTickPriceEventArgs> TickPrice;

    /// <summary>
    /// Occurs when TWS sends a size market-data update/event
    /// </summary>
    public event EventHandler<TWSTickSizeEventArgs> TickSize;

    /// <summary>
    /// Occurs when TWS sends a string market-data update/event
    /// </summary>
    public event EventHandler<TWSTickStringEventArgs> TickString;

    /// <summary>
    /// Occurs when generic market-data update/event
    /// </summary>
    public event EventHandler<TWSTickGenericEventArgs> TickGeneric;

    public event EventHandler<TWSTickOptionComputationEventArgs> TickOptionComputation;

    public event EventHandler<TWSTickEFPEventArgs> TickEFP;

    public event EventHandler<TWSCurrentTimeEventArgs> CurrentTime;

    /// <summary>
    /// Occurs when an order's status is updated remotely,
    /// e.g. a fill event, cancellation confirmation etc.
    /// </summary>
    public event EventHandler<TWSOrderStatusEventArgs> OrderStatus;

    /// <summary>
    /// Occurs when an order is reported as active/working
    /// This event is sent for new orders place with the <see cref="PlaceOrder"/> method
    /// as well as for all working orders submitted by this <see cref="TWSClientId"/> upon initial connection
    /// </summary>
    public event EventHandler<TWSOpenOrderEventArgs> OpenOrder;

    public event EventHandler<TWSContractDetailsEventArgs> BondContractDetails;

    /// <summary>
    /// Occurs when TWS responds with retrieved contract details in response to
    /// a <see cref="RequestContractDetails"/>
#if NET_4_5
    /// or <see cref="GetContractDetailsAsync"/>
#endif
    /// call
    /// </summary>
    public event EventHandler<TWSContractDetailsEventArgs> ContractDetails;

    public event EventHandler<TWSScannerDataEventArgs> ScannerData;

    public event EventHandler<TWSScannerParametersEventArgs> ScannerParameters;

    public event EventHandler<TWSUpdatePortfolioEventArgs> UpdatePortfolio;

    /// <summary>
    /// Occurs when TWS responds with execution information for executed orders.
    /// This event is called for executed orders retrieved for a corresponding <see cref="RequestExecutions"/> call
    /// or for orders that we executed for this client while it was disconnected upon reconnection
    /// </summary>
    public event EventHandler<TWSExecutionDetailsEventArgs> ExecutionDetails;

    public event EventHandler<TWSMarketDepthEventArgs> MarketDepth;

    public event EventHandler<TWSMarketDepthEventArgs> MarketDepthL2;

    public event EventHandler<TWSHistoricalDataEventArgs> HistoricalData;

    public event EventHandler<TWSMarketDataEventArgs> MarketData;

    public event EventHandler<TWSRealtimeBarEventArgs> RealtimeBar;

    /// <summary>
    /// Occurs when one of the order related events occurs for an order sent through the api
    /// </summary>
    public event EventHandler<TWSOrderChangedEventArgs> OrderChanged;
  }

  public class NotConnectedException : Exception { }

  public class TWSOutdatedException : Exception { }

  public class TWSDisconnectedException : Exception { }

  public class TWSServerException : Exception
  {
    public TWSServerException(TWSError twsError)
    {
      TWSError = twsError;
    }

    public TWSError TWSError { get; private set; }

    public override string Message { get { return TWSError.Message; } }
  }

  internal class OrderRecord
  {
    internal int OrderId;
    internal IBContract Contract;
    internal IBOrder Order;
  }
}