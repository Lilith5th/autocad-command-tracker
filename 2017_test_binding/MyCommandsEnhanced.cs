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
using _2017_test_binding.Models;
using _2017_test_binding.Services;
using _2017_test_binding.ViewModels;
using _2017_test_binding.Views;

namespace _2017_test_binding
{
    public class MyCommandsEnhanced : IExtensionApplication, INotifyPropertyChanged
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
        private DataPersistenceService _persistenceService;
        private CommandAnalyticsService _analyticsService;
        private List<CommandData> _commandDataList;
        private string _activeCommand;
        private string _previousCommand;
        private bool _isAtMainPrompt = true;
        private readonly object _lockObject = new object();
        private readonly List<string> _currentCommandInputs = new List<string>();
        private bool _captureNextInput = false;

        // Keyboard override system (from WinForms implementation)
        private const int WM_KEYDOWN = 256;
        private const int WM_KEYUP = 257;
        private const int WM_CHAR = 258;
        private const int VK_RETURN = 13;
        private const int VK_SPACE = 32;
        private const int VK_ESCAPE = 27;
        // Use 3x3 grid layout: Q-W-E, A-S-D, Z-X-C (avoiding Y which swaps with Z on QWERTZ)
        private readonly List<char> _shortcutKeys = new List<char>()
            { 'Q', 'W', 'E', 'A', 'S', 'D', 'Z', 'X', 'C' };
        private string _currentInputBuffer = "";
        private List<string> _dimensionalInputs = new List<string>();
        
        // Window state synchronization
        private System.Timers.Timer _windowStateTimer;
        private IntPtr _acadWindowHandle;
        private WindowState _lastWindowState = WindowState.Normal;
        private bool _windowStateSyncEnabled = true;
        
        // Keyboard override toggle
        private bool _keyboardOverrideEnabled = true;
        private bool _preTranslateMessageRegistered = false;

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

                // Register command
                RegisterCommand();
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

                // Stop window state monitoring
                StopWindowStateMonitoring();

                // Cleanup keyboard shortcuts
                if (_preTranslateMessageRegistered)
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage -= OnPreTranslateMessage;
                    _preTranslateMessageRegistered = false;
                }
                
                // Cleanup
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

                // Register event handlers
                RegisterEventHandlers(doc);

                // Enable keyboard shortcuts (from WinForms implementation)
                if (!_preTranslateMessageRegistered)
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage += OnPreTranslateMessage;
                    _preTranslateMessageRegistered = true;
                }

                // Start window state synchronization
                StartWindowStateMonitoring();

                // Update display
                UpdateDisplay();

                ed.WriteMessage("\nCommand tracking started. Window is now active with keyboard shortcuts enabled and synchronized with AutoCAD window state.");
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
                
                // Disable keyboard shortcuts
                if (_preTranslateMessageRegistered)
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.PreTranslateMessage -= OnPreTranslateMessage;
                    _preTranslateMessageRegistered = false;
                }
                
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

        private void RegisterEventHandlers(Autodesk.AutoCAD.ApplicationServices.Document doc)
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
                ed.PromptingForEntity += OnPromptingForEntity;
                ed.PromptingForNestedEntity += OnPromptingForNestedEntity;
                ed.PromptingForSelection += OnPromptingForSelection;

                // Monitor for input echo (for capturing user typed values)
                ed.PointMonitor += OnPointMonitor;
                
                // Use custom input monitoring system
                StartInputCapture();
                
                // Note: PreTranslateMessage is already registered in StartCommandTracking
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
                    ed.PromptingForEntity -= OnPromptingForEntity;
                    ed.PromptingForNestedEntity -= OnPromptingForNestedEntity;
                    ed.PromptingForSelection -= OnPromptingForSelection;
                    ed.PointMonitor -= OnPointMonitor;
                }
                
                // Note: PreTranslateMessage is unregistered in StopCommandTracking and Terminate
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
                    _currentCommandInputs.Clear(); // Clear previous inputs
                    _currentInputBuffer = ""; // Clear input buffer
                    _dimensionalInputs.Clear(); // Clear dimensional inputs

                    var command = GetOrCreateCommand(_activeCommand);
                    command.IncrementUsage();

                    // Track command sequence
                    if (!string.IsNullOrEmpty(_previousCommand))
                    {
                        var prevCmd = GetOrCreateCommand(_previousCommand);
                        prevCmd.AddFollowedCommand(_activeCommand);
                    }

                    if (_analyticsService != null)
                    {
                        _analyticsService.TrackCommand(_activeCommand, _commandDataList);
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

        private void OnCommandEnded(object sender, CommandEventArgs e)
        {
            try
            {
                _isAtMainPrompt = true;
                _previousCommand = _activeCommand;
                _activeCommand = null;
                _currentInputBuffer = ""; // Clear input buffer
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

        private void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            _currentInputBuffer = ""; // Clear input buffer
            _dimensionalInputs.Clear(); // Clear dimensional inputs
            UpdateDisplay();
            
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nCommand '{e.GlobalCommandName}' cancelled - keyboard shortcuts re-enabled");
        }

        private void OnCommandFailed(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            _currentInputBuffer = ""; // Clear input buffer
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

        private CommandData GetOrCreateCommand(string commandName)
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

        private void InitializeDefaultCommands()
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

        private void LoadCommandData()
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

        private void OnPreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
        {
            try
            {
                if (e.Message.message == WM_KEYDOWN)
                {
                    if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) > 0)
                    {
                        System.Windows.Forms.Keys keyCode = (System.Windows.Forms.Keys)(int)e.Message.wParam & System.Windows.Forms.Keys.KeyCode;
                        
                        // Check for Ctrl+` (backtick/tilde key) to toggle override
                        if (keyCode == System.Windows.Forms.Keys.Oemtilde)
                        {
                            _keyboardOverrideEnabled = !_keyboardOverrideEnabled;
                            e.Handled = true;
                            
                            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                            ed?.WriteMessage($"\nKeyboard override {(_keyboardOverrideEnabled ? "ENABLED" : "DISABLED")}");
                            
                            // Update window color and title based on state
                            if (_window != null)
                            {
                                _window.Dispatcher.Invoke(() => {
                                    UpdateWindowColorForOverrideState();
                                    
                                    // Update window title to show state
                                    if (_viewModel != null)
                                    {
                                        _viewModel.WindowTitle = _keyboardOverrideEnabled 
                                            ? "Commands" 
                                            : "Commands (Override OFF)";
                                    }
                                });
                            }
                            return;
                        }
                        
                        // Check if this is one of our shortcut keys
                        char keyChar = (char)keyCode;
                        if (_shortcutKeys.Contains(keyChar))
                        {
                            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                            
                            // Debug output
                            ed?.WriteMessage($"\nDebug: Ctrl+{keyChar} pressed. Override={_keyboardOverrideEnabled}, AtMainPrompt={_isAtMainPrompt}");
                            
                            // Handle keyboard shortcuts only when at main prompt and override is enabled
                            if (_isAtMainPrompt && _keyboardOverrideEnabled)
                            {
                                int shortcutIndex = _shortcutKeys.FindIndex(k => k == keyChar);
                                SendCommand(shortcutIndex);
                                e.Handled = true; // Message handled - prevents AutoCAD from processing it
                                ed?.WriteMessage($" - HANDLED by plugin");
                            }
                            else
                            {
                                // Let AutoCAD handle it
                                if (!_keyboardOverrideEnabled)
                                {
                                    ed?.WriteMessage($" - PASSED to AutoCAD (override disabled)");
                                }
                                else if (!_isAtMainPrompt)
                                {
                                    ed?.WriteMessage($" - PASSED to AutoCAD (not at main prompt)");
                                }
                            }
                        }
                    }
                }
                
                // Capture keyboard input when command is active (not at main prompt)
                if (!_isAtMainPrompt && !string.IsNullOrEmpty(_activeCommand))
                {
                    if (e.Message.message == WM_CHAR)
                    {
                        char ch = (char)e.Message.wParam;
                        // Allow digits, decimal point, minus, plus, and common dimensional characters
                        if (char.IsDigit(ch) || ch == '.' || ch == '-' || ch == '+' || 
                            ch == '/' || ch == '\\' || ch == ',' || ch == '@' || 
                            char.IsLetter(ch)) // Allow letters for variables like 'x', 'y'
                        {
                            _currentInputBuffer += ch;
                        }
                        else if (ch == '\b' && _currentInputBuffer.Length > 0) // Backspace
                        {
                            _currentInputBuffer = _currentInputBuffer.Substring(0, _currentInputBuffer.Length - 1);
                        }
                    }
                    else if (e.Message.message == WM_KEYDOWN)
                    {
                        int keyCode = (int)e.Message.wParam;
                        
                        if (keyCode == VK_RETURN || keyCode == VK_SPACE)
                        {
                            // Enter or Space key pressed - capture the current input buffer
                            if (!string.IsNullOrEmpty(_currentInputBuffer.Trim()))
                            {
                                var inputValue = _currentInputBuffer.Trim();
                                // Parse the input - could be number, dimension (e.g. "10,20"), or variable
                                ParseAndCaptureInput(inputValue);
                                _currentInputBuffer = "";
                                
                                // Show debug message
                                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                                ed?.WriteMessage($"\n[Input captured: {inputValue}]");
                            }
                        }
                        else if (keyCode == VK_ESCAPE)
                        {
                            // Escape key pressed - clear the input buffer and any pending inputs
                            _currentInputBuffer = "";
                            _dimensionalInputs.Clear();
                            
                            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                            ed?.WriteMessage($"\n[Input cleared]");
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
                // Use the view model's ExecuteCommandAtIndex which properly handles the sorted/filtered list
                if (_viewModel != null)
                {
                    _viewModel.ExecuteCommandAtIndex(shortcutIndex);
                    
                    var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                    if (shortcutIndex < _shortcutKeys.Count)
                    {
                        ed?.WriteMessage($"\nShortcut Ctrl+{_shortcutKeys[shortcutIndex]} executed");
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}