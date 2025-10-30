using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using _2017_test_binding.Models;
using _2017_test_binding.Services;
using _2017_test_binding.Views;

namespace _2017_test_binding
{
    public class MyCommandsWinForms : IExtensionApplication, INotifyPropertyChanged
    {
        private MainFormEnhanced _window;
        private DataPersistenceService _persistenceService;
        private CommandAnalyticsService _analyticsService;
        private List<CommandData> _commandDataList;
        private string _activeCommand;
        private string _previousCommand;
        private bool _isAtMainPrompt = true;
        private readonly object _lockObject = new object();

        // Keyboard override system (from original implementation)
        private const int WM_KEYDOWN = 256;
        // Use 3x3 grid layout: Q-W-E, A-S-D, Z-X-C (avoiding Y which swaps with Z on QWERTZ)
        private readonly List<char> _shortcutKeys = new List<char>()
            { 'Q', 'W', 'E', 'A', 'S', 'D', 'Z', 'X', 'C' };
        private bool _keyboardOverrideEnabled = true;

        // Startup commands with categories
        private readonly Dictionary<string, string[]> _startupCommands = new Dictionary<string, string[]>
        {
            ["Drawing"] = new[] { "PLINE", "LINE", "CIRCLE", "ARC", "ELLIPSE", "RECTANGLE" },
            ["Modify"] = new[] { "OFFSET", "TRIM", "EXTEND", "FILLET", "CHAMFER", "SCALE" },
            ["Annotation"] = new[] { "DIMENSION", "TEXT", "MTEXT", "LEADER", "TABLE" },
            ["Layers"] = new[] { "LAYER", "LAYCUR", "LAYISO", "LAYUNISO" },
            ["View"] = new[] { "ZOOM", "PAN", "REGEN", "REDRAW" }
        };

        #region IExtensionApplication

        public void Initialize()
        {
            try
            {
                // Initialize services
                _persistenceService = new DataPersistenceService();
                _analyticsService = new CommandAnalyticsService();
                _commandDataList = new List<CommandData>();

                // Load saved data
                LoadCommandData();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during initialization: {ex.Message}");
            }
        }

        public void Terminate()
        {
            try
            {
                // Save data before termination
                SaveCommandData();

                // Cleanup
                Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage -= OnPreTranslateMessage;
                UnregisterEventHandlers();
                
                if (_window != null)
                {
                    _window.Close();
                    _window = null;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during termination: {ex.Message}");
            }
        }

        #endregion

        [CommandMethod("CMDTRACKWF", CommandFlags.Session)]
        public void StartCommandTrackingWinForms()
        {
            try
            {
                // Ensure services are initialized
                if (_persistenceService == null || _analyticsService == null)
                {
                    Initialize();
                }

                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var ed = doc.Editor;

                // Create and show window
                if (_window == null)
                {
                    _window = new MainFormEnhanced();
                    _window.Show();
                    _window.StartTracking();
                    UpdateWindowColorForOverrideState(); // Set initial color state
                    Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
                }

                // Register event handlers
                RegisterEventHandlers(doc);

                // Enable keyboard shortcuts (like original pokretanje)
                Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage += OnPreTranslateMessage;

                // Use base class method to add default commands (like original pokretanje)
                ed.WriteMessage($"\nBefore adding defaults: {_commandDataList?.Count ?? 0} commands in list.");
                
                if (_commandDataList == null || !_commandDataList.Any())
                {
                    ed.WriteMessage("\nAdding default commands using proven logic...");
                    var allCommands = _startupCommands.Values.SelectMany(x => x).ToList();
                    foreach (var cmd in allCommands)
                    {
                        GetOrCreateCommand(cmd);
                    }
                    ed.WriteMessage($"\nAfter adding defaults: {_commandDataList.Count} commands in list.");
                }
                else
                {
                    ed.WriteMessage($"\nSkipping defaults - already have {_commandDataList.Count} commands.");
                }

                // Update display with current data
                UpdateDisplay();

                // Force an immediate refresh to ensure data is displayed
                if (_window != null)
                {
                    _window.UpdateDisplayData(new List<CommandData>(_commandDataList), true);
                }

                ed.WriteMessage($"\nWinForms command tracking started. {_commandDataList.Count} commands available. Window is now active.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting command tracking: {ex.Message}");
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nError starting command tracking: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACKWF_STOP", CommandFlags.Session)]
        public void StopCommandTrackingWinForms()
        {
            try
            {
                // Disable keyboard shortcuts
                Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage -= OnPreTranslateMessage;
                
                UnregisterEventHandlers();
                
                if (_window != null)
                {
                    _window.StopTracking();
                    _window.Close();
                    _window = null;
                }

                SaveCommandData();

                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage("\nWinForms command tracking stopped.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping command tracking: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACKWF_ADD", CommandFlags.Session)]
        public void AddDefaultCommands()
        {
            try
            {
                // Ensure services are initialized
                if (_persistenceService == null || _analyticsService == null)
                {
                    Initialize();
                }

                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                
                // Add all default commands
                foreach (var category in _startupCommands)
                {
                    foreach (var cmd in category.Value)
                    {
                        var cmdData = GetOrCreateCommand(cmd);
                        cmdData.IncrementUsage(); // Give them initial usage count
                        
                        // Add common input values
                        AddCommonInputValues(cmdData, cmd);
                    }
                }

                ed?.WriteMessage($"\nAdded {_commandDataList.Count} commands with input values.");

                // Update display if window is open
                if (_window != null)
                {
                    UpdateDisplay();
                    _window.UpdateDisplayData(new List<CommandData>(_commandDataList), true);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding default commands: {ex.Message}");
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nError adding commands: {ex.Message}");
            }
        }

        #region Event Handlers

        private void RegisterEventHandlers(Document doc)
        {
            try
            {
                // Document events
                doc.CommandWillStart += OnCommandWillStart;
                doc.CommandEnded += OnCommandEnded;
                doc.CommandCancelled += OnCommandCancelled;
                doc.CommandFailed += OnCommandFailed;

                // Editor events
                var ed = doc.Editor;
                ed.PromptingForPoint += OnPromptingForPoint;
                ed.PromptingForDistance += OnPromptingForDistance;
                ed.PromptingForInteger += OnPromptingForInteger;
                ed.PromptingForDouble += OnPromptingForDouble;
                ed.PromptingForString += OnPromptingForString;
                ed.PromptingForKeyword += OnPromptingForKeyword;
                ed.PromptingForAngle += OnPromptingForAngle;
                ed.PromptingForCorner += OnPromptingForCorner;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering event handlers: {ex.Message}");
            }
        }

        private void UnregisterEventHandlers()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    // Document events
                    doc.CommandWillStart -= OnCommandWillStart;
                    doc.CommandEnded -= OnCommandEnded;
                    doc.CommandCancelled -= OnCommandCancelled;
                    doc.CommandFailed -= OnCommandFailed;

                    // Editor events
                    var ed = doc.Editor;
                    ed.PromptingForPoint -= OnPromptingForPoint;
                    ed.PromptingForDistance -= OnPromptingForDistance;
                    ed.PromptingForInteger -= OnPromptingForInteger;
                    ed.PromptingForDouble -= OnPromptingForDouble;
                    ed.PromptingForString -= OnPromptingForString;
                    ed.PromptingForKeyword -= OnPromptingForKeyword;
                    ed.PromptingForAngle -= OnPromptingForAngle;
                    ed.PromptingForCorner -= OnPromptingForCorner;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unregistering event handlers: {ex.Message}");
            }
        }

        #endregion

        #region Command Events

        private void OnCommandWillStart(object sender, CommandEventArgs e)
        {
            try
            {
                lock (_lockObject)
                {
                    _previousCommand = _activeCommand;
                    _activeCommand = e.GlobalCommandName;
                    _isAtMainPrompt = false;

                    var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                    ed?.WriteMessage($"\nCommand '{_activeCommand}' started - keyboard shortcuts disabled");

                    // Use the proven check_and_add_command logic pattern
                    CheckAndAddCommand(_activeCommand);
                    
                    // Add common input values for specific commands
                    var command = GetOrCreateCommand(_activeCommand);
                    AddCommonInputValues(command, _activeCommand);

                    // Track command sequence
                    if (!string.IsNullOrEmpty(_previousCommand))
                    {
                        var prevCmd = GetOrCreateCommand(_previousCommand);
                        prevCmd.AddFollowedCommand(_activeCommand);
                    }

                    _analyticsService?.TrackCommand(_activeCommand, _commandDataList);
                }

                UpdateDisplay();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnCommandWillStart: {ex.Message}");
            }
        }

        private void OnCommandEnded(object sender, CommandEventArgs e)
        {
            try
            {
                _isAtMainPrompt = true;
                _activeCommand = null; // Clear active command
                UpdateDisplay();
                
                // Auto-save periodically
                AutoSave();
                
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' ended - keyboard shortcuts re-enabled");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnCommandEnded: {ex.Message}");
            }
        }

        private void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null; // Clear active command
            UpdateDisplay();
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' cancelled - keyboard shortcuts re-enabled");
        }

        private void OnCommandFailed(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null; // Clear active command
            UpdateDisplay();
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' failed - keyboard shortcuts re-enabled");
        }

        #endregion

        #region Prompt Events

        private void OnPromptingForPoint(object sender, PromptPointOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForDistance(object sender, PromptDistanceOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForAngle(object sender, PromptAngleOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        private void OnPromptingForString(object sender, PromptStringOptionsEventArgs e)
        {
            if (e.Options.Keywords.Count > 0)
            {
                DisplayKeywords(e.Options.Keywords);
            }
            else
            {
                DisplayCommandValues();
            }
        }

        private void OnPromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            DisplayKeywords(e.Options.Keywords);
        }

        private void OnPromptingForCorner(object sender, PromptPointOptionsEventArgs e)
        {
            DisplayCommandValues();
        }

        #endregion

        #region Helper Methods

        // Embedded proven check_and_add_command logic from original implementation
        private void CheckAndAddCommand(string commandName)
        {
            lock (_lockObject)
            {
                if (_commandDataList == null)
                {
                    _commandDataList = new List<CommandData>();
                }

                // Use the same logic as the original check_and_add_command
                var existingCommand = _commandDataList.FirstOrDefault(c => c.CommandName == commandName);
                
                if (existingCommand == null)
                {
                    // Command not in list - add new command
                    _commandDataList.Add(new CommandData(commandName));
                }
                else
                {
                    // Command already in list - increment counter
                    existingCommand.IncrementUsage();
                }
            }
        }

        private CommandData GetOrCreateCommand(string commandName)
        {
            lock (_lockObject)
            {
                if (_commandDataList == null)
                {
                    _commandDataList = new List<CommandData>();
                }

                var command = _commandDataList.FirstOrDefault(c => c.CommandName == commandName);
                if (command == null)
                {
                    command = new CommandData(commandName);
                    _commandDataList.Add(command);
                }
                return command;
            }
        }

        private void UpdateDisplay()
        {
            try
            {
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nUpdateDisplay called: window={_window != null}, commands={_commandDataList?.Count ?? 0}");
                
                if (_window != null && _commandDataList != null)
                {
                    // Always pass the current command data list
                    var currentDataList = new List<CommandData>(_commandDataList);
                    ed?.WriteMessage($"\nPassing {currentDataList.Count} commands to window. IsAtMainPrompt={_isAtMainPrompt}");
                    
                    if (_isAtMainPrompt)
                    {
                        _window.UpdateDisplayData(currentDataList, true);
                    }
                    else
                    {
                        var currentCommand = GetOrCreateCommand(_activeCommand);
                        _window.UpdateDisplayData(currentDataList, false, _activeCommand);
                    }
                }
                else
                {
                    ed?.WriteMessage($"\nSkipping display update: window null={_window == null}, commands null={_commandDataList == null}");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating display: {ex.Message}");
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nError updating display: {ex.Message}");
            }
        }

        private void DisplayCommandValues()
        {
            if (_window != null && !string.IsNullOrEmpty(_activeCommand) && _commandDataList != null)
            {
                var command = GetOrCreateCommand(_activeCommand);
                var currentDataList = new List<CommandData>(_commandDataList);
                _window.UpdateDisplayData(currentDataList, false, _activeCommand);
            }
        }

        private void DisplayKeywords(KeywordCollection keywords)
        {
            // Convert keywords to display format
            // This could be enhanced to show keyword descriptions
        }

        private void AddCommonInputValues(CommandData command, string commandName)
        {
            // Add common input values based on command type
            switch (commandName.ToUpper())
            {
                case "OFFSET":
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("10", InputType.Number);
                    command.AddInputValue("15", InputType.Number);
                    command.AddInputValue("20", InputType.Number);
                    command.AddInputValue("25", InputType.Number);
                    break;
                
                case "FILLET":
                case "CHAMFER":
                    command.AddInputValue("0", InputType.Number);
                    command.AddInputValue("2", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("10", InputType.Number);
                    break;
                
                case "SCALE":
                    command.AddInputValue("0.5", InputType.Number);
                    command.AddInputValue("1", InputType.Number);
                    command.AddInputValue("2", InputType.Number);
                    command.AddInputValue("1.5", InputType.Number);
                    break;
                
                case "ROTATE":
                    command.AddInputValue("90", InputType.Angle);
                    command.AddInputValue("180", InputType.Angle);
                    command.AddInputValue("270", InputType.Angle);
                    command.AddInputValue("45", InputType.Angle);
                    break;
                
                case "ARRAY":
                    command.AddInputValue("3", InputType.Number);
                    command.AddInputValue("4", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("6", InputType.Number);
                    break;
                
                case "CIRCLE":
                    command.AddInputValue("10", InputType.Number);
                    command.AddInputValue("25", InputType.Number);
                    command.AddInputValue("50", InputType.Number);
                    command.AddInputValue("100", InputType.Number);
                    break;
                
                case "TEXT":
                case "MTEXT":
                    command.AddInputValue("3.5", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("7", InputType.Number);
                    break;
                
                case "LAYER":
                    command.AddInputValue("0", InputType.Text);
                    command.AddInputValue("Dimensions", InputType.Text);
                    command.AddInputValue("Text", InputType.Text);
                    command.AddInputValue("Hatching", InputType.Text);
                    break;
                
                case "ZOOM":
                    command.AddInputValue("All", InputType.Text);
                    command.AddInputValue("Extents", InputType.Text);
                    command.AddInputValue("Window", InputType.Text);
                    command.AddInputValue("0.5x", InputType.Text);
                    command.AddInputValue("2x", InputType.Text);
                    break;
            }
        }

        private void LoadCommandData()
        {
            try
            {
                _commandDataList = _persistenceService?.LoadCommandData() ?? new List<CommandData>();
                
                // Add default commands if empty
                if (!_commandDataList.Any())
                {
                    foreach (var category in _startupCommands)
                    {
                        foreach (var cmd in category.Value)
                        {
                            GetOrCreateCommand(cmd);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading command data: {ex.Message}");
            }
        }

        private void SaveCommandData()
        {
            try
            {
                lock (_lockObject)
                {
                    _persistenceService?.SaveCommandData(_commandDataList);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving command data: {ex.Message}");
            }
        }

        private void AutoSave()
        {
            try
            {
                // Auto-save every 10 commands
                if (_commandDataList.Sum(c => c.UsageCount) % 10 == 0)
                {
                    SaveCommandData();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in auto-save: {ex.Message}");
            }
        }

        #endregion

        #region PreTranslateMessage Implementation

        private void OnPreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
        {
            try
            {
                if (e.Message.message == WM_KEYDOWN)
                {
                    if ((Control.ModifierKeys & Keys.Control) > 0)
                    {
                        Keys keyCode = (Keys)(int)e.Message.wParam & Keys.KeyCode;
                        
                        // Check for Ctrl+` (backtick/tilde key) to toggle override
                        if (keyCode == Keys.Oemtilde)
                        {
                            _keyboardOverrideEnabled = !_keyboardOverrideEnabled;
                            e.Handled = true;
                            
                            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                            ed?.WriteMessage($"\nKeyboard override {(_keyboardOverrideEnabled ? "ENABLED" : "DISABLED")}");
                            
                            // Update window color based on state
                            if (_window != null)
                            {
                                UpdateWindowColorForOverrideState();
                            }
                            return;
                        }
                        
                        // Only intercept if we're at main prompt and override is enabled
                        if (_isAtMainPrompt && _keyboardOverrideEnabled)
                        {
                            char keyChar = (char)keyCode;
                            if (_shortcutKeys.Contains(keyChar))
                            {
                                int shortcutIndex = _shortcutKeys.FindIndex(k => k == keyChar);
                                SendCommand(shortcutIndex);
                                e.Handled = true; // Message handled - prevents AutoCAD from processing it
                            }
                        }
                        else if (_shortcutKeys.Contains((char)keyCode))
                        {
                            // Debug: Show why shortcut was not handled
                            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                            if (!_keyboardOverrideEnabled)
                            {
                                ed?.WriteMessage($"\nKeyboard override is DISABLED - Ctrl+{(char)keyCode} passed to AutoCAD");
                            }
                            else if (!_isAtMainPrompt)
                            {
                                ed?.WriteMessage($"\nNot at main prompt - Ctrl+{(char)keyCode} passed to AutoCAD");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPreTranslateMessage: {ex.Message}");
            }
        }

        private void SendCommand(int shortcutIndex)
        {
            try
            {
                if (_commandDataList != null)
                {
                    // Get the sorted list (by usage count descending)
                    var sortedCommands = _commandDataList.OrderByDescending(c => c.UsageCount).ToList();
                    
                    if (shortcutIndex < sortedCommands.Count)
                    {
                        var command = sortedCommands[shortcutIndex];
                        var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                        if (doc != null)
                        {
                            // Use alias if available, otherwise use full command name
                            string cmdToSend = CommandAliasService.GetAlias(command.CommandName);
                            doc.SendStringToExecute(cmdToSend + " ", true, false, false);
                            
                            var ed = doc.Editor;
                            ed?.WriteMessage($"\nShortcut Ctrl+{_shortcutKeys[shortcutIndex]} executed: {cmdToSend}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendCommand: {ex.Message}");
            }
        }

        private void UpdateWindowColorForOverrideState()
        {
            if (_window == null) return;
            
            // Clean up any previous paint handlers
            _window.Paint -= OnWindowPaintRedBorder;
            
            if (_keyboardOverrideEnabled)
            {
                // Normal state - no special painting needed
                _window.Invalidate(); // Force repaint to remove any existing border
            }
            else
            {
                // Disabled state - add paint handler for red border
                _window.Paint += OnWindowPaintRedBorder;
                _window.Invalidate(); // Force repaint to show border
            }
        }
        
        private void OnWindowPaintRedBorder(object sender, PaintEventArgs e)
        {
            // Draw thick red border around the entire form
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 4))
            {
                var rect = e.ClipRectangle;
                e.Graphics.DrawRectangle(pen, 2, 2, _window.ClientSize.Width - 4, _window.ClientSize.Height - 4);
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}