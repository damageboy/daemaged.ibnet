using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daemaged.IBNet;
using Daemaged.IBNet.Client;
using SourceGrid;
using SourceGrid.Cells;
using SourceGrid.Cells.Views;

namespace IBGrid
{
    public enum IBGridColumn
    {
        Symbol = 0,
        SecType,
        Expiry,
        Rollover,
        Strike,
        PutOrCall,
        Multiplier,
        Exchange,
        Currency,        
        Status,
        UpdateTime,
        TradeEvents,
        TradeDups,
        BidEvents,
        BidDups,
        AskEvents,
        AskDups,
        VolumeResets,       
        VolumeEvents,
        VolumeMisses,
        BidImpVol,
        BidDelta,
        BidSize,
        BidPrice,
        AskPrice,
        AskSize,
        AskImpVol,
        AskDelta,
        LastPrice,
        LastSize,
        High,
        Low,
        Volume,
        SyntheticVolume,
        Close,
        Volatility,
        Delta,
        Price,
        PVDividend,
        LAST_COLUMN,
    }

    public class GridCellInfoAttribute : Attribute
    {
        public GridCellInfoAttribute(IBGridColumn column, Type type)
        {
            Type = type;
            Column = column;
        }
        public GridCellInfoAttribute(IBGridColumn column)
        {
            Column = column;
        }


        public Type Type { get; private set; }
        public IBGridColumn Column { get; private set; }
    }

    public class IBSimplefiedContract
    {        
        [GridCellInfo(IBGridColumn.Symbol)]
        public string Symbol { get; set; }
        [GridCellInfo(IBGridColumn.SecType)]
        public IBSecurityType SecurityType { get; set; }
        [GridCellInfo(IBGridColumn.Expiry)]
        public DateTime Expiry { get; set; }
        [GridCellInfo(IBGridColumn.Rollover)]
        public string Rollover { get; set; }
        [GridCellInfo(IBGridColumn.Strike)]
        public double Strike { get; set; }
        [GridCellInfo(IBGridColumn.PutOrCall)]
        public string PutOrCall { get; set; }
        [GridCellInfo(IBGridColumn.Multiplier)]
        public int Multiplier { get; set; }
        [GridCellInfo(IBGridColumn.Exchange)]
        public string Exchange { get; set; }
        [GridCellInfo(IBGridColumn.Currency)]
        public string Currency {get; set;}
    }

    public class InstrumentDataRecord
    {
        internal IList<KeyValuePair<ICell, IView>> PreviousHighlights { get; set; }
        internal Grid Grid { get; set; }
        internal int Row { get; set; }
        internal TWSMarketDataSnapshot Snapshot { get; set; }
        internal TWSMarketDataSnapshot PreviousSnapshot { get; set; }
        internal bool Updated { get; set; }

        public InstrumentDataRecord()
        {
            PreviousHighlights = new List<KeyValuePair<ICell, IView>>();
        }
    }

    public class RecordedInstruments
    {
        public List<IBSimplefiedContract> Options { get; set; }
        public List<IBSimplefiedContract> Stocks { get; set; }
        public List<IBSimplefiedContract> StockOptions { get; set; }
        public List<IBSimplefiedContract> Indices { get; set; }
        public List<IBSimplefiedContract> Futures { get; set; }
        public List<IBSimplefiedContract> FutureOptions { get; set; }


        public RecordedInstruments()
        {
            Options = new List<IBSimplefiedContract>();
            Stocks = new List<IBSimplefiedContract>();
            StockOptions = new List<IBSimplefiedContract>();
            Indices = new List<IBSimplefiedContract>();
            Futures = new List<IBSimplefiedContract>();
            FutureOptions = new List<IBSimplefiedContract>();
        }


    }
}
