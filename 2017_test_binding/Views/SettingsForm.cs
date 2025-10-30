using System;
using System.Drawing;
using System.Windows.Forms;
using _2017_test_binding.Services;

namespace _2017_test_binding.Views
{
    public class SettingsForm : Form
    {
        private DataPersistenceService _persistenceService;
        private UserSettings _settings;

        // Controls
        private TabControl _tabControl;
        private RadioButton _lightThemeRadio;
        private RadioButton _darkThemeRadio;
        private TrackBar _opacityTrackBar;
        private Label _opacityLabel;
        private CheckBox _resizableCheckBox;
        private NumericUpDown _maxValuesNumeric;
        private CheckBox _trackTimeCheckBox;
        private CheckBox _trackSequencesCheckBox;
        private Button _okButton;
        private Button _cancelButton;
        private Button _applyButton;

        public SettingsForm(DataPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
            _settings = _persistenceService.LoadSettings();
            
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Settings";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };

            // Appearance Tab
            var appearanceTab = new TabPage("Appearance");
            CreateAppearanceTab(appearanceTab);
            _tabControl.TabPages.Add(appearanceTab);

            // Behavior Tab
            var behaviorTab = new TabPage("Behavior");
            CreateBehaviorTab(behaviorTab);
            _tabControl.TabPages.Add(behaviorTab);

            // Shortcuts Tab
            var shortcutsTab = new TabPage("Shortcuts");
            CreateShortcutsTab(shortcutsTab);
            _tabControl.TabPages.Add(shortcutsTab);

            // Button Panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            _okButton = new Button
            {
                Text = "OK",
                Size = new Size(75, 23),
                Location = new Point(235, 8),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(75, 23),
                Location = new Point(315, 8),
                DialogResult = DialogResult.Cancel
            };

            _applyButton = new Button
            {
                Text = "Apply",
                Size = new Size(75, 23),
                Location = new Point(155, 8)
            };
            _applyButton.Click += ApplyButton_Click;

            buttonPanel.Controls.Add(_okButton);
            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_applyButton);

            this.Controls.Add(_tabControl);
            this.Controls.Add(buttonPanel);

            this.ResumeLayout(false);
        }

        private void CreateAppearanceTab(TabPage tab)
        {
            var themeGroup = new GroupBox
            {
                Text = "Theme",
                Location = new Point(10, 10),
                Size = new Size(350, 60)
            };

            _lightThemeRadio = new RadioButton
            {
                Text = "Light Theme",
                Location = new Point(10, 20),
                Checked = true
            };

            _darkThemeRadio = new RadioButton
            {
                Text = "Dark Theme",
                Location = new Point(10, 40)
            };

            themeGroup.Controls.Add(_lightThemeRadio);
            themeGroup.Controls.Add(_darkThemeRadio);

            var windowGroup = new GroupBox
            {
                Text = "Window Settings",
                Location = new Point(10, 80),
                Size = new Size(350, 100)
            };

            var opacityLabel = new Label
            {
                Text = "Opacity:",
                Location = new Point(10, 25),
                Size = new Size(60, 20)
            };

            _opacityTrackBar = new TrackBar
            {
                Location = new Point(80, 20),
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 10,
                TickFrequency = 1,
                Value = 6
            };
            _opacityTrackBar.ValueChanged += OpacityTrackBar_ValueChanged;

            _opacityLabel = new Label
            {
                Text = "0.6",
                Location = new Point(290, 25),
                Size = new Size(40, 20)
            };

            _resizableCheckBox = new CheckBox
            {
                Text = "Resizable Window",
                Location = new Point(10, 65),
                Checked = true
            };

            windowGroup.Controls.Add(opacityLabel);
            windowGroup.Controls.Add(_opacityTrackBar);
            windowGroup.Controls.Add(_opacityLabel);
            windowGroup.Controls.Add(_resizableCheckBox);

            tab.Controls.Add(themeGroup);
            tab.Controls.Add(windowGroup);
        }

        private void CreateBehaviorTab(TabPage tab)
        {
            var dataGroup = new GroupBox
            {
                Text = "Data Management",
                Location = new Point(10, 10),
                Size = new Size(350, 80)
            };

            var maxValuesLabel = new Label
            {
                Text = "Max Stored Values:",
                Location = new Point(10, 25),
                Size = new Size(120, 20)
            };

            _maxValuesNumeric = new NumericUpDown
            {
                Location = new Point(140, 23),
                Size = new Size(80, 20),
                Minimum = 10,
                Maximum = 1000,
                Value = 50
            };

            _trackTimeCheckBox = new CheckBox
            {
                Text = "Track Time Patterns",
                Location = new Point(10, 50)
            };

            dataGroup.Controls.Add(maxValuesLabel);
            dataGroup.Controls.Add(_maxValuesNumeric);
            dataGroup.Controls.Add(_trackTimeCheckBox);

            var analyticsGroup = new GroupBox
            {
                Text = "Advanced Analytics",
                Location = new Point(10, 100),
                Size = new Size(350, 80)
            };

            _trackSequencesCheckBox = new CheckBox
            {
                Text = "Track Command Sequences",
                Location = new Point(10, 25),
                Checked = true
            };

            analyticsGroup.Controls.Add(_trackSequencesCheckBox);

            tab.Controls.Add(dataGroup);
            tab.Controls.Add(analyticsGroup);
        }

        private void CreateShortcutsTab(TabPage tab)
        {
            var shortcutsLabel = new Label
            {
                Text = "Keyboard Shortcuts",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var shortcutsText = new RichTextBox
            {
                Location = new Point(10, 40),
                Size = new Size(350, 150),
                ReadOnly = true,
                Text = "Ctrl+Q - Execute item 1\n" +
                       "Ctrl+W - Execute item 2\n" +
                       "Ctrl+E - Execute item 3\n" +
                       "Ctrl+A - Execute item 4\n" +
                       "Ctrl+S - Execute item 5\n" +
                       "Ctrl+D - Execute item 6\n" +
                       "Ctrl+Y - Execute item 7\n" +
                       "Ctrl+X - Execute item 8\n" +
                       "Ctrl+C - Execute item 9\n" +
                       "\nEnter - Execute selected item\n" +
                       "Double-click - Execute item"
            };

            tab.Controls.Add(shortcutsLabel);
            tab.Controls.Add(shortcutsText);
        }

        private void LoadCurrentSettings()
        {
            _lightThemeRadio.Checked = _settings.Theme != "Dark";
            _darkThemeRadio.Checked = _settings.Theme == "Dark";
            
            _opacityTrackBar.Value = (int)(_settings.WindowOpacity * 10);
            _opacityLabel.Text = _settings.WindowOpacity.ToString("F1");
            
            _resizableCheckBox.Checked = _settings.IsResizable;
            _maxValuesNumeric.Value = _settings.MaxStoredValues;
            _trackTimeCheckBox.Checked = _settings.TrackTimePatterns;
            _trackSequencesCheckBox.Checked = _settings.TrackCommandSequences;
        }

        private void SaveSettings()
        {
            _settings.Theme = _darkThemeRadio.Checked ? "Dark" : "Light";
            _settings.WindowOpacity = _opacityTrackBar.Value / 10.0;
            _settings.IsResizable = _resizableCheckBox.Checked;
            _settings.MaxStoredValues = (int)_maxValuesNumeric.Value;
            _settings.TrackTimePatterns = _trackTimeCheckBox.Checked;
            _settings.TrackCommandSequences = _trackSequencesCheckBox.Checked;
            
            _persistenceService.SaveSettings(_settings);
        }

        private void OpacityTrackBar_ValueChanged(object sender, EventArgs e)
        {
            _opacityLabel.Text = (_opacityTrackBar.Value / 10.0).ToString("F1");
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }
    }
}