namespace AutoCadCommandTracker.Views
{
    partial class MainFormEnhanced
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
            this._dataGrid = new System.Windows.Forms.DataGridView();
            this._contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._setShortcutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._clearShortcutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._searchBox = new System.Windows.Forms.TextBox();
            this._statusLabel = new System.Windows.Forms.Label();
            this._titleLabel = new System.Windows.Forms.Label();
            this._settingsButton = new System.Windows.Forms.Button();
            this._statsButton = new System.Windows.Forms.Button();
            this._compactViewButton = new System.Windows.Forms.Button();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.statusPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this._dataGrid)).BeginInit();
            this._contextMenu.SuspendLayout();
            this.titlePanel.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.statusPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _dataGrid
            //
            this._dataGrid.AllowUserToAddRows = false;
            this._dataGrid.AllowUserToDeleteRows = false;
            this._dataGrid.AutoGenerateColumns = false;
            this._dataGrid.BackgroundColor = System.Drawing.Color.White;
            this._dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGrid.ContextMenuStrip = this._contextMenu;
            this._dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._dataGrid.Location = new System.Drawing.Point(0, 60);
            this._dataGrid.MultiSelect = false;
            this._dataGrid.Name = "_dataGrid";
            this._dataGrid.ReadOnly = true;
            this._dataGrid.RowHeadersVisible = false;
            this._dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dataGrid.Size = new System.Drawing.Size(300, 315);
            this._dataGrid.TabIndex = 2;
            this._dataGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGrid_CellMouseDown);
            this._dataGrid.DoubleClick += new System.EventHandler(this.DataGrid_DoubleClick);
            this._dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGrid_KeyDown);
            //
            // _contextMenu
            //
            this._contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._setShortcutMenuItem,
            this._clearShortcutMenuItem});
            this._contextMenu.Name = "_contextMenu";
            this._contextMenu.Size = new System.Drawing.Size(200, 48);
            this._contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // _setShortcutMenuItem
            //
            this._setShortcutMenuItem.Name = "_setShortcutMenuItem";
            this._setShortcutMenuItem.Size = new System.Drawing.Size(199, 22);
            this._setShortcutMenuItem.Text = "Set Custom Shortcut...";
            this._setShortcutMenuItem.Click += new System.EventHandler(this.SetShortcutMenuItem_Click);
            //
            // _clearShortcutMenuItem
            //
            this._clearShortcutMenuItem.Name = "_clearShortcutMenuItem";
            this._clearShortcutMenuItem.Size = new System.Drawing.Size(199, 22);
            this._clearShortcutMenuItem.Text = "Clear Custom Shortcut";
            this._clearShortcutMenuItem.Click += new System.EventHandler(this.ClearShortcutMenuItem_Click);
            // 
            // _searchBox
            // 
            this._searchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._searchBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._searchBox.Location = new System.Drawing.Point(5, 5);
            this._searchBox.Name = "_searchBox";
            this._searchBox.Size = new System.Drawing.Size(290, 23);
            this._searchBox.TabIndex = 0;
            this._searchBox.Text = "Search...";
            this._searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            this._searchBox.GotFocus += new System.EventHandler(this.SearchBox_GotFocus);
            this._searchBox.LostFocus += new System.EventHandler(this.SearchBox_LostFocus);
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._statusLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._statusLabel.Location = new System.Drawing.Point(0, 0);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this._statusLabel.Size = new System.Drawing.Size(200, 25);
            this._statusLabel.TabIndex = 0;
            this._statusLabel.Text = "Ready";
            this._statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _titleLabel
            // 
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(100, 30);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "Commands";
            this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._titleLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            // 
            // _settingsButton
            // 
            this._settingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._settingsButton.BackColor = System.Drawing.Color.Transparent;
            this._settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._settingsButton.Location = new System.Drawing.Point(345, 2);
            this._settingsButton.Name = "_settingsButton";
            this._settingsButton.Size = new System.Drawing.Size(25, 25);
            this._settingsButton.TabIndex = 2;
            this._settingsButton.Text = "⚙";
            this._settingsButton.UseVisualStyleBackColor = false;
            this._settingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // _statsButton
            //
            this._statsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._statsButton.BackColor = System.Drawing.Color.Transparent;
            this._statsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._statsButton.Location = new System.Drawing.Point(315, 2);
            this._statsButton.Name = "_statsButton";
            this._statsButton.Size = new System.Drawing.Size(25, 25);
            this._statsButton.TabIndex = 1;
            this._statsButton.Text = "📊";
            this._statsButton.UseVisualStyleBackColor = false;
            this._statsButton.Click += new System.EventHandler(this.StatsButton_Click);
            //
            // _compactViewButton
            //
            this._compactViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._compactViewButton.BackColor = System.Drawing.Color.Transparent;
            this._compactViewButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._compactViewButton.Location = new System.Drawing.Point(285, 2);
            this._compactViewButton.Name = "_compactViewButton";
            this._compactViewButton.Size = new System.Drawing.Size(25, 25);
            this._compactViewButton.TabIndex = 0;
            this._compactViewButton.Text = "⊞";
            this._compactViewButton.UseVisualStyleBackColor = false;
            this._compactViewButton.Click += new System.EventHandler(this.CompactViewButton_Click);
            //
            // titlePanel
            // 
            this.titlePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.titlePanel.Controls.Add(this._titleLabel);
            this.titlePanel.Controls.Add(this._settingsButton);
            this.titlePanel.Controls.Add(this._statsButton);
            this.titlePanel.Controls.Add(this._compactViewButton);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(0, 0);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(300, 30);
            this.titlePanel.TabIndex = 0;
            // 
            // searchPanel
            // 
            this.searchPanel.Controls.Add(this._searchBox);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Location = new System.Drawing.Point(0, 30);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Padding = new System.Windows.Forms.Padding(5);
            this.searchPanel.Size = new System.Drawing.Size(300, 30);
            this.searchPanel.TabIndex = 1;
            // 
            // statusPanel
            //
            this.statusPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.statusPanel.Controls.Add(this._statusLabel);
            this.statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusPanel.Location = new System.Drawing.Point(0, 375);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Size = new System.Drawing.Size(300, 25);
            this.statusPanel.TabIndex = 3;
            //
            // MainFormEnhanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(300, 400);
            this.Controls.Add(this._dataGrid);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.titlePanel);
            this.Controls.Add(this.statusPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Name = "MainFormEnhanced";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AutoCAD Command Tracker";
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this._dataGrid)).EndInit();
            this._contextMenu.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.statusPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // Declare controls as private fields
        private System.Windows.Forms.DataGridView _dataGrid;
        private System.Windows.Forms.ContextMenuStrip _contextMenu;
        private System.Windows.Forms.ToolStripMenuItem _setShortcutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _clearShortcutMenuItem;
        private System.Windows.Forms.TextBox _searchBox;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.Button _settingsButton;
        private System.Windows.Forms.Button _statsButton;
        private System.Windows.Forms.Button _compactViewButton;
        private System.Windows.Forms.Panel titlePanel;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.Panel statusPanel;
    }
}