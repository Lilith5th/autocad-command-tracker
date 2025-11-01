using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AutoCadCommandTracker.Services;

namespace AutoCadCommandTracker.Views
{
    public partial class SettingsForm : Form
    {
        private DataPersistenceService _persistenceService;
        private UserSettings _settings;

        // Event for shortcut changes
        public delegate void ShortcutKeysChangedHandler(List<char> newKeys);
        public event ShortcutKeysChangedHandler ShortcutKeysChanged;

        // UI Controls are now declared in SettingsForm.Designer.cs

        public SettingsForm(DataPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
            _settings = _persistenceService.LoadSettings();
            
            InitializeComponent(); // From Designer file
            CreateTabs();
            LoadCurrentSettings();
        }

        private void CreateTabs()
        {
            // Initialize controls that are used across tabs
            _lightThemeRadio = new System.Windows.Forms.RadioButton();
            _darkThemeRadio = new System.Windows.Forms.RadioButton();
            _opacityTrackBar = new System.Windows.Forms.TrackBar();
            _opacityLabel = new System.Windows.Forms.Label();
            _resizableCheckBox = new System.Windows.Forms.CheckBox();
            _maxValuesNumeric = new System.Windows.Forms.NumericUpDown();
            _trackTimeCheckBox = new System.Windows.Forms.CheckBox();
            _trackSequencesCheckBox = new System.Windows.Forms.CheckBox();
            _shortcutTextBoxes = new System.Windows.Forms.TextBox[9];
            _resetShortcutsButton = new System.Windows.Forms.Button();

            // Create and add the tabs
            var appearanceTab = new TabPage("Appearance");
            CreateAppearanceTab(appearanceTab);
            _tabControl.TabPages.Add(appearanceTab);

            var behaviorTab = new TabPage("Behavior");
            CreateBehaviorTab(behaviorTab);
            _tabControl.TabPages.Add(behaviorTab);

            var shortcutsTab = new TabPage("Shortcuts");
            CreateShortcutsTab(shortcutsTab);
            _tabControl.TabPages.Add(shortcutsTab);

            // Create button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(235, 8),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(315, 8),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };

            var applyButton = new Button
            {
                Text = "Apply",
                Location = new Point(155, 8),
                Size = new Size(75, 23)
            };
            applyButton.Click += ApplyButton_Click;

            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(applyButton);

            // _tabControl is already added in Designer, just add the button panel
            Controls.Add(buttonPanel);
        }

        // InitializeComponent is now in SettingsForm.Designer.cs

        private void CreateAppearanceTab(TabPage tab)
        {
            var themeGroup = new GroupBox
            {
                Text = "Theme",
                Location = new Point(10, 10),
                Size = new Size(350, 60)
            };

            // Initialize the radio buttons that are declared in the Designer
            _lightThemeRadio.Text = "Light Theme";
            _lightThemeRadio.Location = new Point(10, 20);
            _lightThemeRadio.Checked = true;

            _darkThemeRadio.Text = "Dark Theme";
            _darkThemeRadio.Location = new Point(10, 40);

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

            // Initialize the controls that are declared in the Designer
            _opacityTrackBar.Location = new Point(80, 20);
            _opacityTrackBar.Size = new Size(200, 45);
            _opacityTrackBar.Minimum = 1;
            _opacityTrackBar.Maximum = 10;
            _opacityTrackBar.TickFrequency = 1;
            _opacityTrackBar.Value = 6;
            _opacityTrackBar.ValueChanged += OpacityTrackBar_ValueChanged;

            _opacityLabel.Text = "0.6";
            _opacityLabel.Location = new Point(290, 25);
            _opacityLabel.Size = new Size(40, 20);

            _resizableCheckBox.Text = "Resizable Window";
            _resizableCheckBox.Location = new Point(10, 65);
            _resizableCheckBox.Checked = true;

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

            // Initialize the controls that are declared in the Designer
            _maxValuesNumeric.Location = new Point(140, 23);
            _maxValuesNumeric.Size = new Size(80, 20);
            _maxValuesNumeric.Minimum = 10;
            _maxValuesNumeric.Maximum = 1000;
            _maxValuesNumeric.Value = 50;

            _trackTimeCheckBox.Text = "Track Time Patterns";
            _trackTimeCheckBox.Location = new Point(10, 50);

            dataGroup.Controls.Add(maxValuesLabel);
            dataGroup.Controls.Add(_maxValuesNumeric);
            dataGroup.Controls.Add(_trackTimeCheckBox);

            var analyticsGroup = new GroupBox
            {
                Text = "Advanced Analytics",
                Location = new Point(10, 100),
                Size = new Size(350, 80)
            };

            // Initialize the control that is declared in the Designer
            _trackSequencesCheckBox.Text = "Track Command Sequences";
            _trackSequencesCheckBox.Location = new Point(10, 25);
            _trackSequencesCheckBox.Checked = true;

            analyticsGroup.Controls.Add(_trackSequencesCheckBox);

            tab.Controls.Add(dataGroup);
            tab.Controls.Add(analyticsGroup);
        }

        private void CreateShortcutsTab(TabPage tab)
        {
            var shortcutsLabel = new Label
            {
                Text = "Customize Keyboard Shortcuts (3x3 Grid Layout)",
                Location = new Point(10, 10),
                Size = new Size(350, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var instructionLabel = new Label
            {
                Text = "Enter single letters (A-Z). Use Ctrl+` to toggle shortcut override.",
                Location = new Point(10, 35),
                Size = new Size(350, 20),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.DarkBlue
            };

            // Initialize the shortcut textboxes array that is declared in the Designer
            // _shortcutTextBoxes is already created in the Designer
            var positions = new[]
            {
                new Point(50, 70),   new Point(100, 70),  new Point(150, 70),   // Q W E
                new Point(50, 100),  new Point(100, 100), new Point(150, 100),  // A S D  
                new Point(50, 130),  new Point(100, 130), new Point(150, 130)   // Z X C
            };

            var labels = new[] { "Pos 1:", "Pos 2:", "Pos 3:", "Pos 4:", "Pos 5:", "Pos 6:", "Pos 7:", "Pos 8:", "Pos 9:" };

            for (int i = 0; i < 9; i++)
            {
                // Position label
                var posLabel = new Label
                {
                    Text = labels[i],
                    Location = new Point(positions[i].X - 40, positions[i].Y + 3),
                    Size = new Size(35, 20),
                    TextAlign = ContentAlignment.MiddleRight
                };

                // Initialize shortcut input (already created in Designer)
                _shortcutTextBoxes[i] = new TextBox();
                _shortcutTextBoxes[i].Location = positions[i];
                _shortcutTextBoxes[i].Size = new Size(30, 20);
                _shortcutTextBoxes[i].MaxLength = 1;
                _shortcutTextBoxes[i].TextAlign = HorizontalAlignment.Center;
                _shortcutTextBoxes[i].Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                // Restrict to letters only
                _shortcutTextBoxes[i].KeyPress += (sender, e) =>
                {
                    if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                    else if (char.IsLetter(e.KeyChar))
                    {
                        e.KeyChar = char.ToUpper(e.KeyChar);
                    }
                };

                // Display current shortcut
                var currentLabel = new Label
                {
                    Text = $"Ctrl+{_settings.CustomShortcutKeys[i]}",
                    Location = new Point(positions[i].X + 40, positions[i].Y + 3),
                    Size = new Size(50, 20),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.Gray
                };

                tab.Controls.Add(posLabel);
                tab.Controls.Add(_shortcutTextBoxes[i]);
                tab.Controls.Add(currentLabel);
            }

            // Initialize reset button (already declared in Designer)
            _resetShortcutsButton.Text = "Reset to Default";
            _resetShortcutsButton.Location = new Point(50, 170);
            _resetShortcutsButton.Size = new Size(120, 25);
            _resetShortcutsButton.Click += ResetShortcuts_Click;

            // Info panel
            var infoLabel = new Label
            {
                Text = "Fixed shortcuts:\n• Enter - Execute selected item\n• Double-click - Execute item\n• Ctrl+` - Toggle shortcut override",
                Location = new Point(220, 70),
                Size = new Size(150, 100),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.DarkGreen
            };

            tab.Controls.Add(shortcutsLabel);
            tab.Controls.Add(instructionLabel);
            tab.Controls.Add(_resetShortcutsButton);
            tab.Controls.Add(infoLabel);
        }

        private void LoadCurrentSettings()
        {
            _lightThemeRadio.Checked = !_settings.IsDarkTheme;
            _darkThemeRadio.Checked = _settings.IsDarkTheme;
            
            _opacityTrackBar.Value = (int)(_settings.WindowOpacity * 10);
            _opacityLabel.Text = _settings.WindowOpacity.ToString("F1");
            
            _resizableCheckBox.Checked = _settings.IsResizable;
            _maxValuesNumeric.Value = _settings.MaxStoredValues;
            _trackTimeCheckBox.Checked = _settings.TrackTimePatterns;
            _trackSequencesCheckBox.Checked = _settings.TrackCommandSequences;
            
            // Load shortcut keys
            if (_shortcutTextBoxes != null)
            {
                for (int i = 0; i < _shortcutTextBoxes.Length && i < _settings.CustomShortcutKeys.Count; i++)
                {
                    _shortcutTextBoxes[i].Text = _settings.CustomShortcutKeys[i].ToString();
                }
            }
        }

        private void SaveSettings()
        {
            _settings.IsDarkTheme = _darkThemeRadio.Checked;
            _settings.WindowOpacity = _opacityTrackBar.Value / 10.0;
            _settings.IsResizable = _resizableCheckBox.Checked;
            _settings.MaxStoredValues = (int)_maxValuesNumeric.Value;
            _settings.TrackTimePatterns = _trackTimeCheckBox.Checked;
            _settings.TrackCommandSequences = _trackSequencesCheckBox.Checked;
            
            // Save shortcut keys
            if (_shortcutTextBoxes != null)
            {
                var newShortcuts = new List<char>();
                for (int i = 0; i < _shortcutTextBoxes.Length; i++)
                {
                    var text = _shortcutTextBoxes[i].Text.Trim().ToUpper();
                    if (text.Length == 1 && char.IsLetter(text[0]))
                    {
                        newShortcuts.Add(text[0]);
                    }
                    else
                    {
                        // If invalid, keep the current shortcut
                        newShortcuts.Add(_settings.CustomShortcutKeys[i]);
                    }
                }
                
                if (newShortcuts.Count == 9)
                {
                    _settings.CustomShortcutKeys = newShortcuts;
                    // Notify about shortcut changes
                    ShortcutKeysChanged?.Invoke(newShortcuts);
                }
            }
            
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

        private void ResetShortcuts_Click(object sender, EventArgs e)
        {
            var defaultKeys = UserSettings.GetDefaultShortcutKeys();
            for (int i = 0; i < _shortcutTextBoxes.Length && i < defaultKeys.Count; i++)
            {
                _shortcutTextBoxes[i].Text = defaultKeys[i].ToString();
            }
        }
    }
}