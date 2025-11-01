using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AutoCadCommandTracker.Models;
using AutoCadCommandTracker.Services;
using AutoCadCommandTracker.ViewModels;
using AutoCadCommandTracker.Views;

namespace AutoCadCommandTracker
{
    public class WpfCommandTracker : CommandTrackerBase
    {
        #region Windows API
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        private const int SW_HIDE = 0;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        
        #endregion
        private MainWindowEnhanced _window;
        private MainViewModelEnhanced _viewModel;
        private readonly List<string> _currentCommandInputs = new List<string>();

        // WPF-specific constants
        private const int WM_KEYUP = 257;
        private const int WM_CHAR = 258;
        private const int VK_RETURN = 13;
        private const int VK_SPACE = 32;
        private const int VK_ESCAPE = 27;
        private List<string> _dimensionalInputs = new List<string>();
        
        // Window state synchronization
        private System.Timers.Timer _windowStateTimer;
        private IntPtr _acadWindowHandle;
        private WindowState _lastWindowState = WindowState.Normal;
        private bool _windowStateSyncEnabled = true;
        

        #region IExtensionApplication

        public override void Initialize()
        {
            try
            {
                // Call base initialization
                base.Initialize();

                // Ensure shortcuts are loaded
                if (_shortcutKeys == null || _shortcutKeys.Count == 0)
                {
                    var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                    ed?.WriteMessage("\nWARNING: Shortcuts not loaded, using defaults");
                    _shortcutKeys = UserSettings.GetDefaultShortcutKeys();
                }

                var ed2 = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed2?.WriteMessage($"\nWPF Tracker initialized with shortcuts: [{string.Join(",", _shortcutKeys)}]");

                // Register command
                RegisterCommand();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during WPF initialization: {ex.Message}");
            }
        }

        public override void Terminate()
        {
            try
            {
                // Stop window state monitoring
                StopWindowStateMonitoring();
                
                if (_window != null)
                {
                    _window.Close();
                    _window = null;
                }
                
                // Call base termination
                base.Terminate();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during termination: {ex.Message}");
            }
        }

        #endregion

        #region Abstract Method Implementations

        protected override void OnDisplayUpdateNeeded()
        {
            try
            {
                // Update the WPF view model with current data
                _viewModel?.UpdateDisplayData(_commandDataList, _isAtMainPrompt);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating WPF display: {ex.Message}");
            }
        }

        protected override void OnOverrideStateChanged()
        {
            try
            {
                // Update window border color to indicate override state
                if (_window != null)
                {
                    _window.Dispatcher.Invoke(() =>
                    {
                        // Update the visual border color
                        UpdateWindowColorForOverrideState();
                        
                        // Update the view model status text
                        _viewModel?.OnOverrideStateChanged(_keyboardOverrideEnabled);
                    });
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating WPF override state: {ex.Message}");
            }
        }

        #endregion

        [CommandMethod("CMDTRACK", CommandFlags.Session)]
        public void StartCommandTracking()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var ed = doc.Editor;

                // Initialize view model
                _viewModel = new MainViewModelEnhanced();
                
                // Initialize state
                _isAtMainPrompt = true;
                _keyboardOverrideEnabled = true;
                
                // Initialize default commands if list is empty
                InitializeDefaultCommands();

                // Create and show window
                if (_window == null)
                {
                    _window = new MainWindowEnhanced(_viewModel);
                    _window.Show();
                    UpdateWindowColorForOverrideState(); // Set initial color state
                    Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
                }

                // Register event handlers (this will NOT register PreTranslateMessage since we override RegisterEventHandlers)
                RegisterEventHandlers(doc);

                // Start window state synchronization
                StartWindowStateMonitoring();

                // Update display
                UpdateDisplay();

                ed.WriteMessage("\nWPF command tracking started. Window is now active with keyboard shortcuts enabled.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting command tracking: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACK_STOP", CommandFlags.Session)]
        public void StopCommandTracking()
        {
            try
            {
                // Stop window state monitoring
                StopWindowStateMonitoring();
                
                // Unregister event handlers (this will call CleanupKeyboardShortcuts)
                UnregisterEventHandlers();
                
                if (_window != null)
                {
                    _window.Close();
                    _window = null;
                }

                SaveCommandData();

                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage("\nCommand tracking stopped, keyboard shortcuts disabled, and window synchronization stopped.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping command tracking: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACK_ADDVALUE", CommandFlags.Session)]
        public void AddValueToCurrentCommand()
        {
            try
            {
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                if (ed == null) return;

                if (string.IsNullOrEmpty(_activeCommand))
                {
                    ed.WriteMessage("\nNo active command to add value to.");
                    return;
                }

                var prompt = new PromptStringOptions($"\nEnter value to remember for {_activeCommand}: ");
                prompt.AllowSpaces = true;
                var result = ed.GetString(prompt);

                if (result.Status == PromptStatus.OK && !string.IsNullOrEmpty(result.StringResult))
                {
                    // Determine if it's a number or text
                    var inputType = double.TryParse(result.StringResult, out _) ? InputType.Number : InputType.Text;
                    CaptureConfirmedInput(result.StringResult, inputType);
                    
                    ed.WriteMessage($"\nAdded '{result.StringResult}' to {_activeCommand} values.");
                    UpdateDisplay();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding value: {ex.Message}");
            }
        }

        [CommandMethod("CMDTRACK_WINDOWSYNC", CommandFlags.Session)]
        public void ToggleWindowSync()
        {
            try
            {
                _windowStateSyncEnabled = !_windowStateSyncEnabled;
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nWindow synchronization with AutoCAD: {(_windowStateSyncEnabled ? "ENABLED" : "DISABLED")}");
                
                if (_windowStateSyncEnabled)
                {
                    StartWindowStateMonitoring();
                }
                else
                {
                    StopWindowStateMonitoring();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling window sync: {ex.Message}");
            }
        }

        #region Event Handlers

        protected override void RegisterEventHandlers(Autodesk.AutoCAD.ApplicationServices.Document doc)
        {
            try
            {
                // Register only the base command events (NOT keyboard shortcuts)
                doc.CommandWillStart += OnCommandWillStart;
                doc.CommandEnded += OnCommandEnded;
                doc.CommandCancelled += OnCommandCancelled;
                doc.CommandFailed += OnCommandFailed;
                
                // Add WPF-specific events
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
                ed.PromptingForEntity += OnPromptingForEntity;
                ed.PromptingForNestedEntity += OnPromptingForNestedEntity;
                ed.PromptingForSelection += OnPromptingForSelection;

                // Monitor for input echo (for capturing user typed values)
                ed.PointMonitor += OnPointMonitor;
                
                // Use custom input monitoring system
                StartInputCapture();
                
                // Register keyboard shortcuts (PreTranslateMessage)
                RegisterKeyboardShortcuts();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering event handlers: {ex.Message}");
            }
        }

        protected override void UnregisterEventHandlers()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    // Unregister command events
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
                    ed.PromptingForEntity -= OnPromptingForEntity;
                    ed.PromptingForNestedEntity -= OnPromptingForNestedEntity;
                    ed.PromptingForSelection -= OnPromptingForSelection;
                    ed.PointMonitor -= OnPointMonitor;
                }
                
                // Cleanup keyboard shortcuts
                CleanupKeyboardShortcuts();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unregistering event handlers: {ex.Message}");
            }
        }

        #endregion

        #region Command Events

        private new void OnCommandWillStart(object sender, CommandEventArgs e)
        {
            try
            {
                lock (_lockObject)
                {
                    _previousCommand = _activeCommand;
                    _activeCommand = e.GlobalCommandName;
                    _isAtMainPrompt = false;
                    _currentCommandInputs.Clear(); // Clear previous inputs
                    // Input buffer cleared (field removed)
                    _dimensionalInputs.Clear(); // Clear dimensional inputs

                    var command = GetOrCreateCommand(_activeCommand);
                    command.IncrementUsage();

                    // Track command sequence
                    if (!string.IsNullOrEmpty(_previousCommand))
                    {
                        var prevCmd = GetOrCreateCommand(_previousCommand);
                        prevCmd.AddFollowedCommand(_activeCommand);
                    }

                    
                    // Add common input values for command
                    AddCommonInputValues(command, _activeCommand);
                }

                UpdateDisplay();
                
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nCommand '{_activeCommand}' started - keyboard shortcuts disabled");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnCommandWillStart: {ex.Message}");
            }
        }

        private new void OnCommandEnded(object sender, CommandEventArgs e)
        {
            try
            {
                _isAtMainPrompt = true;
                _previousCommand = _activeCommand;
                _activeCommand = null;
                // Input buffer cleared (field removed)
                _dimensionalInputs.Clear(); // Clear dimensional inputs
                UpdateDisplay();
                
                // Auto-save periodically
                Task.Run(() => AutoSave());
                
                // Process any collected inputs for the completed command
                ProcessCollectedInputs();
                
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' ended - keyboard shortcuts re-enabled");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnCommandEnded: {ex.Message}");
            }
        }

        private new void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            // Input buffer cleared (field removed)
            _dimensionalInputs.Clear(); // Clear dimensional inputs
            UpdateDisplay();
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' cancelled - keyboard shortcuts re-enabled");
        }

        private new void OnCommandFailed(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            // Input buffer cleared (field removed)
            _dimensionalInputs.Clear(); // Clear dimensional inputs
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
            // Only display existing values - don't add new ones during prompt
        }

        private void OnPromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            DisplayCommandValues();
            // Only display existing values - don't add new ones during prompt
        }

        private void OnPromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            DisplayCommandValues();
            // Only display existing values - don't add new ones during prompt
        }

        private void OnPromptingForAngle(object sender, PromptAngleOptionsEventArgs e)
        {
            DisplayCommandValues();
            // Only display existing values - don't add new ones during prompt
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
                // Only display existing values - don't add new ones during prompt
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

        private void OnPromptingForEntity(object sender, PromptEntityOptionsEventArgs e)
        {
            // Could show entity type filters or selection history
        }

        private void OnPromptingForNestedEntity(object sender, PromptNestedEntityOptionsEventArgs e)
        {
            // Could show nested entity selection history
        }

        private void OnPromptingForSelection(object sender, PromptSelectionOptionsEventArgs e)
        {
            // Could show selection set history
        }

        private void OnPointMonitor(object sender, PointMonitorEventArgs e)
        {
            // Don't capture preview values - only show existing values during prompts
            // Actual confirmed values will be captured through prompt result handlers
        }

        #endregion

        #region Input Capture System

        private void StartInputCapture()
        {
            // Initialize input capture system
            // We'll capture confirmed inputs through command completion analysis
        }

        private void CaptureConfirmedInput(string input, InputType type)
        {
            if (!string.IsNullOrEmpty(_activeCommand) && !string.IsNullOrEmpty(input))
            {
                var command = GetOrCreateCommand(_activeCommand);
                command.AddInputValue(input, type);
                
                // Also add to current session tracking
                _currentCommandInputs.Add(input);
                
                System.Diagnostics.Debug.WriteLine($"Captured confirmed input for {_activeCommand}: {input} ({type})");
            }
        }

        // Method to manually add confirmed values (can be called by custom prompts)
        public void AddConfirmedValue(string value, InputType type = InputType.Number)
        {
            CaptureConfirmedInput(value, type);
        }

        private void ParseAndCaptureInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            var command = GetOrCreateCommand(_activeCommand);
            
            // Check for different input patterns
            if (input.Contains(","))
            {
                // Coordinate or dimension input (e.g., "10,20" or "100,50,0")
                var parts = input.Split(',');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        command.AddInputValue(trimmed, InputType.Number);
                        _dimensionalInputs.Add(trimmed);
                    }
                }
                
                // Also store the complete coordinate as a single value
                command.AddInputValue(input, InputType.Text);
            }
            else if (input.Contains("@"))
            {
                // Relative coordinate (e.g., "@10,20" or "@50<45")
                command.AddInputValue(input, InputType.Text);
                
                // Also extract numeric values
                var numericPart = input.Replace("@", "").Replace("<", ",");
                if (numericPart.Contains(","))
                {
                    var parts = numericPart.Split(',');
                    foreach (var part in parts)
                    {
                        if (double.TryParse(part.Trim(), out _))
                        {
                            command.AddInputValue(part.Trim(), InputType.Number);
                        }
                    }
                }
            }
            else if (input.Contains("/") || input.Contains("\\"))
            {
                // Fraction input (e.g., "1/2", "3/4")
                command.AddInputValue(input, InputType.Text);
                
                // Try to evaluate the fraction
                try
                {
                    var parts = input.Split('/', '\\');
                    if (parts.Length == 2 && 
                        double.TryParse(parts[0], out double numerator) && 
                        double.TryParse(parts[1], out double denominator) && 
                        denominator != 0)
                    {
                        var result = numerator / denominator;
                        command.AddInputValue(result.ToString("F4"), InputType.Number);
                    }
                }
                catch { }
            }
            else if (double.TryParse(input, out double numValue))
            {
                // Simple numeric input
                command.AddInputValue(numValue.ToString(), InputType.Number);
                _dimensionalInputs.Add(numValue.ToString());
            }
            else
            {
                // Text input (could be a variable name like 'x', 'y', or a keyword)
                command.AddInputValue(input, InputType.Text);
            }
            
            // Save after each confirmed input
            SaveCommandData();
        }

        #endregion

        #region Window State Synchronization

        private void StartWindowStateMonitoring()
        {
            try
            {
                // Get AutoCAD window handle
                _acadWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
                
                // Start timer to monitor window state changes
                _windowStateTimer = new System.Timers.Timer(500); // Check every 500ms
                _windowStateTimer.Elapsed += OnWindowStateTimerElapsed;
                _windowStateTimer.Start();
                
                System.Diagnostics.Debug.WriteLine("Window state monitoring started");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting window state monitoring: {ex.Message}");
            }
        }

        private void StopWindowStateMonitoring()
        {
            try
            {
                if (_windowStateTimer != null)
                {
                    _windowStateTimer.Stop();
                    _windowStateTimer.Dispose();
                    _windowStateTimer = null;
                }
                
                System.Diagnostics.Debug.WriteLine("Window state monitoring stopped");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping window state monitoring: {ex.Message}");
            }
        }

        private void OnWindowStateTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_windowStateSyncEnabled || _window == null || _acadWindowHandle == IntPtr.Zero)
                return;

            try
            {
                var currentState = GetAutoCadWindowState();
                if (currentState != _lastWindowState)
                {
                    _lastWindowState = currentState;
                    
                    // Update our window state on UI thread
                    _window.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SynchronizeWindowState(currentState);
                    }));
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in window state monitoring: {ex.Message}");
            }
        }

        private WindowState GetAutoCadWindowState()
        {
            if (_acadWindowHandle == IntPtr.Zero)
                return WindowState.Normal;

            if (IsIconic(_acadWindowHandle))
                return WindowState.Minimized;
            else if (IsZoomed(_acadWindowHandle))
                return WindowState.Maximized;
            else if (IsWindowVisible(_acadWindowHandle))
                return WindowState.Normal;
            else
                return WindowState.Minimized; // Hidden = minimized for our purposes
        }

        private void SynchronizeWindowState(WindowState autocadState)
        {
            if (_window == null) return;

            try
            {
                switch (autocadState)
                {
                    case WindowState.Minimized:
                        if (_window.WindowState != WindowState.Minimized)
                        {
                            _window.WindowState = WindowState.Minimized;
                            System.Diagnostics.Debug.WriteLine("Synchronized: Window minimized");
                        }
                        break;
                        
                    case WindowState.Maximized:
                        if (_window.WindowState == WindowState.Minimized)
                        {
                            _window.WindowState = WindowState.Normal;
                            System.Diagnostics.Debug.WriteLine("Synchronized: Window restored from minimized");
                        }
                        break;
                        
                    case WindowState.Normal:
                        if (_window.WindowState == WindowState.Minimized)
                        {
                            _window.WindowState = WindowState.Normal;
                            System.Diagnostics.Debug.WriteLine("Synchronized: Window restored");
                        }
                        break;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error synchronizing window state: {ex.Message}");
            }
        }

        public void EnableWindowStateSync(bool enable)
        {
            _windowStateSyncEnabled = enable;
        }

        #endregion

        #region Helper Methods

        private new CommandData GetOrCreateCommand(string commandName)
        {
            lock (_lockObject)
            {
                // Ensure _commandDataList is initialized
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
                if (_viewModel != null && _window != null)
                {
                    _window.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_isAtMainPrompt)
                        {
                            _viewModel.UpdateDisplayData(_commandDataList, true);
                        }
                        else
                        {
                            var currentCommand = GetOrCreateCommand(_activeCommand);
                            _viewModel.UpdateDisplayDataWithValues(currentCommand.InputValues);
                        }
                    }));
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating display: {ex.Message}");
            }
        }

        private void DisplayCommandValues()
        {
            if (_viewModel != null && _window != null && !string.IsNullOrEmpty(_activeCommand))
            {
                var command = GetOrCreateCommand(_activeCommand);
                _window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _viewModel.UpdateDisplayDataWithValues(command.InputValues);
                }));
            }
        }

        private void DisplayKeywords(KeywordCollection keywords)
        {
            // Convert keywords to display format
            // This could be enhanced to show keyword descriptions
        }

        private new void InitializeDefaultCommands()
        {
            if (_commandDataList == null)
            {
                _commandDataList = new List<CommandData>();
            }

            if (_commandDataList.Count == 0)
            {
                foreach (var category in _startupCommands)
                {
                    foreach (var command in category.Value)
                    {
                        _commandDataList.Add(new CommandData(command) { Category = category.Key });
                    }
                }
            }
        }

        private void AddCommonInputValues(CommandData command, string commandName)
        {
            // Add common input values based on command type (from WinForms implementation)
            switch (commandName.ToUpper())
            {
                case "OFFSET":
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("10", InputType.Number);
                    command.AddInputValue("15", InputType.Number);
                    command.AddInputValue("20", InputType.Number);
                    break;
                    
                case "SCALE":
                    command.AddInputValue("2", InputType.Number);
                    command.AddInputValue("0.5", InputType.Number);
                    command.AddInputValue("1.5", InputType.Number);
                    break;
                    
                case "FILLET":
                case "CHAMFER":
                    command.AddInputValue("5", InputType.Number);
                    command.AddInputValue("10", InputType.Number);
                    command.AddInputValue("2", InputType.Number);
                    break;
                    
                case "TEXT":
                case "MTEXT":
                    command.AddInputValue("Standard", InputType.Text);
                    command.AddInputValue("Arial", InputType.Text);
                    command.AddInputValue("2.5", InputType.Number);
                    command.AddInputValue("3.5", InputType.Number);
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

        private void AddNumericInputValues(CommandData command, string[] values)
        {
            foreach (var value in values)
            {
                command.AddInputValue(value, InputType.Number);
            }
        }

        private void AddCommandSpecificStringValues(CommandData command, string commandName)
        {
            switch (commandName.ToUpper())
            {
                case "LAYER":
                    command.AddInputValue("0", InputType.Text);
                    command.AddInputValue("Defpoints", InputType.Text);
                    command.AddInputValue("Text", InputType.Text);
                    command.AddInputValue("Dimensions", InputType.Text);
                    break;
                    
                case "TEXT":
                case "MTEXT":
                    command.AddInputValue("Standard", InputType.Text);
                    command.AddInputValue("Arial", InputType.Text);
                    command.AddInputValue("Times New Roman", InputType.Text);
                    command.AddInputValue("2.5", InputType.Number);
                    command.AddInputValue("3.5", InputType.Number);
                    command.AddInputValue("5", InputType.Number);
                    break;
                    
                case "LINETYPE":
                    command.AddInputValue("Continuous", InputType.Text);
                    command.AddInputValue("DASHED", InputType.Text);
                    command.AddInputValue("HIDDEN", InputType.Text);
                    command.AddInputValue("CENTER", InputType.Text);
                    break;
                    
                case "COLOR":
                    command.AddInputValue("BYLAYER", InputType.Text);
                    command.AddInputValue("BYBLOCK", InputType.Text);
                    command.AddInputValue("RED", InputType.Text);
                    command.AddInputValue("BLUE", InputType.Text);
                    command.AddInputValue("GREEN", InputType.Text);
                    break;
            }
        }

        private void ProcessCollectedInputs()
        {
            // Process any inputs collected during command execution
            if (!string.IsNullOrEmpty(_previousCommand) && _currentCommandInputs.Count > 0)
            {
                var command = GetOrCreateCommand(_previousCommand);
                foreach (var input in _currentCommandInputs)
                {
                    // Try to determine if it's a number
                    if (double.TryParse(input, out _))
                    {
                        command.AddInputValue(input, InputType.Number);
                    }
                    else
                    {
                        command.AddInputValue(input, InputType.Text);
                    }
                }
                _currentCommandInputs.Clear();
                SaveCommandData();
            }
        }

        private new void LoadCommandData()
        {
            try
            {
                _commandDataList = _persistenceService.LoadCommandData() ?? new List<CommandData>();
                
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

        private new void SaveCommandData()
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

        private new void AutoSave()
        {
            try
            {
                // Auto-save every 10 commands
                if (_commandDataList != null && _commandDataList.Sum(c => c.UsageCount) % 10 == 0)
                {
                    SaveCommandData();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in auto-save: {ex.Message}");
            }
        }

        private void RegisterCommand()
        {
            // Command registration is handled by CommandMethod attributes
        }

        #endregion

        #region PreTranslateMessage Implementation

        // NOTE: Removed old OnPreTranslateMessage - now using IMessageFilter.PreFilterMessage instead
        // This provides global message filtering that works regardless of window focus


        private void UpdateWindowColorForOverrideState()
        {
            if (_window == null) return;
            
            // Find the MainBorder element in the window
            var border = _window.FindName("MainBorder") as System.Windows.Controls.Border;
            if (border != null)
            {
                // Set border color based on override state
                if (_keyboardOverrideEnabled)
                {
                    // Normal state - use theme border
                    border.BorderBrush = null; // This will revert to style-defined brush
                    border.BorderThickness = new System.Windows.Thickness(1);
                }
                else
                {
                    // Disabled state - red border
                    border.BorderBrush = System.Windows.Media.Brushes.Red;
                    border.BorderThickness = new System.Windows.Thickness(3);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public new event PropertyChangedEventHandler PropertyChanged;

        protected new virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region PreTranslateMessage Implementation
        
        protected override void OnPreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
        {
            try
            {
                if (e.Message.message == WM_KEYDOWN)
                {
                    if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) > 0)
                    {
                        System.Windows.Forms.Keys keyCode = (System.Windows.Forms.Keys)(int)e.Message.wParam & System.Windows.Forms.Keys.KeyCode;
                        
                        // Filter out modifier keys themselves
                        if (keyCode == System.Windows.Forms.Keys.ControlKey || 
                            keyCode == System.Windows.Forms.Keys.LControlKey || 
                            keyCode == System.Windows.Forms.Keys.RControlKey ||
                            keyCode == System.Windows.Forms.Keys.Menu ||
                            keyCode == System.Windows.Forms.Keys.Alt ||
                            keyCode == System.Windows.Forms.Keys.Shift)
                        {
                            return; // Ignore modifier keys
                        }
                        
                        var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                        ed?.WriteMessage($"\nDEBUG: PreTranslateMessage - Ctrl+{keyCode} detected");
                        
                        // Handle Ctrl+` toggle
                        if (keyCode == System.Windows.Forms.Keys.Oemtilde)
                        {
                            _keyboardOverrideEnabled = !_keyboardOverrideEnabled;
                            ed?.WriteMessage($"\nKeyboard override {(_keyboardOverrideEnabled ? "ENABLED" : "DISABLED")}");
                            OnOverrideStateChanged();
                            e.Handled = true;
                            return;
                        }
                        
                        // Handle shortcut keys - ALWAYS block them when override is enabled
                        char keyChar = (char)keyCode;
                        
                        // Debug: Check if shortcuts are lost
                        if (_shortcutKeys == null)
                        {
                            ed?.WriteMessage($" - ERROR: _shortcutKeys is NULL! Reloading...");
                            base.LoadShortcutKeys(); // Explicitly call base class method
                        }
                        else if (_shortcutKeys.Count == 0)
                        {
                            ed?.WriteMessage($" - ERROR: _shortcutKeys is EMPTY! Reloading...");
                            base.LoadShortcutKeys(); // Explicitly call base class method
                        }
                        
                        ed?.WriteMessage($" - KeyChar: '{keyChar}', Shortcuts: [{string.Join(",", _shortcutKeys ?? new List<char>())}]");
                        
                        if (_shortcutKeys != null && _shortcutKeys.Contains(keyChar))
                        {
                            ed?.WriteMessage($" - Found in shortcuts!");
                            
                            if (_keyboardOverrideEnabled)
                            {
                                // ALWAYS block shortcut keys when override is enabled
                                ed?.WriteMessage($" -> Blocking shortcut key from AutoCAD");
                                
                                // Only execute our custom action if we're at main prompt
                                if (_isAtMainPrompt)
                                {
                                    int shortcutIndex = _shortcutKeys.FindIndex(k => k == keyChar);
                                    ed?.WriteMessage($" -> Executing shortcut {shortcutIndex + 1} for command at index {shortcutIndex}");
                                    
                                    // Execute the shortcut
                                    ExecuteShortcut(shortcutIndex);
                                    
                                    // Return focus to AutoCAD
                                    try
                                    {
                                        Autodesk.AutoCAD.ApplicationServices.Application.MainWindow.Focus();
                                    }
                                    catch { }
                                }
                                else
                                {
                                    ed?.WriteMessage($" -> Shortcut blocked but not executed (not at main prompt)");
                                }
                                
                                e.Handled = true; // ✅ ALWAYS BLOCK AUTOCAD WHEN OVERRIDE ENABLED
                                return;
                            }
                            else
                            {
                                ed?.WriteMessage($" -> Override disabled, letting AutoCAD handle shortcut");
                                // When override is disabled, let AutoCAD handle its normal shortcuts
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PreTranslateMessage: {ex.Message}");
            }
        }
        
        #endregion
    }
}