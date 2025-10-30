using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _2017_test_binding.Models;
using _2017_test_binding.Services;

namespace _2017_test_binding.Views
{
    public class StatisticsForm : Form
    {
        private List<CommandData> _commandData;
        private CommandAnalyticsService _analyticsService;

        // Controls
        private TabControl _tabControl;
        private Label _totalCommandsLabel;
        private Label _uniqueCommandsLabel;
        private Label _mostUsedLabel;
        private Label _avgPerHourLabel;
        private DataGridView _commandsGrid;
        private DataGridView _sequencesGrid;

        public StatisticsForm(List<CommandData> commandData, CommandAnalyticsService analyticsService)
        {
            _commandData = commandData;
            _analyticsService = analyticsService;
            
            InitializeComponent();
            LoadStatistics();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Command Statistics";
            this.Size = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;

            // Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };

            // Overview Tab
            var overviewTab = new TabPage("Overview");
            CreateOverviewTab(overviewTab);
            _tabControl.TabPages.Add(overviewTab);

            // Commands Tab
            var commandsTab = new TabPage("Commands");
            CreateCommandsTab(commandsTab);
            _tabControl.TabPages.Add(commandsTab);

            // Sequences Tab
            var sequencesTab = new TabPage("Sequences");
            CreateSequencesTab(sequencesTab);
            _tabControl.TabPages.Add(sequencesTab);

            this.Controls.Add(_tabControl);

            this.ResumeLayout(false);
        }

        private void CreateOverviewTab(TabPage tab)
        {
            var statsGroup = new GroupBox
            {
                Text = "General Statistics",
                Location = new Point(10, 10),
                Size = new Size(550, 150)
            };

            _totalCommandsLabel = new Label
            {
                Text = "Total Commands Executed: 0",
                Location = new Point(10, 25),
                Size = new Size(300, 20)
            };

            _uniqueCommandsLabel = new Label
            {
                Text = "Unique Commands Used: 0",
                Location = new Point(10, 50),
                Size = new Size(300, 20)
            };

            _mostUsedLabel = new Label
            {
                Text = "Most Used Command: None",
                Location = new Point(10, 75),
                Size = new Size(300, 20)
            };

            _avgPerHourLabel = new Label
            {
                Text = "Average Commands/Hour: 0.0",
                Location = new Point(10, 100),
                Size = new Size(300, 20)
            };

            statsGroup.Controls.Add(_totalCommandsLabel);
            statsGroup.Controls.Add(_uniqueCommandsLabel);
            statsGroup.Controls.Add(_mostUsedLabel);
            statsGroup.Controls.Add(_avgPerHourLabel);

            var tipsGroup = new GroupBox
            {
                Text = "Usage Tips",
                Location = new Point(10, 170),
                Size = new Size(550, 200)
            };

            var tipsText = new RichTextBox
            {
                Location = new Point(10, 20),
                Size = new Size(530, 170),
                ReadOnly = true,
                Text = "• Use Ctrl+Q through Ctrl+C to quickly execute the first 9 items\n" +
                       "• Double-click any item to execute it\n" +
                       "• Use the search box to filter commands and values\n" +
                       "• The tracker learns your patterns and shows most-used items first\n" +
                       "• Command sequences help predict what you'll need next\n" +
                       "• All data is automatically saved between sessions\n" +
                       "• Switch between light and dark themes in Settings\n" +
                       "• Adjust window opacity for better workflow integration"
            };

            tipsGroup.Controls.Add(tipsText);

            tab.Controls.Add(statsGroup);
            tab.Controls.Add(tipsGroup);
        }

        private void CreateCommandsTab(TabPage tab)
        {
            var sortPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(550, 30)
            };

            var sortLabel = new Label
            {
                Text = "Sort by:",
                Location = new Point(0, 6),
                Size = new Size(50, 20)
            };

            var sortCombo = new ComboBox
            {
                Location = new Point(60, 3),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            sortCombo.Items.AddRange(new[] { "Usage Count", "Alphabetical", "Last Used" });
            sortCombo.SelectedIndex = 0;
            sortCombo.SelectedIndexChanged += SortCombo_SelectedIndexChanged;

            sortPanel.Controls.Add(sortLabel);
            sortPanel.Controls.Add(sortCombo);

            _commandsGrid = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(550, 340),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            _commandsGrid.Columns.Add("Command", "Command");
            _commandsGrid.Columns.Add("Count", "Usage Count");
            _commandsGrid.Columns.Add("LastUsed", "Last Used");
            _commandsGrid.Columns.Add("Values", "Stored Values");

            _commandsGrid.Columns[0].Width = 200;
            _commandsGrid.Columns[1].Width = 100;
            _commandsGrid.Columns[2].Width = 150;
            _commandsGrid.Columns[3].Width = 100;

            tab.Controls.Add(sortPanel);
            tab.Controls.Add(_commandsGrid);
        }

        private void CreateSequencesTab(TabPage tab)
        {
            var sequencesLabel = new Label
            {
                Text = "Most Common Command Sequences",
                Location = new Point(10, 10),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            _sequencesGrid = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(550, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            _sequencesGrid.Columns.Add("Sequence", "Command Sequence");
            _sequencesGrid.Columns.Add("Count", "Frequency");

            _sequencesGrid.Columns[0].Width = 400;
            _sequencesGrid.Columns[1].Width = 100;

            tab.Controls.Add(sequencesLabel);
            tab.Controls.Add(_sequencesGrid);
        }

        private void LoadStatistics()
        {
            var stats = _analyticsService.GetStatistics(_commandData);

            // Overview tab
            _totalCommandsLabel.Text = $"Total Commands Executed: {stats.TotalCommands}";
            _uniqueCommandsLabel.Text = $"Unique Commands Used: {stats.UniqueCommands}";
            _mostUsedLabel.Text = $"Most Used Command: {stats.MostUsedCommand ?? "None"}";
            _avgPerHourLabel.Text = $"Average Commands/Hour: {stats.AverageCommandsPerHour:F1}";

            // Commands tab
            LoadCommandsGrid();

            // Sequences tab
            LoadSequencesGrid(stats.CommandSequences);
        }

        private void LoadCommandsGrid()
        {
            _commandsGrid.Rows.Clear();

            foreach (var cmd in _commandData.OrderByDescending(c => c.UsageCount))
            {
                var row = new DataGridViewRow();
                row.CreateCells(_commandsGrid);
                
                row.Cells[0].Value = cmd.CommandName;
                row.Cells[1].Value = cmd.UsageCount;
                row.Cells[2].Value = cmd.LastUsed.ToString("yyyy-MM-dd HH:mm");
                row.Cells[3].Value = cmd.InputValues.Count;
                
                _commandsGrid.Rows.Add(row);
            }
        }

        private void LoadSequencesGrid(List<CommandSequence> sequences)
        {
            _sequencesGrid.Rows.Clear();

            foreach (var seq in sequences)
            {
                var row = new DataGridViewRow();
                row.CreateCells(_sequencesGrid);
                
                row.Cells[0].Value = seq.Sequence;
                row.Cells[1].Value = seq.Count;
                
                _sequencesGrid.Rows.Add(row);
            }
        }

        private void SortCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            _commandsGrid.Rows.Clear();

            IEnumerable<CommandData> sortedData;
            switch (combo.SelectedIndex)
            {
                case 1: // Alphabetical
                    sortedData = _commandData.OrderBy(c => c.CommandName);
                    break;
                case 2: // Last Used
                    sortedData = _commandData.OrderByDescending(c => c.LastUsed);
                    break;
                default: // Usage Count
                    sortedData = _commandData.OrderByDescending(c => c.UsageCount);
                    break;
            }

            foreach (var cmd in sortedData)
            {
                var row = new DataGridViewRow();
                row.CreateCells(_commandsGrid);
                
                row.Cells[0].Value = cmd.CommandName;
                row.Cells[1].Value = cmd.UsageCount;
                row.Cells[2].Value = cmd.LastUsed.ToString("yyyy-MM-dd HH:mm");
                row.Cells[3].Value = cmd.InputValues.Count;
                
                _commandsGrid.Rows.Add(row);
            }
        }
    }
}