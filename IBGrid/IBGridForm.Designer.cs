namespace IBGrid
{
    partial class IBGridForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.openButton = new System.Windows.Forms.ToolStripSplitButton();
            this.sampleQuickAccessItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.saveAsButton = new System.Windows.Forms.ToolStripButton();
            this.connectButton = new System.Windows.Forms.ToolStripButton();
            this.connectLocalButton = new System.Windows.Forms.ToolStripButton();
            this.disconnectButton = new System.Windows.Forms.ToolStripButton();
            this.statusButton = new System.Windows.Forms.ToolStripButton();
            this.propertiesButton = new System.Windows.Forms.ToolStripButton();
            this.resizeButton = new System.Windows.Forms.ToolStripButton();
            this.recordMarketDataButton = new System.Windows.Forms.ToolStripButton();
            this.sizeLabel = new System.Windows.Forms.ToolStripLabel();
            this.logSizeTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.gridTab = new System.Windows.Forms.TabControl();
            this.indicesPage = new System.Windows.Forms.TabPage();
            this.indicesGrid = new SourceGrid.Grid();
            this.gridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.stopMarketDataRetrievalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMarketDataRetrievalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addNewItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.futuresPage = new System.Windows.Forms.TabPage();
            this.futuresGrid = new SourceGrid.Grid();
            this.stocksGridPage = new System.Windows.Forms.TabPage();
            this.stocksGrid = new SourceGrid.Grid();
            this.stockOptionsPage = new System.Windows.Forms.TabPage();
            this.stockOptionsGrid = new SourceGrid.Grid();
            this.indexOptionsGridPage = new System.Windows.Forms.TabPage();
            this.indexOptionsGrid = new SourceGrid.Grid();
            this.futureOptionsPage = new System.Windows.Forms.TabPage();
            this.futureOptionsGrid = new SourceGrid.Grid();
            this.logPage = new System.Windows.Forms.TabPage();
            this.logGrid = new SourceGrid.Grid();
            this.skewPage = new System.Windows.Forms.TabPage();
            this.playbackToolStrip = new System.Windows.Forms.ToolStrip();
            this.openLogFileButton = new System.Windows.Forms.ToolStripButton();
            this.playbackRewindButton = new System.Windows.Forms.ToolStripButton();
            this.playbackPauseButton = new System.Windows.Forms.ToolStripButton();
            this.playbackStepButton = new System.Windows.Forms.ToolStripButton();
            this.playbackPlayButton = new System.Windows.Forms.ToolStripButton();
            this.playbackFastForwardButton = new System.Windows.Forms.ToolStripButton();
            this.playbackStopButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.recordButtonBlinkTimer = new System.Windows.Forms.Timer(this.components);
            this.logSizeTextBoxTimer = new System.Windows.Forms.Timer(this.components);
            this.mainToolStrip.SuspendLayout();
            this.gridTab.SuspendLayout();
            this.indicesPage.SuspendLayout();
            this.gridContextMenu.SuspendLayout();
            this.futuresPage.SuspendLayout();
            this.stocksGridPage.SuspendLayout();
            this.stockOptionsPage.SuspendLayout();
            this.indexOptionsGridPage.SuspendLayout();
            this.futureOptionsPage.SuspendLayout();
            this.logPage.SuspendLayout();
            this.playbackToolStrip.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openButton,
            this.saveButton,
            this.saveAsButton,
            this.connectButton,
            this.connectLocalButton,
            this.disconnectButton,
            this.statusButton,
            this.propertiesButton,
            this.resizeButton,
            this.recordMarketDataButton,
            this.sizeLabel,
            this.logSizeTextBox});
            this.mainToolStrip.Location = new System.Drawing.Point(3, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(721, 25);
            this.mainToolStrip.TabIndex = 0;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // openButton
            // 
            this.openButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sampleQuickAccessItem});
            this.openButton.Image = global::IBGrid.Properties.Resources.openHS;
            this.openButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(32, 22);
            this.openButton.Text = "Open...";
            this.openButton.ButtonClick += new System.EventHandler(this.OpenButton_ButtonClick);
            this.openButton.DropDownOpening += new System.EventHandler(this.OpenButton_DropDownOpening);
            // 
            // sampleQuickAccessItem
            // 
            this.sampleQuickAccessItem.Name = "sampleQuickAccessItem";
            this.sampleQuickAccessItem.Size = new System.Drawing.Size(139, 22);
            this.sampleQuickAccessItem.Text = "Sample Item1";
            this.sampleQuickAccessItem.Visible = false;
            this.sampleQuickAccessItem.Click += new System.EventHandler(this.QuickAccessItem_Click);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveButton.Image = global::IBGrid.Properties.Resources.saveHS;
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(23, 22);
            this.saveButton.Text = "Save";
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // saveAsButton
            // 
            this.saveAsButton.Image = global::IBGrid.Properties.Resources.saveHS;
            this.saveAsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAsButton.Name = "saveAsButton";
            this.saveAsButton.Size = new System.Drawing.Size(74, 22);
            this.saveAsButton.Text = "Save As..";
            this.saveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
            // 
            // connectButton
            // 
            this.connectButton.Image = global::IBGrid.Properties.Resources.plug;
            this.connectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(119, 22);
            this.connectButton.Text = "Connect to IB/TWS";
            this.connectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // connectLocalButton
            // 
            this.connectLocalButton.Image = global::IBGrid.Properties.Resources.plug;
            this.connectLocalButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.connectLocalButton.Name = "connectLocalButton";
            this.connectLocalButton.Size = new System.Drawing.Size(146, 22);
            this.connectLocalButton.Text = "Connect to Local IB/TWS";
            this.connectLocalButton.Click += new System.EventHandler(this.ConnectLocalButton_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.disconnectButton.Image = global::IBGrid.Properties.Resources.plug_delete;
            this.disconnectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(23, 22);
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // statusButton
            // 
            this.statusButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.statusButton.Image = global::IBGrid.Properties.Resources.bullet_square_red;
            this.statusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.statusButton.Name = "statusButton";
            this.statusButton.Size = new System.Drawing.Size(23, 22);
            this.statusButton.Text = "Disconnected";
            // 
            // propertiesButton
            // 
            this.propertiesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.propertiesButton.Image = global::IBGrid.Properties.Resources.PropertiesHS;
            this.propertiesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new System.Drawing.Size(23, 22);
            this.propertiesButton.Text = "toolStripButton1";
            this.propertiesButton.Click += new System.EventHandler(this.PropertiesButton_Click);
            // 
            // resizeButton
            // 
            this.resizeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.resizeButton.Image = global::IBGrid.Properties.Resources.Control_HScrollBar1;
            this.resizeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.resizeButton.Name = "resizeButton";
            this.resizeButton.Size = new System.Drawing.Size(23, 22);
            this.resizeButton.Text = "Automatically Resize Cells";
            this.resizeButton.Click += new System.EventHandler(this.ResizeButton_Click);
            // 
            // recordMarketDataButton
            // 
            this.recordMarketDataButton.BackColor = System.Drawing.SystemColors.Control;
            this.recordMarketDataButton.Image = global::IBGrid.Properties.Resources.RecordHS;
            this.recordMarketDataButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.recordMarketDataButton.Name = "recordMarketDataButton";
            this.recordMarketDataButton.Size = new System.Drawing.Size(123, 22);
            this.recordMarketDataButton.Text = "Record Market Data";
            this.recordMarketDataButton.Click += new System.EventHandler(this.RecordMarketDataButton_Click);
            // 
            // sizeLabel
            // 
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(50, 22);
            this.sizeLabel.Text = "Log Size:";
            // 
            // logSizeTextBox
            // 
            this.logSizeTextBox.Name = "logSizeTextBox";
            this.logSizeTextBox.ReadOnly = true;
            this.logSizeTextBox.Size = new System.Drawing.Size(50, 25);
            this.logSizeTextBox.Text = "0KB";
            // 
            // gridTab
            // 
            this.gridTab.Controls.Add(this.indicesPage);
            this.gridTab.Controls.Add(this.futuresPage);
            this.gridTab.Controls.Add(this.stocksGridPage);
            this.gridTab.Controls.Add(this.stockOptionsPage);
            this.gridTab.Controls.Add(this.indexOptionsGridPage);
            this.gridTab.Controls.Add(this.futureOptionsPage);
            this.gridTab.Controls.Add(this.logPage);
            this.gridTab.Controls.Add(this.skewPage);
            this.gridTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTab.Location = new System.Drawing.Point(0, 0);
            this.gridTab.Name = "gridTab";
            this.gridTab.SelectedIndex = 0;
            this.gridTab.Size = new System.Drawing.Size(877, 613);
            this.gridTab.TabIndex = 1;
            // 
            // indicesPage
            // 
            this.indicesPage.Controls.Add(this.indicesGrid);
            this.indicesPage.Location = new System.Drawing.Point(4, 22);
            this.indicesPage.Name = "indicesPage";
            this.indicesPage.Padding = new System.Windows.Forms.Padding(3);
            this.indicesPage.Size = new System.Drawing.Size(869, 587);
            this.indicesPage.TabIndex = 4;
            this.indicesPage.Text = "Indices";
            this.indicesPage.UseVisualStyleBackColor = true;
            // 
            // indicesGrid
            // 
            this.indicesGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.indicesGrid.ColumnsCount = 40;
            this.indicesGrid.ContextMenuStrip = this.gridContextMenu;
            this.indicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indicesGrid.FixedColumns = 9;
            this.indicesGrid.FixedRows = 1;
            this.indicesGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.indicesGrid.Location = new System.Drawing.Point(3, 3);
            this.indicesGrid.Name = "indicesGrid";
            this.indicesGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.indicesGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.indicesGrid.Size = new System.Drawing.Size(863, 581);
            this.indicesGrid.TabIndex = 2;
            this.indicesGrid.TabStop = true;
            this.indicesGrid.ToolTipText = "";
            // 
            // gridContextMenu
            // 
            this.gridContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stopMarketDataRetrievalToolStripMenuItem,
            this.startMarketDataRetrievalToolStripMenuItem,
            this.separator1,
            this.addNewItemToolStripMenuItem,
            this.clearLineToolStripMenuItem});
            this.gridContextMenu.Name = "gridContextMenu";
            this.gridContextMenu.Size = new System.Drawing.Size(207, 98);
            // 
            // stopMarketDataRetrievalToolStripMenuItem
            // 
            this.stopMarketDataRetrievalToolStripMenuItem.Image = global::IBGrid.Properties.Resources.PauseRecorderHS;
            this.stopMarketDataRetrievalToolStripMenuItem.Name = "stopMarketDataRetrievalToolStripMenuItem";
            this.stopMarketDataRetrievalToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.stopMarketDataRetrievalToolStripMenuItem.Text = "Stop Market Data Retrieval";
            // 
            // startMarketDataRetrievalToolStripMenuItem
            // 
            this.startMarketDataRetrievalToolStripMenuItem.Image = global::IBGrid.Properties.Resources.RecordHS;
            this.startMarketDataRetrievalToolStripMenuItem.Name = "startMarketDataRetrievalToolStripMenuItem";
            this.startMarketDataRetrievalToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.startMarketDataRetrievalToolStripMenuItem.Text = "Start Market Data Retrieval";
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            this.separator1.Size = new System.Drawing.Size(203, 6);
            // 
            // addNewItemToolStripMenuItem
            // 
            this.addNewItemToolStripMenuItem.Name = "addNewItemToolStripMenuItem";
            this.addNewItemToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.addNewItemToolStripMenuItem.Text = "Add New Instrument";
            this.addNewItemToolStripMenuItem.Click += new System.EventHandler(this.AddNewItemToolStripMenuItem_Click);
            // 
            // clearLineToolStripMenuItem
            // 
            this.clearLineToolStripMenuItem.Image = global::IBGrid.Properties.Resources.DeleteHS;
            this.clearLineToolStripMenuItem.Name = "clearLineToolStripMenuItem";
            this.clearLineToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.clearLineToolStripMenuItem.Text = "Clear Instrument Details";
            this.clearLineToolStripMenuItem.Click += new System.EventHandler(this.ClearLineToolStripMenuItem_Click);
            // 
            // futuresPage
            // 
            this.futuresPage.Controls.Add(this.futuresGrid);
            this.futuresPage.Location = new System.Drawing.Point(4, 22);
            this.futuresPage.Name = "futuresPage";
            this.futuresPage.Padding = new System.Windows.Forms.Padding(3);
            this.futuresPage.Size = new System.Drawing.Size(869, 587);
            this.futuresPage.TabIndex = 3;
            this.futuresPage.Text = "Futures";
            this.futuresPage.UseVisualStyleBackColor = true;
            // 
            // futuresGrid
            // 
            this.futuresGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.futuresGrid.ColumnsCount = 40;
            this.futuresGrid.ContextMenuStrip = this.gridContextMenu;
            this.futuresGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.futuresGrid.FixedColumns = 9;
            this.futuresGrid.FixedRows = 1;
            this.futuresGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.futuresGrid.Location = new System.Drawing.Point(3, 3);
            this.futuresGrid.Name = "futuresGrid";
            this.futuresGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.futuresGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.futuresGrid.Size = new System.Drawing.Size(863, 581);
            this.futuresGrid.TabIndex = 2;
            this.futuresGrid.TabStop = true;
            this.futuresGrid.ToolTipText = "";
            // 
            // stocksGridPage
            // 
            this.stocksGridPage.Controls.Add(this.stocksGrid);
            this.stocksGridPage.Location = new System.Drawing.Point(4, 22);
            this.stocksGridPage.Name = "stocksGridPage";
            this.stocksGridPage.Padding = new System.Windows.Forms.Padding(3);
            this.stocksGridPage.Size = new System.Drawing.Size(869, 587);
            this.stocksGridPage.TabIndex = 1;
            this.stocksGridPage.Text = "Stocks";
            this.stocksGridPage.UseVisualStyleBackColor = true;
            // 
            // stocksGrid
            // 
            this.stocksGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stocksGrid.ColumnsCount = 40;
            this.stocksGrid.ContextMenuStrip = this.gridContextMenu;
            this.stocksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stocksGrid.FixedColumns = 9;
            this.stocksGrid.FixedRows = 1;
            this.stocksGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stocksGrid.Location = new System.Drawing.Point(3, 3);
            this.stocksGrid.Name = "stocksGrid";
            this.stocksGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.stocksGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.stocksGrid.Size = new System.Drawing.Size(863, 581);
            this.stocksGrid.TabIndex = 1;
            this.stocksGrid.TabStop = true;
            this.stocksGrid.ToolTipText = "";
            // 
            // stockOptionsPage
            // 
            this.stockOptionsPage.Controls.Add(this.stockOptionsGrid);
            this.stockOptionsPage.Location = new System.Drawing.Point(4, 22);
            this.stockOptionsPage.Name = "stockOptionsPage";
            this.stockOptionsPage.Padding = new System.Windows.Forms.Padding(3);
            this.stockOptionsPage.Size = new System.Drawing.Size(869, 587);
            this.stockOptionsPage.TabIndex = 2;
            this.stockOptionsPage.Text = "Stock Options";
            this.stockOptionsPage.UseVisualStyleBackColor = true;
            // 
            // stockOptionsGrid
            // 
            this.stockOptionsGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stockOptionsGrid.ColumnsCount = 40;
            this.stockOptionsGrid.ContextMenuStrip = this.gridContextMenu;
            this.stockOptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stockOptionsGrid.FixedColumns = 9;
            this.stockOptionsGrid.FixedRows = 1;
            this.stockOptionsGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stockOptionsGrid.Location = new System.Drawing.Point(3, 3);
            this.stockOptionsGrid.Name = "stockOptionsGrid";
            this.stockOptionsGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.stockOptionsGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.stockOptionsGrid.Size = new System.Drawing.Size(863, 581);
            this.stockOptionsGrid.TabIndex = 2;
            this.stockOptionsGrid.TabStop = true;
            this.stockOptionsGrid.ToolTipText = "";
            // 
            // indexOptionsGridPage
            // 
            this.indexOptionsGridPage.Controls.Add(this.indexOptionsGrid);
            this.indexOptionsGridPage.Location = new System.Drawing.Point(4, 22);
            this.indexOptionsGridPage.Name = "indexOptionsGridPage";
            this.indexOptionsGridPage.Padding = new System.Windows.Forms.Padding(3);
            this.indexOptionsGridPage.Size = new System.Drawing.Size(869, 587);
            this.indexOptionsGridPage.TabIndex = 0;
            this.indexOptionsGridPage.Text = "Index Options";
            this.indexOptionsGridPage.UseVisualStyleBackColor = true;
            // 
            // indexOptionsGrid
            // 
            this.indexOptionsGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.indexOptionsGrid.ColumnsCount = 40;
            this.indexOptionsGrid.ContextMenuStrip = this.gridContextMenu;
            this.indexOptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indexOptionsGrid.FixedColumns = 9;
            this.indexOptionsGrid.FixedRows = 1;
            this.indexOptionsGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.indexOptionsGrid.Location = new System.Drawing.Point(3, 3);
            this.indexOptionsGrid.Name = "indexOptionsGrid";
            this.indexOptionsGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.indexOptionsGrid.SelectionMode = SourceGrid.GridSelectionMode.Row;
            this.indexOptionsGrid.Size = new System.Drawing.Size(863, 581);
            this.indexOptionsGrid.TabIndex = 0;
            this.indexOptionsGrid.TabStop = true;
            this.indexOptionsGrid.ToolTipText = "";
            // 
            // futureOptionsPage
            // 
            this.futureOptionsPage.Controls.Add(this.futureOptionsGrid);
            this.futureOptionsPage.Location = new System.Drawing.Point(4, 22);
            this.futureOptionsPage.Name = "futureOptionsPage";
            this.futureOptionsPage.Padding = new System.Windows.Forms.Padding(3);
            this.futureOptionsPage.Size = new System.Drawing.Size(869, 587);
            this.futureOptionsPage.TabIndex = 5;
            this.futureOptionsPage.Text = "Future Options";
            this.futureOptionsPage.UseVisualStyleBackColor = true;
            // 
            // futureOptionsGrid
            // 
            this.futureOptionsGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.futureOptionsGrid.ColumnsCount = 40;
            this.futureOptionsGrid.ContextMenuStrip = this.gridContextMenu;
            this.futureOptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.futureOptionsGrid.FixedColumns = 9;
            this.futureOptionsGrid.FixedRows = 1;
            this.futureOptionsGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.futureOptionsGrid.Location = new System.Drawing.Point(3, 3);
            this.futureOptionsGrid.Name = "futureOptionsGrid";
            this.futureOptionsGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.futureOptionsGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.futureOptionsGrid.Size = new System.Drawing.Size(863, 581);
            this.futureOptionsGrid.TabIndex = 2;
            this.futureOptionsGrid.TabStop = true;
            this.futureOptionsGrid.ToolTipText = "";
            // 
            // logPage
            // 
            this.logPage.Controls.Add(this.logGrid);
            this.logPage.Location = new System.Drawing.Point(4, 22);
            this.logPage.Name = "logPage";
            this.logPage.Padding = new System.Windows.Forms.Padding(3);
            this.logPage.Size = new System.Drawing.Size(869, 587);
            this.logPage.TabIndex = 6;
            this.logPage.Text = "Log";
            this.logPage.UseVisualStyleBackColor = true;
            // 
            // logGrid
            // 
            this.logGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logGrid.ColumnsCount = 40;
            this.logGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logGrid.FixedRows = 1;
            this.logGrid.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logGrid.Location = new System.Drawing.Point(3, 3);
            this.logGrid.Name = "logGrid";
            this.logGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.logGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.logGrid.Size = new System.Drawing.Size(863, 581);
            this.logGrid.TabIndex = 4;
            this.logGrid.TabStop = true;
            this.logGrid.ToolTipText = "";
            this.logGrid.Paint += new System.Windows.Forms.PaintEventHandler(this.logGrid_Paint);
            // 
            // skewPage
            // 
            this.skewPage.Location = new System.Drawing.Point(4, 22);
            this.skewPage.Name = "skewPage";
            this.skewPage.Padding = new System.Windows.Forms.Padding(3);
            this.skewPage.Size = new System.Drawing.Size(869, 587);
            this.skewPage.TabIndex = 7;
            this.skewPage.Text = "Skew";
            this.skewPage.UseVisualStyleBackColor = true;
            // 
            // playbackToolStrip
            // 
            this.playbackToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.playbackToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLogFileButton,
            this.playbackRewindButton,
            this.playbackPauseButton,
            this.playbackStepButton,
            this.playbackPlayButton,
            this.playbackFastForwardButton,
            this.playbackStopButton,
            this.toolStripProgressBar1});
            this.playbackToolStrip.Location = new System.Drawing.Point(3, 25);
            this.playbackToolStrip.Name = "playbackToolStrip";
            this.playbackToolStrip.Size = new System.Drawing.Size(273, 25);
            this.playbackToolStrip.TabIndex = 3;
            this.playbackToolStrip.Text = "toolStrip1";
            // 
            // openLogFileButton
            // 
            this.openLogFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openLogFileButton.Image = global::IBGrid.Properties.Resources.openHS;
            this.openLogFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openLogFileButton.Name = "openLogFileButton";
            this.openLogFileButton.Size = new System.Drawing.Size(23, 22);
            this.openLogFileButton.Text = "Open Log File...";
            this.openLogFileButton.Click += new System.EventHandler(this.OpenLogFileButton_Click);
            // 
            // playbackRewindButton
            // 
            this.playbackRewindButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playbackRewindButton.Image = global::IBGrid.Properties.Resources.media_beginning;
            this.playbackRewindButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playbackRewindButton.Name = "playbackRewindButton";
            this.playbackRewindButton.Size = new System.Drawing.Size(23, 22);
            this.playbackRewindButton.Text = "toolStripButton1";
            this.playbackRewindButton.Click += new System.EventHandler(this.PlaybackRewindButton_Click);
            // 
            // playbackPauseButton
            // 
            this.playbackPauseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playbackPauseButton.Image = global::IBGrid.Properties.Resources.media_pause;
            this.playbackPauseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playbackPauseButton.Name = "playbackPauseButton";
            this.playbackPauseButton.Size = new System.Drawing.Size(23, 22);
            this.playbackPauseButton.Text = "toolStripButton2";
            this.playbackPauseButton.Click += new System.EventHandler(this.PlaybackPauseButton_Click);
            // 
            // playbackStepButton
            // 
            this.playbackStepButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playbackStepButton.Image = global::IBGrid.Properties.Resources.media_step_forward;
            this.playbackStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playbackStepButton.Name = "playbackStepButton";
            this.playbackStepButton.Size = new System.Drawing.Size(23, 22);
            this.playbackStepButton.Text = "toolStripButton3";
            this.playbackStepButton.Click += new System.EventHandler(this.PlaybackStepButton_Click);
            // 
            // playbackPlayButton
            // 
            this.playbackPlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playbackPlayButton.Image = global::IBGrid.Properties.Resources.media_play_green;
            this.playbackPlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playbackPlayButton.Name = "playbackPlayButton";
            this.playbackPlayButton.Size = new System.Drawing.Size(23, 22);
            this.playbackPlayButton.Text = "toolStripButton4";
            this.playbackPlayButton.Click += new System.EventHandler(this.PlaybackPlayButton_Click);
            // 
            // playbackFastForwardButton
            // 
            this.playbackFastForwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playbackFastForwardButton.Image = global::IBGrid.Properties.Resources.media_fast_forward;
            this.playbackFastForwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playbackFastForwardButton.Name = "playbackFastForwardButton";
            this.playbackFastForwardButton.Size = new System.Drawing.Size(23, 22);
            this.playbackFastForwardButton.Text = "toolStripButton5";
            this.playbackFastForwardButton.Click += new System.EventHandler(this.PlaybackFastForwardButton_Click);
            // 
            // playbackStopButton
            // 
            this.playbackStopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playbackStopButton.Image = global::IBGrid.Properties.Resources.media_stop;
            this.playbackStopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playbackStopButton.Name = "playbackStopButton";
            this.playbackStopButton.Size = new System.Drawing.Size(23, 22);
            this.playbackStopButton.Text = "toolStripButton6";
            this.playbackStopButton.Click += new System.EventHandler(this.PlaybackStopButton_Click);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Maximum = 1000;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 22);
            this.toolStripProgressBar1.Step = 1;
            this.toolStripProgressBar1.Value = 121;
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.Controls.Add(this.gridTab);
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(877, 613);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.Size = new System.Drawing.Size(877, 663);
            this.toolStripContainer.TabIndex = 4;
            this.toolStripContainer.Text = "toolStripContainer1";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.mainToolStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.playbackToolStrip);
            // 
            // recordButtonBlinkTimer
            // 
            this.recordButtonBlinkTimer.Interval = 1000;
            this.recordButtonBlinkTimer.Tick += new System.EventHandler(this.RecordButtonBlinkTimer_Tick);
            // 
            // logSizeTextBoxTimer
            // 
            this.logSizeTextBoxTimer.Interval = 5000;
            this.logSizeTextBoxTimer.Tick += new System.EventHandler(this.LogSizeTextBoxTimer_Tick);
            // 
            // IBGridForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(877, 663);
            this.Controls.Add(this.toolStripContainer);
            this.Name = "IBGridForm";
            this.Text = "Interactive Brokers Grid";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_FormClosed);
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.gridTab.ResumeLayout(false);
            this.indicesPage.ResumeLayout(false);
            this.gridContextMenu.ResumeLayout(false);
            this.futuresPage.ResumeLayout(false);
            this.stocksGridPage.ResumeLayout(false);
            this.stockOptionsPage.ResumeLayout(false);
            this.indexOptionsGridPage.ResumeLayout(false);
            this.futureOptionsPage.ResumeLayout(false);
            this.logPage.ResumeLayout(false);
            this.playbackToolStrip.ResumeLayout(false);
            this.playbackToolStrip.PerformLayout();
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton connectButton;
        private System.Windows.Forms.ToolStripButton disconnectButton;
        private System.Windows.Forms.TabControl gridTab;
        private System.Windows.Forms.TabPage indexOptionsGridPage;
        private System.Windows.Forms.TabPage stocksGridPage;
        private SourceGrid.Grid indexOptionsGrid;
        private System.Windows.Forms.ToolStripButton statusButton;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripButton resizeButton;
        private SourceGrid.Grid stocksGrid;
        private System.Windows.Forms.ToolStripButton connectLocalButton;
        private System.Windows.Forms.ContextMenuStrip gridContextMenu;
        private System.Windows.Forms.ToolStripMenuItem stopMarketDataRetrievalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startMarketDataRetrievalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearLineToolStripMenuItem;
        private System.Windows.Forms.ToolStrip playbackToolStrip;
        private System.Windows.Forms.ToolStripButton playbackRewindButton;
        private System.Windows.Forms.ToolStripButton playbackPauseButton;
        private System.Windows.Forms.ToolStripButton playbackStepButton;
        private System.Windows.Forms.ToolStripButton playbackPlayButton;
        private System.Windows.Forms.ToolStripButton playbackFastForwardButton;
        private System.Windows.Forms.ToolStripButton playbackStopButton;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripButton recordMarketDataButton;
        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.ToolStripTextBox logSizeTextBox;
        private System.Windows.Forms.ToolStripLabel sizeLabel;
        private System.Windows.Forms.ToolStripButton saveAsButton;
        private System.Windows.Forms.ToolStripSplitButton openButton;
        private System.Windows.Forms.ToolStripMenuItem sampleQuickAccessItem;
        private System.Windows.Forms.TabPage stockOptionsPage;
        private SourceGrid.Grid stockOptionsGrid;
        private System.Windows.Forms.TabPage futuresPage;
        private SourceGrid.Grid futuresGrid;
        private System.Windows.Forms.TabPage indicesPage;
        private SourceGrid.Grid indicesGrid;
        private System.Windows.Forms.TabPage futureOptionsPage;
        private SourceGrid.Grid futureOptionsGrid;
        private System.Windows.Forms.TabPage logPage;
        private System.Windows.Forms.ToolStripSeparator separator1;
        private System.Windows.Forms.ToolStripMenuItem addNewItemToolStripMenuItem;
        private System.Windows.Forms.Timer recordButtonBlinkTimer;
        private System.Windows.Forms.Timer logSizeTextBoxTimer;
        private SourceGrid.Grid logGrid;
        private System.Windows.Forms.ToolStripButton propertiesButton;
        private System.Windows.Forms.TabPage skewPage;
        private System.Windows.Forms.ToolStripButton openLogFileButton;

    }
}

