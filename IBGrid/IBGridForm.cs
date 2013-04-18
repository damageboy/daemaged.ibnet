using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Daemaged.IBNet;
using Daemaged.IBNet.Client;
using Daemaged.IBNet.Playback;
using SourceGrid;
using SourceGrid.Cells;
using SourceGrid.Cells.Views;
using System.Linq;

namespace IBGrid
{
  public partial class IBGridForm : Form
  {
    private const IBGridColumn LAST_CONTRACT_DETAILS_COLUMN = IBGridColumn.Currency;
    private const IBGridColumn LAST_STATUS_DETAILS_COLUMN = IBGridColumn.VolumeMisses;

    private TWSClient _client;
    private TWSPlaybackClient _playback;
    private TWSClientSettings _settings;
    private RecordedInstruments _instruments;
    private Dictionary<IBContract, InstrumentDataRecord> _symbolDataMap;
    private string _filename;
    private PropertyForm _propertyForm;
    private int _logGridRow;
    private Dictionary<string, TextWriter> _logFiles;

    private IList<Grid> _grids;
    private SourceGrid.Cells.Views.ColumnHeader _columnHeaderView;
    private DevAge.Drawing.VisualElements.TextRenderer _textRenderer;
    private SourceGrid.Cells.Views.Cell _highlightView;
    private SourceGrid.Cells.Views.Cell _darkGreen;
    private SourceGrid.Cells.Views.Cell _lightGreen;
    private SourceGrid.Cells.Views.Cell _whiteView;
    private SourceGrid.Cells.Views.Cell _lightGray;
    private SourceGrid.Cells.Views.Cell _yellowView;
    private SourceGrid.Cells.Views.Cell _defaultView;
    private SourceGrid.Cells.Views.Cell _defaultViewClearType;

    private delegate void UpdateGridRowDelegate(TWSMarketDataSnapshot e, IBTickType t);

    private UpdateGridRowDelegate _updateGridRowDelegate;

    #region Constructor

    public IBGridForm()
    {
      InitializeComponent();

      _grids = new List<Grid>();
      _grids.Add(stocksGrid);
      _grids.Add(futuresGrid);
      _grids.Add(indicesGrid);
      _grids.Add(stockOptionsGrid);
      _grids.Add(futureOptionsGrid);
      _grids.Add(indexOptionsGrid);

      stocksGrid.Tag = IBSecurityType.Stock;
      futuresGrid.Tag = IBSecurityType.Future;
      indicesGrid.Tag = IBSecurityType.Index;
      stockOptionsGrid.Tag = IBSecurityType.Option;
      futureOptionsGrid.Tag = IBSecurityType.FutureOption;
      indexOptionsGrid.Tag = IBSecurityType.Option;

      SetupCommonGridResources();
      SetupMarketDataGrids();
      SetupLogGrid();

      _settings = new TWSClientSettings();

      _instruments = new RecordedInstruments();

      _logFiles = new Dictionary<string, TextWriter>();

      _logGridRow = 1;

      _updateGridRowDelegate = this.UpdateGridRow;
    }

    #endregion

    #region Grid Setup

    private void SetupMarketDataGrids()
    {
      foreach (var g in _grids)
        SetupMarketDataGrid(g);
    }

    private void SetupCommonGridResources()
    {
      _columnHeaderView = new SourceGrid.Cells.Views.ColumnHeader();
      _columnHeaderView.ElementText = new RotatedText(-60);
      _columnHeaderView.BackColor = Color.LightYellow;

      _textRenderer = new DevAge.Drawing.VisualElements.TextRenderer();

      _defaultView = new SourceGrid.Cells.Views.Cell();
      _defaultView.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;

      _defaultViewClearType = new SourceGrid.Cells.Views.Cell();
      _defaultViewClearType.ElementText = _textRenderer;

      _yellowView = _defaultView.Clone() as SourceGrid.Cells.Views.Cell;
      _yellowView.BackColor = Color.LightYellow;
      _yellowView.ElementText = _defaultView.ElementText;

      _lightGray = _defaultView.Clone() as SourceGrid.Cells.Views.Cell;
      _lightGray.BackColor = Color.LightGray;
      _lightGray.ElementText = _defaultView.ElementText;

      _whiteView = _defaultView.Clone() as SourceGrid.Cells.Views.Cell;
      _whiteView.BackColor = Color.White;
      _whiteView.ElementText = _defaultView.ElementText;

      _lightGreen = _defaultView.Clone() as SourceGrid.Cells.Views.Cell;
      _lightGreen.BackColor = Color.LightGreen;
      _lightGreen.ElementText = _defaultView.ElementText;

      _darkGreen = _defaultView.Clone() as SourceGrid.Cells.Views.Cell;
      _darkGreen.BackColor = Color.DarkGreen;
      _darkGreen.ForeColor = Color.White;
      _darkGreen.ElementText = _defaultView.ElementText;

      _highlightView = new SourceGrid.Cells.Views.Cell();
      _highlightView.BackColor = Color.LightPink;
      _highlightView.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;
      _highlightView.ElementText = _defaultView.ElementText;
    }

    private void SetupMarketDataGrid(Grid grid)
    {
      grid.Parent.Tag = grid;
      grid.Controller.AddController(new PopupSelection());
      grid.ColumnsCount = (int) IBGridColumn.LAST_COLUMN;
      grid.Rows.Insert(0);
      for (int i = 0; i < (int) IBGridColumn.LAST_COLUMN; i++)
      {
        grid[0, i] = new SourceGrid.Cells.ColumnHeader(((IBGridColumn) i).ToString());
        grid[0, i].View = _columnHeaderView;
      }
      grid.Rows[0].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize;

      grid.Rows.Insert(1);

      Type cft = typeof (IBSimplefiedContract);
      foreach (var p in cft.GetProperties())
      {
        var attrs = from a in p.GetCustomAttributes(false)
                    where a is GridCellInfoAttribute
                    select (a as GridCellInfoAttribute).Column;

        if (attrs.Count() == 0)
          continue;

        grid[1, (int) attrs.First()] = new SourceGrid.Cells.Cell(null, p.PropertyType);
        grid[1, (int) attrs.First()].Tag = p.PropertyType;
        grid[1, (int) attrs.First()].View = _yellowView;
      }
      grid.Rows[1].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize;

      SetupMarketDataGridRow(grid, 1);
      grid.AutoSizeCells();
      SetupMarketDataGridColumns(grid);
    }

    private void SetupLogGrid()
    {
      logGrid.ColumnsCount = 4;
      int i = 0;
      logGrid.Rows.Insert(0);

      logGrid[0, i++] = new SourceGrid.Cells.ColumnHeader("Time");
      logGrid[0, i++] = new SourceGrid.Cells.ColumnHeader("TickerId");
      logGrid[0, i++] = new SourceGrid.Cells.ColumnHeader("Code");
      logGrid[0, i++] = new SourceGrid.Cells.ColumnHeader("Message");

      logGrid.Columns[3].AutoSizeMode = SourceGrid.AutoSizeMode.EnableStretch;
      logGrid.Rows[0].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize;
      logGrid.AutoSizeCells();
    }


    private void SetupMarketDataGridColumns(Grid grid)
    {
      IBSecurityType secType = (IBSecurityType) grid.Tag;
      if (secType != IBSecurityType.FutureOption &&
          secType != IBSecurityType.Option)
      {
        grid.Columns[(int) IBGridColumn.AskDelta].Visible = false;
        grid.Columns[(int) IBGridColumn.BidDelta].Visible = false;
        grid.Columns[(int) IBGridColumn.BidImpVol].Visible = false;
        grid.Columns[(int) IBGridColumn.AskImpVol].Visible = false;
        grid.Columns[(int) IBGridColumn.Delta].Visible = false;
        grid.Columns[(int) IBGridColumn.Volatility].Visible = false;
        grid.Columns[(int) IBGridColumn.Price].Visible = false;
        grid.Columns[(int) IBGridColumn.PVDividend].Visible = false;
      }
    }

    private void SetupMarketDataGridRow(Grid grid, int r)
    {
      if (grid.Rows.Count <= r)
      {
        grid.Rows.Insert(r);
      }
      for (int i = 0; i <= (int) LAST_CONTRACT_DETAILS_COLUMN; i++)
      {
        Type t = grid[1, (int) i].Tag as Type;
        grid[r, (int) i] = new SourceGrid.Cells.Cell(null, t);
        grid[r, (int) i].Tag = t;
        grid[r, (int) i].View = _yellowView;
      }

      for (int i = (int) LAST_CONTRACT_DETAILS_COLUMN + 1; i < (int) IBGridColumn.LAST_COLUMN; i++)
      {
        ICell cell = new SourceGrid.Cells.Cell(0.0);
        grid[r, i] = cell;
        bool alternate = (r%2 != 0);
        if (i <= (int) LAST_STATUS_DETAILS_COLUMN)
          cell.View = alternate ? _lightGreen : _darkGreen;
        else
          cell.View = alternate ? _whiteView : _lightGray;
      }
      grid.Rows[r].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize;
    }

    #endregion

    #region Form Event Handlers

    private void AddNewItemToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Grid g = gridTab.SelectedTab.Tag as Grid;
      int row = g.Selection.ActivePosition.Row + 1;

      g.Rows.Insert(row);
      SetupMarketDataGridRow(g, row);
    }

    private void ClearLineToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Grid g = gridTab.SelectedTab.Tag as Grid;
      int row = g.Selection.ActivePosition.Row;
      SetupMarketDataGridRow(g, row);
    }

    private void ConnectButton_Click(object sender, EventArgs e)
    {
      ConnectForm cf = new ConnectForm();

      if (cf.ShowDialog() != DialogResult.OK)
        return;

      string host = cf.HostTextBox.Text;
      int port = Int32.Parse(cf.PortTextBox.Text);

      ConnectToTWS(host, port);
    }

    private void ConnectLocalButton_Click(object sender, EventArgs e)
    {
      ConnectToTWS("127.0.0.1", 7496);
    }

    private void DisconnectButton_Click(object sender, EventArgs e)
    {
      DisconnectFromTWS();
    }

    private void Form_FormClosed(object sender, FormClosedEventArgs e)
    {
      Environment.Exit(0);
    }

    private void LogSizeTextBoxTimer_Tick(object sender, EventArgs e)
    {
      if (_client.RecordStream != null)
      {
        Stream fs = _client.RecordStream;

        if (fs.Length > 1024*1024)
        {
          if (fs.Length > 1024*1024*1024)
            logSizeTextBox.Text = String.Format("{0:F2} GB", fs.Length/(1024*1024*1024.0));
          else
            logSizeTextBox.Text = String.Format("{0:F2} MB", fs.Length/(1024*1024.0));
        }
        else
          logSizeTextBox.Text = String.Format("{0:F2} KB", fs.Length/(1024.0));
      }
    }

    private void OpenButton_ButtonClick(object sender, EventArgs e)
    {
      string filename;
      using (OpenFileDialog ofd = new OpenFileDialog())
      {
        ofd.Filter = ("IML Files|*.iml");
        if (ofd.ShowDialog() != DialogResult.OK)
          return;

        filename = ofd.SafeFileName;
      }

      LoadInstrumens(filename);
    }

    private void OpenButton_DropDownOpening(object sender, EventArgs e)
    {
      openButton.DropDownItems.Clear();
      foreach (var f in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.iml"))
      {
        var fi = new FileInfo(f);
        var item = new ToolStripMenuItem() {Text = fi.Name, Tag = f};
        item.Click += new System.EventHandler(this.QuickAccessItem_Click);
        openButton.DropDownItems.Add(item);
      }
    }

    private void PlaybackRewindButton_Click(object sender, EventArgs e)
    {
      _playback.Reset();
    }

    private void PlaybackPauseButton_Click(object sender, EventArgs e)
    {
      _playback.Pause();
      playbackPauseButton.Enabled = _playback.IsRunning;
      playbackPlayButton.Enabled = !_playback.IsRunning;
    }

    private void PlaybackStepButton_Click(object sender, EventArgs e)
    {
      _playback.Pause();
      _playback.Step();

    }

    private void PlaybackPlayButton_Click(object sender, EventArgs e)
    {
      _playback.Speed = PlaybackSpeed.Normal;
      _playback.Start();
    }

    private void PlaybackFastForwardButton_Click(object sender, EventArgs e)
    {
      _playback.Speed = PlaybackSpeed.FullSpeedProcessing;
    }

    private void PlaybackStopButton_Click(object sender, EventArgs e)
    {
      _playback.Stop();
    }


    private void PropertiesButton_Click(object sender, EventArgs e)
    {
      if (propertiesButton.CheckState == CheckState.Checked)
      {
        if (_propertyForm == null || _propertyForm.IsDisposed)
          return;
        _propertyForm.Hide();
        propertiesButton.CheckState = CheckState.Unchecked;
      }
      else if (propertiesButton.CheckState == CheckState.Unchecked)
      {
        if (_propertyForm == null)
        {
          _propertyForm = new PropertyForm();
          _propertyForm.PropertyGrid.SelectedObject = _settings;
        }
        _propertyForm.Show();
        propertiesButton.CheckState = CheckState.Checked;
      }
    }

    private void ResizeButton_Click(object sender, EventArgs e)
    {
      foreach (var g in _grids)
      {
        g.AutoSizeCells();
        SetupMarketDataGridColumns(g);
      }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      SaveInstruments(_filename);
    }

    private void QuickAccessItem_Click(object sender, EventArgs e)
    {
      LoadInstrumens((string) (sender as ToolStripMenuItem).Tag);
    }

    private void SaveAsButton_Click(object sender, EventArgs e)
    {
      using (SaveFileDialog sfd = new SaveFileDialog())
      {
        sfd.Filter = "IML Files|*.iml";
        sfd.OverwritePrompt = true;

        if (sfd.ShowDialog() != DialogResult.OK)
          return;

        _filename = sfd.FileName;
      }
      SaveInstruments(_filename);
    }

    private void RecordButtonBlinkTimer_Tick(object sender, EventArgs e)
    {
      if (recordMarketDataButton.BackColor == Color.Red)
        recordMarketDataButton.BackColor = SystemColors.Control;
      else
        recordMarketDataButton.BackColor = Color.Red;
    }

    private void RecordMarketDataButton_Click(object sender, EventArgs e)
    {
      if (recordMarketDataButton.Tag == null || (bool) recordMarketDataButton.Tag == false)
        recordButtonBlinkTimer.Start();
      else
        recordButtonBlinkTimer.Stop();

      recordMarketDataButton.Tag = recordButtonBlinkTimer.Enabled;
    }

    private void OpenLogFileButton_Click(object sender, EventArgs e)
    {
      string fileName;
      using (var ofd = new OpenFileDialog())
      {
        ofd.Filter = "Log Files|*.log";
        if (ofd.ShowDialog() != DialogResult.OK)
          return;
        fileName = ofd.FileName;
      }

      ConnectToPlayback(fileName);
    }

    #endregion

    #region TWS Client Events

    private void Client_Error(object sender, TWSClientErrorEventArgs e)
    {
      if (InvokeRequired)
        Invoke(new EventHandler<TWSClientErrorEventArgs>(Client_Error), sender, e);
      else
      {
        int i = 0;
        logGrid.Rows.Insert(_logGridRow);
        logGrid[_logGridRow, i] = new SourceGrid.Cells.Cell(DateTime.Now);
        logGrid[_logGridRow, i].View = _defaultViewClearType;
        logGrid[_logGridRow, 1] = new SourceGrid.Cells.Cell(e.RequestId);
        logGrid[_logGridRow, 2] = new SourceGrid.Cells.Cell(e.Error.Code);
        logGrid[_logGridRow, 3] = new SourceGrid.Cells.Cell(e.Error.Message);
      }
    }

    private void Client_MarketData(object sender, TWSMarketDataEventArgs e)
    {
      if (InvokeRequired)
      {
        TWSMarketDataSnapshot s = e.Snapshot.Clone() as TWSMarketDataSnapshot;
        BeginInvoke(_updateGridRowDelegate, s, e.TickType);

      }
      else
        _updateGridRowDelegate(e.Snapshot, e.TickType);
    }

    private void Client_MarketDataLogger(object sender, TWSMarketDataEventArgs e)
    {
      if (e.Snapshot.Contract.SecurityType == IBSecurityType.Index)
      {
        if (e.TickType != IBTickType.LastPrice)
          return;
      }
      else if (e.TickType != IBTickType.LastSize &&
               e.TickType != IBTickType.AskSize &
               e.TickType != IBTickType.BidSize)
        return;

      var s = e.Snapshot;

      string symbol = e.Snapshot.Contract.Symbol + "-" + e.Snapshot.Contract.SecurityType;
      TextWriter sw;
      if (!_logFiles.TryGetValue(symbol, out sw))
      {
        _logFiles.Add(symbol, sw = new StreamWriter(symbol + ".log"));
        sw.WriteLine("ts,price,size,type");
      }

      if (e.TickType == IBTickType.LastSize)
        WriteToLog(sw, s.TradeTimeStamp, s.Last, s.LastSize, e.TickType);
      if (e.TickType == IBTickType.AskSize)
        WriteToLog(sw, s.AskTimeStamp, s.Ask, s.AskSize, e.TickType);
      if (e.TickType == IBTickType.BidSize)
        WriteToLog(sw, s.BidTimeStamp, s.Bid, s.BidSize, e.TickType);
      if (e.TickType == IBTickType.LastPrice)
        WriteToLog(sw, s.TradeTimeStamp, s.Last, 0, e.TickType);

    }

    private void WriteToLog(TextWriter file, DateTime ts, double last, int size, IBTickType tickType)
    {
      file.WriteLine("{0},{1},{2},{3}", ts.Ticks, last, size, (int) tickType);
    }


    private void Client_StatusChanged(object sender, TWSClientStatusEventArgs e)
    {
      if (InvokeRequired)
        this.BeginInvoke(new EventHandler<TWSClientStatusEventArgs>(Client_StatusChanged));
      else
      {
        statusButton.Image = (_client.IsConnected)
                               ? IBGrid.Properties.Resources.bullet_square_green
                               : IBGrid.Properties.Resources.bullet_square_red;
        statusButton.Text = statusButton.ToolTipText = e.Status.ToString();
        if (_client.IsConnected)
        {
          _symbolDataMap.Clear();
          UpdateContractListsFromGrid();
          RegisterContractLists();
          logSizeTextBoxTimer.Start();
        }
        else
        {
          logSizeTextBoxTimer.Stop();
        }
      }
    }

    #endregion

    #region Connection Functions

    private void ConnectToPlayback(string fileName)
    {
      _client = _playback = new TWSPlaybackClient(fileName);
      _client.Settings = _settings;

      AddGenericClientEventHandlers();

      _symbolDataMap = new Dictionary<IBContract, InstrumentDataRecord>();
    }


    private void ConnectToTWS(string host, int port)
    {
      _client = TWSSocketFactory.GetSocket(host, port);
      _client.RecordForPlayback = (bool) (recordMarketDataButton.Tag ?? false);
      _client.Settings = _settings;

      AddGenericClientEventHandlers();

      _symbolDataMap = new Dictionary<IBContract, InstrumentDataRecord>();
      _client.Connect();
    }

    private void DisconnectFromTWS()
    {
      _client.Disconnect();

      RemoveGenericClientEventHandlers();
      _symbolDataMap.Clear();
      foreach (TextWriter tw in _logFiles.Values)
      {
        tw.Flush();
        tw.Close();
      }
      _logFiles.Clear();

      logSizeTextBoxTimer.Stop();
    }

    private void AddGenericClientEventHandlers()
    {
      _client.StatusChanged += Client_StatusChanged;
      _client.MarketData += Client_MarketData;
      _client.MarketData += Client_MarketDataLogger;
      _client.Error += Client_Error;
    }

    private void RemoveGenericClientEventHandlers()
    {
      _client.StatusChanged -= Client_StatusChanged;
      _client.MarketData -= Client_MarketData;
      _client.MarketData -= Client_MarketDataLogger;
      _client.Error -= Client_Error;
    }

    #endregion

    #region Utilities

    private void RegisterContractList(IList<IBSimplefiedContract> list, Grid grid)
    {
      var i = 1;
      foreach (var c in list)
      {
        var contract = new IBContract
          {
            Currency = c.Currency,
            Symbol = c.Symbol,
            Exchange = c.Exchange,
            SecurityType = c.SecurityType,
            Expiry = c.Expiry,
            Multiplier = c.Multiplier.ToString(),
            Strike = c.Strike,
            Right = c.PutOrCall
          };

        _client.RequestMarketData(contract, null);
        _symbolDataMap.Add(contract, new InstrumentDataRecord() {Grid = grid, Row = i});
        i++;
      }
    }

    private void RegisterContractLists()
    {
      RegisterContractList(_instruments.Options, indexOptionsGrid);
      RegisterContractList(_instruments.Stocks, stocksGrid);
      RegisterContractList(_instruments.FutureOptions, futureOptionsGrid);
      RegisterContractList(_instruments.Futures, futuresGrid);
      RegisterContractList(_instruments.StockOptions, stockOptionsGrid);
      RegisterContractList(_instruments.Indices, indicesGrid);
    }

    #endregion

    #region Debug Functions

    private void DumpContract(IBContract c)
    {
      Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5} {6}", c.Symbol, c.SecurityType, c.Expiry, c.Strike, c.Right,
                                    c.Exchange, c.Currency));
    }

    #endregion

    #region Grid Updates

    private void UpdateGridRow(TWSMarketDataSnapshot s, IBTickType t)
    {
      InstrumentDataRecord record = _symbolDataMap[s.Contract];

      record.Snapshot = s;
      // Make sure we don't crash and burn on the first invocation
      if (record.PreviousSnapshot == null)
        record.PreviousSnapshot = record.Snapshot;

      Grid g = record.Grid;
      int r = record.Row;
      TWSMarketDataSnapshot p = record.PreviousSnapshot;

      try
      {
        UnHighlightCells(record);

        if (s.LastTimeStamp != p.LastTimeStamp) SetValue(record, IBGridColumn.UpdateTime, s.LastTimeStamp);
        if (s.Last != p.Last) SetValue(record, IBGridColumn.LastPrice, s.Last);
        if (s.LastSize != p.LastSize) SetValue(record, IBGridColumn.LastSize, s.LastSize);
        if (s.Volume != p.Volume) SetValue(record, IBGridColumn.Volume, s.Volume);
        if (s.VolumeEvents != p.VolumeEvents) SetValue(record, IBGridColumn.VolumeEvents, s.VolumeEvents);
        if (s.VolumeMisses != p.VolumeMisses) SetValue(record, IBGridColumn.VolumeMisses, s.VolumeMisses);

        if (s.TradeDups != p.TradeDups) SetValue(record, IBGridColumn.TradeDups, s.TradeDups);
        if (s.BidDups != p.BidDups) SetValue(record, IBGridColumn.BidDups, s.BidDups);
        if (s.AskDups != p.AskDups) SetValue(record, IBGridColumn.AskDups, s.AskDups);

        if (s.Bid != p.Bid) SetValue(record, IBGridColumn.BidPrice, s.Bid);
        if (s.Ask != p.Ask) SetValue(record, IBGridColumn.AskPrice, s.Ask);
        if (s.BidSize != p.BidSize) SetValue(record, IBGridColumn.BidSize, s.BidSize);
        if (s.AskSize != p.AskSize) SetValue(record, IBGridColumn.AskSize, s.AskSize);
        if (s.Close != p.Close) SetValue(record, IBGridColumn.Close, s.Close);
        if (s.High != p.High) SetValue(record, IBGridColumn.High, s.High);
        if (s.Low != p.Low) SetValue(record, IBGridColumn.Low, s.Low);
        if (s.SyntheticVolume != p.SyntheticVolume) SetValue(record, IBGridColumn.SyntheticVolume, s.SyntheticVolume);
        if (s.TradeEvents != p.TradeEvents) SetValue(record, IBGridColumn.TradeEvents, s.TradeEvents);
        if (s.BidEvents != p.BidEvents) SetValue(record, IBGridColumn.BidEvents, s.BidEvents);
        if (s.AskEvents != p.AskEvents) SetValue(record, IBGridColumn.AskEvents, s.AskEvents);

        if (s.Contract.SecurityType != IBSecurityType.Option &&
            s.Contract.SecurityType != IBSecurityType.FutureOption)
          return;

        if (s.AskImpliedVol != p.AskImpliedVol) SetValue(record, IBGridColumn.AskImpVol, s.AskImpliedVol);
        if (s.BidImpliedVol != p.BidImpliedVol) SetValue(record, IBGridColumn.BidImpVol, s.BidImpliedVol);
        if (s.AskDelta != p.AskDelta) SetValue(record, IBGridColumn.AskDelta, s.AskDelta);
        if (s.BidDelta != p.BidDelta) SetValue(record, IBGridColumn.BidDelta, s.BidDelta);
        if (s.Delta != p.Delta) SetValue(record, IBGridColumn.Delta, s.Delta);
        if (s.ImpliedVol != p.ImpliedVol) SetValue(record, IBGridColumn.Volatility, s.ImpliedVol);
        if (s.ModelPrice != p.ModelPrice) SetValue(record, IBGridColumn.Price, s.ModelPrice);
        if (s.PVDividend != p.PVDividend) SetValue(record, IBGridColumn.PVDividend, s.PVDividend);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
        record.PreviousSnapshot = record.Snapshot;
        record.Snapshot = null;
      }

    }

    private void UnHighlightCells(InstrumentDataRecord record)
    {
      foreach (var entry in record.PreviousHighlights)
      {
        entry.Key.View = entry.Value;
        entry.Key.Grid.InvalidateCell(entry.Key);
      }

      record.PreviousHighlights.Clear();
    }

    private void HighLightCell(InstrumentDataRecord record, IBGridColumn col, ICell cell)
    {
      record.PreviousHighlights.Add(new KeyValuePair<ICell, IView>(cell, cell.View));
      cell.View = _highlightView;
    }

    private void SetValue(InstrumentDataRecord record, IBGridColumn col, DateTime value)
    {
      record.Grid[record.Row, (int) col].Value = value.ToShortTimeString();
      HighLightCell(record, col, record.Grid[record.Row, (int) col]);
    }

    private void SetValue(InstrumentDataRecord record, IBGridColumn col, int value)
    {
      record.Grid[record.Row, (int) col].Value = value;
      HighLightCell(record, col, record.Grid[record.Row, (int) col]);
    }

    private void SetValue(InstrumentDataRecord record, IBGridColumn col, double value)
    {
      if (value != Double.MaxValue)
        record.Grid[record.Row, (int) col].Value = String.Format("{0:F2}", value);
      else
        record.Grid[record.Row, (int) col].Value = "";
      HighLightCell(record, col, record.Grid[record.Row, (int) col]);
    }

    #endregion

    #region Load/Save Functions

    private void LoadInstrumens(string filename)
    {
      XmlSerializer xs = new XmlSerializer(typeof (RecordedInstruments));

      Cursor = Cursors.WaitCursor;

      using (XmlTextReader reader = new XmlTextReader(filename))
      {
        _instruments = (RecordedInstruments) xs.Deserialize(reader);
        reader.Close();
      }

      UpdateGridsFromContractLists();
      Cursor = Cursors.Default;

      _filename = filename;

    }

    private void SaveInstruments(string filename)
    {
      UpdateContractListsFromGrid();

      XmlSerializer xs = new XmlSerializer(typeof (RecordedInstruments));

      using (XmlTextWriter writer = new XmlTextWriter(filename, System.Text.Encoding.UTF8))
      {
        writer.Formatting = Formatting.Indented;
        xs.Serialize(writer, _instruments);
        writer.Flush();
        writer.Close();
      }
    }


    private void UpdateContractListFromGrid(IList<IBSimplefiedContract> contracts, Grid grid)
    {
      contracts.Clear();
      for (int i = 1; i < grid.Rows.Count; i++)
      {
        try
        {
          var contract = new IBSimplefiedContract();

          // SecurityType always has to be present...
          contract.SecurityType = (IBSecurityType) grid[i, (int) IBGridColumn.SecType].Value;

          // String fields are always "safe"
          contract.Currency = (string) grid[i, (int) IBGridColumn.Currency].Value;
          contract.Exchange = (string) grid[i, (int) IBGridColumn.Exchange].Value;
          contract.Symbol = (string) grid[i, (int) IBGridColumn.Symbol].Value;
          contract.Rollover = (string) grid[i, (int) IBGridColumn.Rollover].Value;
          contract.PutOrCall = (string) grid[i, (int) IBGridColumn.PutOrCall].Value;

          if (contract.SecurityType == IBSecurityType.FutureOption ||
              contract.SecurityType == IBSecurityType.Option)
          {
            contract.Expiry = (DateTime) grid[i, (int) IBGridColumn.Expiry].Value;
            contract.Strike = (double) grid[i, (int) IBGridColumn.Strike].Value;
            contract.Multiplier = (int) grid[i, (int) IBGridColumn.Multiplier].Value;
          }

          if (contract.SecurityType == IBSecurityType.FutureOption)
          {
            contract.Expiry = (DateTime) grid[i, (int) IBGridColumn.Expiry].Value;
            contract.Multiplier = (int) grid[i, (int) IBGridColumn.Multiplier].Value;
          }


          contracts.Add(contract);
        }
        catch (NullReferenceException)
        {
        }
      }
    }

    private void UpdateContractListsFromGrid()
    {
      UpdateContractListFromGrid(_instruments.Options, indexOptionsGrid);
      UpdateContractListFromGrid(_instruments.Stocks, stocksGrid);
      UpdateContractListFromGrid(_instruments.StockOptions, stockOptionsGrid);
      UpdateContractListFromGrid(_instruments.Futures, futuresGrid);
      UpdateContractListFromGrid(_instruments.FutureOptions, futureOptionsGrid);
      UpdateContractListFromGrid(_instruments.Indices, indicesGrid);
    }


    private void UpdateGridFromContractLists(IList<IBSimplefiedContract> contracts, Grid grid)
    {
      int i = 1;
      foreach (var contract in contracts)
      {
        SetupMarketDataGridRow(grid, i);
        grid[i, (int) IBGridColumn.Symbol].Value = contract.Symbol;
        grid[i, (int) IBGridColumn.SecType].Value = contract.SecurityType;
        grid[i, (int) IBGridColumn.Expiry].Value = contract.Expiry;
        grid[i, (int) IBGridColumn.Rollover].Value = contract.Rollover;
        grid[i, (int) IBGridColumn.Strike].Value = contract.Strike;
        grid[i, (int) IBGridColumn.PutOrCall].Value = contract.PutOrCall;
        grid[i, (int) IBGridColumn.Multiplier].Value = contract.Multiplier;
        grid[i, (int) IBGridColumn.Exchange].Value = contract.Exchange;
        grid[i, (int) IBGridColumn.Currency].Value = contract.Currency;
        i++;
      }
      SetupMarketDataGridRow(grid, i);
      grid.AutoSizeCells();
    }

    private void UpdateGridsFromContractLists()
    {
      UpdateGridFromContractLists(_instruments.Options, indexOptionsGrid);
      UpdateGridFromContractLists(_instruments.Stocks, stocksGrid);
      UpdateGridFromContractLists(_instruments.FutureOptions, futureOptionsGrid);
      UpdateGridFromContractLists(_instruments.Futures, futuresGrid);
      UpdateGridFromContractLists(_instruments.StockOptions, stockOptionsGrid);
      UpdateGridFromContractLists(_instruments.Indices, indicesGrid);
    }

    #endregion

    private void logGrid_Paint(object sender, PaintEventArgs e)
    {

    }
  }
}