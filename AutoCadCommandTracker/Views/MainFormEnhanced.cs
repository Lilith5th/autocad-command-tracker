using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AutoCadCommandTracker.Models;
using AutoCadCommandTracker.Services;

namespace AutoCadCommandTracker.Views
{
    public partial class MainFormEnhanced : Form
    {
        private DataPersistenceService _persistenceService;
        private List<CommandData> _commandDataList;
        private Timer _updateTimer;
        private bool _isAtMainPrompt = true;
        private string _currentCommand;
        private bool _isCompactView = false;

        // The UI Controls are now declared in MainFormEnhanced.Designer.cs

        public MainFormEnhanced()
        {
            InitializeComponent(); // From Designer file
            InitializeServices();
            SetupForm();
            LoadSettings();
        }

        // InitializeComponent is now in MainFormEnhanced.Designer.cs

        private void InitializeServices()
        {
            _persistenceService = new DataPersistenceService();
            _commandDataList = new List<CommandData>(); // Initialize empty - data will come from MyCommandsWinForms

            // Setup update timer
            _updateTimer = new Timer();
            _updateTimer.Interval = 100; // 100ms updates
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        private void SetupForm()
        {
            // Make window draggable
            this.MouseDown += MainForm_MouseDown;
            _titleLabel.MouseDown += MainForm_MouseDown;

            // Setup keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // Configure DataGrid columns
            SetupDataGridColumns();
        }

        private void SetupDataGridColumns()
        {
            // Clear any existing columns
            _dataGrid.Columns.Clear();

            // Column 0: Index
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Index",
                HeaderText = "#",
                Width = 30,
                ReadOnly = true
            });

            // Column 1: Command/Value
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Command",
                HeaderText = "Command",
                Width = 120,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Column 2: Alias
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Alias",
                HeaderText = "Alias",
                Width = 60,
                ReadOnly = true
            });

            // Column 3: Shortcut
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Shortcut",
                HeaderText = "Shortcut",
                Width = 70,
                ReadOnly = true
            });

            // Column 4: Count
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Count",
                HeaderText = "Count",
                Width = 50,
                ReadOnly = true
            });

            // Column 5: Type
            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "Type",
                Width = 60,
                ReadOnly = true
            });
        }

        private void LoadSettings()
        {
            var settings = _persistenceService.LoadSettings();

            this.Location = new Point((int)settings.WindowLeft, (int)settings.WindowTop);
            this.Size = new Size((int)settings.WindowWidth, (int)settings.WindowHeight);
            this.Opacity = settings.WindowOpacity;

            ApplyTheme(settings.IsDarkTheme);

            // Apply compact view state
            _isCompactView = settings.IsCompactView;
            _compactViewButton.Text = _isCompactView ? "⊟" : "⊞";
            searchPanel.Visible = !_isCompactView;

            // Show/hide columns
            if (_dataGrid.Columns.Count >= 6)
            {
                _dataGrid.Columns[0].Visible = !_isCompactView; // Index
                _dataGrid.Columns[2].Visible = !_isCompactView; // Alias
                _dataGrid.Columns[4].Visible = !_isCompactView; // Count
                _dataGrid.Columns[5].Visible = !_isCompactView; // Type
            }
        }

        private void ApplyTheme(bool isDarkTheme)
        {
            if (isDarkTheme)
            {
                this.BackColor = Color.FromArgb(45, 45, 45);
                _dataGrid.BackgroundColor = Color.FromArgb(45, 45, 45);
                _dataGrid.ForeColor = Color.White;
                _dataGrid.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
                _dataGrid.DefaultCellStyle.ForeColor = Color.White;
                _titleLabel.ForeColor = Color.White;
                _statusLabel.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = Color.FromArgb(240, 240, 240);
                _dataGrid.BackgroundColor = Color.White;
                _dataGrid.ForeColor = Color.Black;
                _dataGrid.DefaultCellStyle.BackColor = Color.White;
                _dataGrid.DefaultCellStyle.ForeColor = Color.Black;
                _titleLabel.ForeColor = Color.Black;
                _statusLabel.ForeColor = Color.Black;
            }
        }

        public void UpdateDisplayData(List<CommandData> commands, bool isAtMainPrompt, string currentCommand = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateDisplayData(commands, isAtMainPrompt, currentCommand)));
                return;
            }

            // Debug output
            System.Diagnostics.Debug.WriteLine($"MainFormEnhanced.UpdateDisplayData called with {commands?.Count ?? 0} commands");
            
            _isAtMainPrompt = isAtMainPrompt;
            _currentCommand = currentCommand;
            _commandDataList = commands;

            RefreshGrid();
            
            _titleLabel.Text = isAtMainPrompt ? "Commands" : $"Values - {currentCommand}";
        }

        private void RefreshGrid()
        {
            System.Diagnostics.Debug.WriteLine($"RefreshGrid called. _commandDataList has {_commandDataList?.Count ?? 0} commands");
            
            _dataGrid.Rows.Clear();

            var searchText = (_searchBox.Text == "Search..." || string.IsNullOrEmpty(_searchBox.Text)) ? "" : _searchBox.Text.ToLower();
            var filteredData = GetFilteredData(searchText);
            
            System.Diagnostics.Debug.WriteLine($"Filtered data has {filteredData.Count} items");

            int index = 1;
            foreach (var item in filteredData)
            {
                System.Diagnostics.Debug.WriteLine($"Adding row for: {item.DisplayText}");
                
                var row = new DataGridViewRow();
                row.CreateCells(_dataGrid);
                
                row.Cells[0].Value = index;
                row.Cells[1].Value = item.DisplayText;
                row.Cells[2].Value = item.Alias;
                row.Cells[3].Value = GetKeyboardShortcut(index);
                row.Cells[4].Value = item.UsageCount;
                row.Cells[5].Value = item.Type;
                
                index++;
                
                row.Tag = item.RawData;
                
                _dataGrid.Rows.Add(row);
            }

            _statusLabel.Text = $"{filteredData.Count} items";
            System.Diagnostics.Debug.WriteLine($"RefreshGrid completed. Added {filteredData.Count} rows to grid");
        }

        private List<DisplayItem> GetFilteredData(string searchText)
        {
            var items = new List<DisplayItem>();

            if (_isAtMainPrompt)
            {
                foreach (var cmd in _commandDataList.OrderByDescending(c => c.UsageCount))
                {
                    if (string.IsNullOrEmpty(searchText) || cmd.CommandName.ToLower().Contains(searchText))
                    {
                        items.Add(new DisplayItem
                        {
                            DisplayText = cmd.CommandName,
                            Alias = CommandAliasService.GetAlias(cmd.CommandName),
                            UsageCount = cmd.UsageCount,
                            Type = "Command",
                            RawData = cmd
                        });
                    }
                }
            }
            else
            {
                var currentCmd = _commandDataList.FirstOrDefault(c => c.CommandName == _currentCommand);
                if (currentCmd != null)
                {
                    foreach (var val in currentCmd.InputValues.OrderByDescending(v => v.UsageCount))
                    {
                        if (string.IsNullOrEmpty(searchText) || val.DisplayValue.ToLower().Contains(searchText))
                        {
                            items.Add(new DisplayItem
                            {
                                DisplayText = val.DisplayValue,
                                Alias = "", // Values don't have aliases
                                UsageCount = val.UsageCount,
                                Type = val.Type.ToString(),
                                RawData = val
                            });
                        }
                    }
                }
            }

            return items;
        }

        public void StartTracking()
        {
            _updateTimer.Start();
        }

        public void StopTracking()
        {
            _updateTimer.Stop();
        }

        #region Event Handlers

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Periodic updates if needed
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void DataGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Select the row when right-clicking so context menu works properly
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                _dataGrid.ClearSelection();
                _dataGrid.Rows[e.RowIndex].Selected = true;
            }
        }

        private void DataGrid_DoubleClick(object sender, EventArgs e)
        {
            ExecuteSelectedItem();
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelectedItem();
                e.Handled = true;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var settings = _persistenceService?.LoadSettings();

                // Build the pressed key combination
                string pressedCombo = "";
                if (e.Control) pressedCombo += "Ctrl+";
                if (e.Alt) pressedCombo += "Alt+";
                if (e.Shift) pressedCombo += "Shift+";
                pressedCombo += e.KeyCode.ToString();

                // First, check custom command shortcuts
                if (settings?.CustomCommandShortcuts != null)
                {
                    foreach (var kvp in settings.CustomCommandShortcuts)
                    {
                        if (kvp.Value == pressedCombo)
                        {
                            // Find the command in the grid
                            for (int i = 0; i < _dataGrid.Rows.Count; i++)
                            {
                                var cmdText = _dataGrid.Rows[i].Cells[1].Value?.ToString();
                                if (cmdText == kvp.Key)
                                {
                                    ExecuteItemAtIndex(i);
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }
                    }
                }

                // Fall back to grid position shortcuts (Ctrl+Q, Ctrl+W, etc.)
                if (e.Control && !e.Alt && !e.Shift)
                {
                    var shortcutKeys = settings?.CustomShortcutKeys ?? UserSettings.GetDefaultShortcutKeys();

                    // Find the index based on current shortcut configuration
                    char pressedKey = char.ToUpper((char)e.KeyCode);
                    int index = shortcutKeys.FindIndex(k => k == pressedKey);

                    if (index >= 0 && index < _dataGrid.Rows.Count)
                    {
                        ExecuteItemAtIndex(index);
                        e.Handled = true;
                    }
                }
            }
            catch
            {
                // If anything fails, ignore the keypress
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Make form draggable
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            ShowSettingsDialog();
        }

        private void StatsButton_Click(object sender, EventArgs e)
        {
            ShowStatisticsDialog();
        }

        private void CompactViewButton_Click(object sender, EventArgs e)
        {
            ToggleCompactView();
        }

        private void SearchBox_GotFocus(object sender, EventArgs e)
        {
            if (_searchBox.Text == "Search...")
            {
                _searchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_searchBox.Text))
            {
                _searchBox.Text = "Search...";
            }
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Only show context menu if we're at main prompt (not showing values)
            if (!_isAtMainPrompt)
            {
                e.Cancel = true;
                return;
            }

            // Enable/disable Clear option based on whether the selected command has a custom shortcut
            if (_dataGrid.SelectedRows.Count > 0)
            {
                var commandText = _dataGrid.SelectedRows[0].Cells[1].Value?.ToString();
                if (!string.IsNullOrEmpty(commandText))
                {
                    var settings = _persistenceService.LoadSettings();
                    _clearShortcutMenuItem.Enabled = settings.CustomCommandShortcuts.ContainsKey(commandText);
                }
                else
                {
                    _clearShortcutMenuItem.Enabled = false;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void SetShortcutMenuItem_Click(object sender, EventArgs e)
        {
            if (_dataGrid.SelectedRows.Count == 0)
                return;

            var commandText = _dataGrid.SelectedRows[0].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(commandText))
                return;

            // Show dialog to set custom shortcut
            using (var dialog = new ShortcutInputDialog(commandText))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var settings = _persistenceService.LoadSettings();
                    settings.CustomCommandShortcuts[commandText] = dialog.ShortcutKey;
                    _persistenceService.SaveSettings(settings);

                    // Refresh the grid to show the new shortcut
                    RefreshGrid();

                    MessageBox.Show($"Custom shortcut '{dialog.ShortcutKey}' set for {commandText}",
                        "Shortcut Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ClearShortcutMenuItem_Click(object sender, EventArgs e)
        {
            if (_dataGrid.SelectedRows.Count == 0)
                return;

            var commandText = _dataGrid.SelectedRows[0].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(commandText))
                return;

            var settings = _persistenceService.LoadSettings();
            if (settings.CustomCommandShortcuts.ContainsKey(commandText))
            {
                settings.CustomCommandShortcuts.Remove(commandText);
                _persistenceService.SaveSettings(settings);

                // Refresh the grid to update the shortcut display
                RefreshGrid();

                MessageBox.Show($"Custom shortcut cleared for {commandText}",
                    "Shortcut Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Helper Methods

        private void ExecuteSelectedItem()
        {
            if (_dataGrid.SelectedRows.Count > 0)
            {
                var row = _dataGrid.SelectedRows[0];
                var commandText = row.Cells[1].Value?.ToString();
                var aliasText = row.Cells[2].Value?.ToString();
                
                if (!string.IsNullOrEmpty(commandText))
                {
                    // Use alias if available and not empty, otherwise use the command/value text
                    var textToSend = !string.IsNullOrEmpty(aliasText) ? aliasText : commandText;
                    SendToAutoCAD(textToSend);
                }
            }
        }

        private void ExecuteItemAtIndex(int index)
        {
            if (index >= 0 && index < _dataGrid.Rows.Count)
            {
                var commandText = _dataGrid.Rows[index].Cells[1].Value?.ToString();
                var aliasText = _dataGrid.Rows[index].Cells[2].Value?.ToString();
                
                if (!string.IsNullOrEmpty(commandText))
                {
                    // Use alias if available and not empty, otherwise use the command/value text
                    var textToSend = !string.IsNullOrEmpty(aliasText) ? aliasText : commandText;
                    SendToAutoCAD(textToSend);
                }
            }
        }

        private void SendToAutoCAD(string command)
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.SendStringToExecute(command + " ", true, false, false);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error sending command: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSettingsDialog()
        {
            using (var settingsForm = new SettingsForm(_persistenceService))
            {
                // Subscribe to shortcut changes
                settingsForm.ShortcutKeysChanged += (newKeys) =>
                {
                    // Update the tracker's shortcuts if available
                    WinFormsCommandTracker.CurrentInstance?.UpdateShortcutKeys(newKeys);
                };
                
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSettings();
                }
            }
        }

        private void ShowStatisticsDialog()
        {
            using (var statsForm = new StatisticsForm(_commandDataList, _persistenceService))
            {
                statsForm.ShowDialog();
            }
        }

        private void ToggleCompactView()
        {
            _isCompactView = !_isCompactView;

            // Update button text
            _compactViewButton.Text = _isCompactView ? "⊟" : "⊞";

            // Show/hide search panel
            searchPanel.Visible = !_isCompactView;

            // Debug: Check column count
            System.Diagnostics.Debug.WriteLine($"ToggleCompactView: _isCompactView={_isCompactView}, Column count={_dataGrid.Columns.Count}");

            // Suspend layout for better performance
            _dataGrid.SuspendLayout();

            try
            {
                // Show/hide columns (0=Index, 1=Command, 2=Alias, 3=Shortcut, 4=Count, 5=Type)
                if (_dataGrid.Columns.Count >= 6)
                {
                    if (_isCompactView)
                    {
                        // Compact view - hide columns by setting width to 0
                        _dataGrid.Columns[0].Visible = false; // Index
                        _dataGrid.Columns[2].Visible = false; // Alias
                        _dataGrid.Columns[4].Visible = false; // Count
                        _dataGrid.Columns[5].Visible = false; // Type
                    }
                    else
                    {
                        // Expanded view - restore columns
                        _dataGrid.Columns[0].Visible = true; // Index
                        _dataGrid.Columns[2].Visible = true; // Alias
                        _dataGrid.Columns[4].Visible = true; // Count
                        _dataGrid.Columns[5].Visible = true; // Type
                    }

                    // Debug: Log visibility
                    for (int i = 0; i < _dataGrid.Columns.Count; i++)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Column {i} ({_dataGrid.Columns[i].HeaderText}): Visible={_dataGrid.Columns[i].Visible}, Width={_dataGrid.Columns[i].Width}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  WARNING: Not enough columns! Count={_dataGrid.Columns.Count}");
                }
            }
            finally
            {
                _dataGrid.ResumeLayout();
            }

            // Force complete refresh
            _dataGrid.Invalidate();
            this.Refresh();

            // Save the setting
            var settings = _persistenceService.LoadSettings();
            settings.IsCompactView = _isCompactView;
            _persistenceService.SaveSettings(settings);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Save settings
            var settings = _persistenceService.LoadSettings();
            settings.WindowLeft = this.Location.X;
            settings.WindowTop = this.Location.Y;
            settings.WindowWidth = this.Size.Width;
            settings.WindowHeight = this.Size.Height;
            _persistenceService.SaveSettings(settings);

            // Save command data
            _persistenceService.SaveCommandData(_commandDataList);

            base.OnFormClosing(e);
        }

        private string GetKeyboardShortcut(int index)
        {
            try
            {
                // Check if there's a custom command shortcut for the current row
                if (index >= 1 && index <= _dataGrid.Rows.Count)
                {
                    var commandText = _dataGrid.Rows[index - 1].Cells[1].Value?.ToString();
                    if (!string.IsNullOrEmpty(commandText))
                    {
                        var settings = _persistenceService?.LoadSettings();
                        if (settings?.CustomCommandShortcuts != null &&
                            settings.CustomCommandShortcuts.ContainsKey(commandText))
                        {
                            return settings.CustomCommandShortcuts[commandText];
                        }
                    }
                }

                // Fall back to grid position shortcuts (Ctrl+Q, Ctrl+W, etc.)
                var settings2 = _persistenceService?.LoadSettings();
                if (settings2?.CustomShortcutKeys != null &&
                    index >= 1 && index <= settings2.CustomShortcutKeys.Count)
                {
                    return $"Ctrl+{settings2.CustomShortcutKeys[index - 1]}";
                }

                // Fallback to default shortcuts
                var defaultKeys = UserSettings.GetDefaultShortcutKeys();
                if (index >= 1 && index <= defaultKeys.Count)
                {
                    return $"Ctrl+{defaultKeys[index - 1]}";
                }
            }
            catch
            {
                // If anything fails, return empty string
            }

            return "";
        }

        #endregion

        #region Win32 API for dragging

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion
    }

}