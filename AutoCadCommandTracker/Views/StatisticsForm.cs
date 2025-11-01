using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AutoCadCommandTracker.Models;
using AutoCadCommandTracker.Services;

namespace AutoCadCommandTracker.Views
{
    public partial class StatisticsForm : Form
    {
        private List<CommandData> _commandData;
        private DataPersistenceService _persistenceService;

        // UI Controls are now declared in StatisticsForm.Designer.cs

        public StatisticsForm(List<CommandData> commandData, DataPersistenceService persistenceService)
        {
            _commandData = commandData;
            _persistenceService = persistenceService;
            
            InitializeComponent(); // From Designer file
            CreateTabs();
            LoadStatistics();
        }

        private void CreateTabs()
        {
            // Initialize controls that are used across tabs
            _totalCommandsLabel = new System.Windows.Forms.Label();
            _uniqueCommandsLabel = new System.Windows.Forms.Label();
            _mostUsedLabel = new System.Windows.Forms.Label();
            _avgPerHourLabel = new System.Windows.Forms.Label();
            _commandsGrid = new System.Windows.Forms.DataGridView();
            _sequencesGrid = new System.Windows.Forms.DataGridView();

            // Create and add the tabs
            var overviewTab = new TabPage("Overview");
            CreateOverviewTab(overviewTab);
            _tabControl.TabPages.Add(overviewTab);

            var commandsTab = new TabPage("Commands");
            CreateCommandsTab(commandsTab);
            _tabControl.TabPages.Add(commandsTab);

            var sequencesTab = new TabPage("Sequences");
            CreateSequencesTab(sequencesTab);
            _tabControl.TabPages.Add(sequencesTab);

            // _tabControl is already added in Designer
        }

        // InitializeComponent is now in StatisticsForm.Designer.cs

        private void CreateOverviewTab(TabPage tab)
        {
            var statsGroup = new GroupBox
            {
                Text = "General Statistics",
                Location = new Point(10, 10),
                Size = new Size(550, 150)
            };

            // Initialize the labels that are declared in the Designer
            _totalCommandsLabel.Text = "Total Commands Executed: 0";
            _totalCommandsLabel.Location = new Point(10, 25);
            _totalCommandsLabel.Size = new Size(300, 20);

            _uniqueCommandsLabel.Text = "Unique Commands Used: 0";
            _uniqueCommandsLabel.Location = new Point(10, 50);
            _uniqueCommandsLabel.Size = new Size(300, 20);

            _mostUsedLabel.Text = "Most Used Command: None";
            _mostUsedLabel.Location = new Point(10, 75);
            _mostUsedLabel.Size = new Size(300, 20);

            _avgPerHourLabel.Text = "Average Commands/Hour: 0.0";
            _avgPerHourLabel.Location = new Point(10, 100);
            _avgPerHourLabel.Size = new Size(300, 20);

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

            // Initialize the _commandsGrid that is declared in the Designer
            _commandsGrid.Location = new Point(10, 50);
            _commandsGrid.Size = new Size(550, 340);
            _commandsGrid.AllowUserToAddRows = false;
            _commandsGrid.AllowUserToDeleteRows = false;
            _commandsGrid.ReadOnly = true;
            _commandsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _commandsGrid.MultiSelect = false;
            _commandsGrid.RowHeadersVisible = false;

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

            // Initialize the _sequencesGrid that is declared in the Designer
            _sequencesGrid.Location = new Point(10, 40);
            _sequencesGrid.Size = new Size(550, 350);
            _sequencesGrid.AllowUserToAddRows = false;
            _sequencesGrid.AllowUserToDeleteRows = false;
            _sequencesGrid.ReadOnly = true;
            _sequencesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _sequencesGrid.MultiSelect = false;
            _sequencesGrid.RowHeadersVisible = false;

            _sequencesGrid.Columns.Add("Sequence", "Command Sequence");
            _sequencesGrid.Columns.Add("Count", "Frequency");

            _sequencesGrid.Columns[0].Width = 400;
            _sequencesGrid.Columns[1].Width = 100;

            tab.Controls.Add(sequencesLabel);
            tab.Controls.Add(_sequencesGrid);
        }

        private void LoadStatistics()
        {
            var stats = _persistenceService.GetStatistics(_commandData);

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