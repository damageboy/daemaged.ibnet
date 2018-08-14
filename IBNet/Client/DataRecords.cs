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
  public class TWSMarketDataSnapshot : ICloneable
  {
    private int _askSize;
    private int _bidSize;
    private int _lastSize;
    private int _volume;
    private int _volumeDiff;

    public TWSMarketDataSnapshot(IBContract contract, int reqId)
    {
      Contract = contract;
      RequestId = reqId;
    }

    public TWSMarketDataSnapshot(TWSMarketDataSnapshot that)
    {
      _askSize = that._askSize;
      _bidSize = that._bidSize;
      _lastSize = that._lastSize;
      _volume = that._volume;

      Contract = that.Contract;
      Ask = that.Ask;
      Bid = that.Bid;
      Close = that.Close;
      Open = that.Open;
      High = that.High;
      Low = that.Low;
      Last = that.Last;
      SyntheticVolume = that.SyntheticVolume;
      VolumeEvents = that.VolumeEvents;
      VolumeMisses = that.VolumeMisses;
      TradeEvents = that.TradeEvents;
      TradeDups = that.TradeDups;
      BidEvents = that.BidEvents;
      AskEvents = that.AskEvents;
      AskDups = that.AskDups;
      BidDups = that.BidDups;
      TradeTimeStamp = that.TradeTimeStamp;
      BidTimeStamp = that.BidTimeStamp;
      AskTimeStamp = that.AskTimeStamp;
      Delta = that.Delta;
      ImpliedVol = that.ImpliedVol;
      BidDelta = that.BidDelta;
      BidImpliedVol = that.BidImpliedVol;
      AskDelta = that.AskDelta;
      AskImpliedVol = that.AskImpliedVol;
      PVDividend = that.PVDividend;
      ModelPrice = that.ModelPrice;
    }

    public IBContract Contract { get; set; }
    public double Ask { get; set; }
    public double Bid { get; set; }
    public double Close { get; set; }
    public double High { get; set; }
    public double Open { get; set; }
    public double Low { get; set; }
    public double Last { get; set; }
    public double BidDelta { get; set; }
    public double AskDelta { get; set; }
    public double Delta { get; set; }
    public double ImpliedVol { get; set; }
    public double BidImpliedVol { get; set; }
    public double AskImpliedVol { get; set; }
    public double PVDividend { get; set; }
    public double ModelPrice { get; set; }
    public DateTime LastTimeStamp { get; set; }
    public int RequestId { get; set; }

    public int Volume
    {
      get { return _volume; }
      set
      {
        _volume = value;
        VolumeEvents++;

        if ((_volume - _volumeDiff) == SyntheticVolume) return;
        VolumeMisses++;
        _volumeDiff = _volume - SyntheticVolume;
      }
    }

    public int LastSize
    {
      get { return _lastSize; }
      set
      {
        // We got the first notification of today's volume
        // after we had stored yesterday's volume
        if (TradeEvents == 0)
          SyntheticVolume = Volume;

        _lastSize = value;
        TradeEvents++;

        SyntheticVolume += _lastSize;
      }
    }

    public int BidSize
    {
      get { return _bidSize; }
      set
      {
        _bidSize = value;
        BidEvents++;
      }
    }

    public int AskSize

    {
      get { return _askSize; }
      set
      {
        _askSize = value;
        AskEvents++;
      }
    }


    // Statistical data fields that help
    // verify popert collection
    public int VolumeEvents { get; internal set; }
    public int VolumeMisses { get; internal set; }
    public int TradeEvents { get; internal set; }
    public int BidEvents { get; internal set; }
    public int AskEvents { get; internal set; }
    public DateTime TradeTimeStamp { get; set; }
    public DateTime BidTimeStamp { get; set; }
    public DateTime AskTimeStamp { get; set; }
    public int AskDups { get; set; }
    public int BidDups { get; set; }
    public int TradeDups { get; set; }
    public int SyntheticVolume { get; internal set; }

    #region ICloneable Members

    public object Clone()
    {
      return new TWSMarketDataSnapshot(this);
    }

    #endregion
  }
}