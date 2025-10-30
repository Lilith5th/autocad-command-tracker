using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _2017_test_binding.Models;
using _2017_test_binding.Services;

namespace _2017_test_binding.Views
{
    public class MainFormEnhanced : Form
    {
        private DataPersistenceService _persistenceService;
        private CommandAnalyticsService _analyticsService;
        private List<CommandData> _commandDataList;
        private Timer _updateTimer;
        private bool _isAtMainPrompt = true;
        private string _currentCommand;

        // UI Controls
        private DataGridView _dataGrid;
        private TextBox _searchBox;
        private Label _statusLabel;
        private Label _titleLabel;
        private ComboBox _themeCombo;
        private TrackBar _opacityTrackBar;
        private Button _settingsButton;
        private Button _statsButton;

        public MainFormEnhanced()
        {
            InitializeComponent();
            InitializeServices();
            SetupForm();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(300, 400);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "AutoCAD Command Tracker";
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Title Panel
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.FromArgb(200, 200, 200)
            };

            _titleLabel = new Label
            {
                Text = "Commands",
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0)
            };

            _settingsButton = new Button
            {
                Text = "⚙",
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(245, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent
            };
            _settingsButton.Click += SettingsButton_Click;

            _statsButton = new Button
            {
                Text = "📊",
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(215, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent
            };
            _statsButton.Click += StatsButton_Click;

            titlePanel.Controls.Add(_titleLabel);
            titlePanel.Controls.Add(_settingsButton);
            titlePanel.Controls.Add(_statsButton);

            // Search Panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(5)
            };

            _searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = "Search..."
            };
            _searchBox.GotFocus += (s, e) => { if (_searchBox.Text == "Search...") _searchBox.Text = ""; };
            _searchBox.LostFocus += (s, e) => { if (string.IsNullOrEmpty(_searchBox.Text)) _searchBox.Text = "Search..."; };
            _searchBox.TextChanged += SearchBox_TextChanged;

            searchPanel.Controls.Add(_searchBox);

            // Data Grid
            _dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F)
            };

            _dataGrid.Columns.Add("Index", "#");
            _dataGrid.Columns.Add("Command", "Command/Value");
            _dataGrid.Columns.Add("Alias", "Alias");
            _dataGrid.Columns.Add("Shortcut", "Shortcut");
            _dataGrid.Columns.Add("Count", "Count");
            _dataGrid.Columns.Add("Type", "Type");

            _dataGrid.Columns[0].Width = 30;
            _dataGrid.Columns[1].Width = 120;
            _dataGrid.Columns[2].Width = 45;
            _dataGrid.Columns[3].Width = 60;
            _dataGrid.Columns[4].Width = 45;
            _dataGrid.Columns[5].Width = 50;

            _dataGrid.DoubleClick += DataGrid_DoubleClick;
            _dataGrid.KeyDown += DataGrid_KeyDown;

            // Status Panel
            var statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            _statusLabel = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8F),
                Padding = new Padding(5, 0, 0, 0)
            };

            statusPanel.Controls.Add(_statusLabel);

            // Add controls to form
            this.Controls.Add(_dataGrid);
            this.Controls.Add(searchPanel);
            this.Controls.Add(titlePanel);
            this.Controls.Add(statusPanel);

            this.ResumeLayout(false);
        }

        private void InitializeServices()
        {
            _persistenceService = new DataPersistenceService();
            _analyticsService = new CommandAnalyticsService();
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
        }

        private void LoadSettings()
        {
            var settings = _persistenceService.LoadSettings();
            
            this.Location = new Point((int)settings.WindowLeft, (int)settings.WindowTop);
            this.Size = new Size((int)settings.WindowWidth, (int)settings.WindowHeight);
            this.Opacity = settings.WindowOpacity;
            
            ApplyTheme(settings.Theme);
        }

        private void ApplyTheme(string theme)
        {
            if (theme == "Dark")
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
            if (e.Control)
            {
                int index = -1;
                switch (e.KeyCode)
                {
                    case Keys.Q: index = 0; break;
                    case Keys.W: index = 1; break;
                    case Keys.E: index = 2; break;
                    case Keys.A: index = 3; break;
                    case Keys.S: index = 4; break;
                    case Keys.D: index = 5; break;
                    case Keys.Y: index = 6; break;
                    case Keys.X: index = 7; break;
                    case Keys.C: index = 8; break;
                }

                if (index >= 0 && index < _dataGrid.Rows.Count)
                {
                    ExecuteItemAtIndex(index);
                    e.Handled = true;
                }
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
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSettings();
                }
            }
        }

        private void ShowStatisticsDialog()
        {
            using (var statsForm = new StatisticsForm(_commandDataList, _analyticsService))
            {
                statsForm.ShowDialog();
            }
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
            switch (index)
            {
                case 1: return "Ctrl+Q";
                case 2: return "Ctrl+W";
                case 3: return "Ctrl+E";
                case 4: return "Ctrl+A";
                case 5: return "Ctrl+S";
                case 6: return "Ctrl+D";
                case 7: return "Ctrl+Y";
                case 8: return "Ctrl+X";
                case 9: return "Ctrl+C";
                default: return "";
            }
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

    public class DisplayItem
    {
        public string DisplayText { get; set; }
        public string Alias { get; set; }
        public int UsageCount { get; set; }
        public string Type { get; set; }
        public object RawData { get; set; }
    }
}