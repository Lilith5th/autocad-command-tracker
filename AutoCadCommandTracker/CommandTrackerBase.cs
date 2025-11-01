using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AutoCadCommandTracker.Models;
using AutoCadCommandTracker.Services;

namespace AutoCadCommandTracker
{
    /// <summary>
    /// Base class containing shared command tracking logic between WPF and WinForms implementations
    /// Eliminates ~800 lines of code duplication
    /// </summary>
    public abstract class CommandTrackerBase : IExtensionApplication, INotifyPropertyChanged
    {
        #region Shared Fields
        protected DataPersistenceService _persistenceService;
        protected List<CommandData> _commandDataList;
        protected string _activeCommand;
        protected string _previousCommand;
        protected bool _isAtMainPrompt = true;
        protected readonly object _lockObject = new object();
        
        // Keyboard override system
        protected const int WM_KEYDOWN = 256;
        protected List<char> _shortcutKeys;
        protected bool _keyboardOverrideEnabled = true;
        protected bool _preTranslateMessageRegistered = false;
        
        // Default commands
        protected readonly Dictionary<string, string[]> _startupCommands = new Dictionary<string, string[]>
        {
            ["Drawing"] = new[] { "PLINE", "LINE", "CIRCLE", "ARC", "ELLIPSE", "RECTANGLE" },
            ["Modify"] = new[] { "OFFSET", "TRIM", "EXTEND", "FILLET", "CHAMFER", "SCALE" },
            ["Annotation"] = new[] { "DIMENSION", "TEXT", "MTEXT", "LEADER", "TABLE" },
            ["Layers"] = new[] { "LAYER", "LAYCUR", "LAYISO", "LAYUNISO" },
            ["View"] = new[] { "ZOOM", "PAN", "REGEN", "REDRAW" }
        };
        #endregion

        #region IExtensionApplication
        public virtual void Initialize()
        {
            try
            {
                _persistenceService = new DataPersistenceService();
                _commandDataList = new List<CommandData>();
                LoadShortcutKeys();
                LoadCommandData();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during initialization: {ex.Message}");
            }
        }

        public virtual void Terminate()
        {
            try
            {
                SaveCommandData();
                CleanupKeyboardShortcuts();
                UnregisterEventHandlers();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during termination: {ex.Message}");
            }
        }
        #endregion

        #region Shared Command Management
        protected void CheckAndAddCommand(string commandName)
        {
            lock (_lockObject)
            {
                if (_commandDataList == null)
                    _commandDataList = new List<CommandData>();

                var existingCommand = _commandDataList.FirstOrDefault(c => c.CommandName == commandName);
                if (existingCommand == null)
                {
                    _commandDataList.Add(new CommandData(commandName));
                }
                else
                {
                    existingCommand.IncrementUsage();
                }
            }
        }

        protected CommandData GetOrCreateCommand(string commandName)
        {
            lock (_lockObject)
            {
                if (_commandDataList == null)
                    _commandDataList = new List<CommandData>();

                var command = _commandDataList.FirstOrDefault(c => c.CommandName == commandName);
                if (command == null)
                {
                    command = new CommandData(commandName);
                    _commandDataList.Add(command);
                }
                return command;
            }
        }

        protected void InitializeDefaultCommands()
        {
            if (_commandDataList == null || !_commandDataList.Any())
            {
                var allCommands = _startupCommands.Values.SelectMany(x => x).ToList();
                foreach (var cmd in allCommands)
                {
                    GetOrCreateCommand(cmd);
                }
            }
        }
        #endregion

        #region Shared Event Handlers
        protected virtual void RegisterEventHandlers(Document doc)
        {
            try
            {
                doc.CommandWillStart += OnCommandWillStart;
                doc.CommandEnded += OnCommandEnded;
                doc.CommandCancelled += OnCommandCancelled;
                doc.CommandFailed += OnCommandFailed;
                
                RegisterKeyboardShortcuts();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering event handlers: {ex.Message}");
            }
        }

        protected virtual void UnregisterEventHandlers()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.CommandWillStart -= OnCommandWillStart;
                    doc.CommandEnded -= OnCommandEnded;
                    doc.CommandCancelled -= OnCommandCancelled;
                    doc.CommandFailed -= OnCommandFailed;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unregistering event handlers: {ex.Message}");
            }
        }

        protected virtual void OnCommandWillStart(object sender, CommandEventArgs e)
        {
            try
            {
                lock (_lockObject)
                {
                    _previousCommand = _activeCommand;
                    _activeCommand = e.GlobalCommandName;
                    _isAtMainPrompt = false;
                    CheckAndAddCommand(_activeCommand);
                    
                    // Track command sequence
                    if (!string.IsNullOrEmpty(_previousCommand))
                    {
                        var prevCmd = GetOrCreateCommand(_previousCommand);
                        prevCmd.AddFollowedCommand(_activeCommand);
                    }
                }
                OnDisplayUpdateNeeded();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnCommandWillStart: {ex.Message}");
            }
        }

        protected virtual void OnCommandEnded(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            OnDisplayUpdateNeeded();
            AutoSave();
        }

        protected virtual void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            OnDisplayUpdateNeeded();
        }

        protected virtual void OnCommandFailed(object sender, CommandEventArgs e)
        {
            _isAtMainPrompt = true;
            _activeCommand = null;
            OnDisplayUpdateNeeded();
        }
        #endregion

        #region Shared Keyboard Shortcuts
        protected void RegisterKeyboardShortcuts()
        {
            if (!_preTranslateMessageRegistered)
            {
                Application.PreTranslateMessage += OnPreTranslateMessage;
                _preTranslateMessageRegistered = true;
            }
        }

        protected void CleanupKeyboardShortcuts()
        {
            if (_preTranslateMessageRegistered)
            {
                Application.PreTranslateMessage -= OnPreTranslateMessage;
                _preTranslateMessageRegistered = false;
            }
        }

        protected virtual void OnPreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
        {
            try
            {
                if (e.Message.message == WM_KEYDOWN)
                {
                    if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) > 0)
                    {
                        System.Windows.Forms.Keys keyCode = (System.Windows.Forms.Keys)(int)e.Message.wParam & System.Windows.Forms.Keys.KeyCode;
                        var editor = Application.DocumentManager.MdiActiveDocument?.Editor;
                        editor?.WriteMessage($"\nDEBUG: Ctrl+{keyCode} detected in OnPreTranslateMessage");
                        
                        // Toggle override with Ctrl+`
                        if (keyCode == System.Windows.Forms.Keys.Oemtilde)
                        {
                            _keyboardOverrideEnabled = !_keyboardOverrideEnabled;
                            e.Handled = true;
                            
                            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                            ed?.WriteMessage($"\nKeyboard override {(_keyboardOverrideEnabled ? "ENABLED" : "DISABLED")}");
                            
                            OnOverrideStateChanged();
                            return;
                        }
                        
                        // Handle shortcuts when enabled and at main prompt
                        if (_isAtMainPrompt && _keyboardOverrideEnabled)
                        {
                            char keyChar = (char)keyCode;
                            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                            
                            // Debug output
                            ed?.WriteMessage($"\nDEBUG: Key '{keyChar}' pressed. Available shortcuts: [{string.Join(",", _shortcutKeys ?? new List<char>())}]");
                            
                            if (_shortcutKeys != null && _shortcutKeys.Contains(keyChar))
                            {
                                int shortcutIndex = _shortcutKeys.FindIndex(k => k == keyChar);
                                ed?.WriteMessage($" -> Executing shortcut {shortcutIndex + 1}");
                                ExecuteShortcut(shortcutIndex);
                                e.Handled = true;
                            }
                            else
                            {
                                ed?.WriteMessage($" -> No matching shortcut found");
                            }
                        }
                        else
                        {
                            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                            ed?.WriteMessage($"\nDEBUG: Shortcuts disabled - MainPrompt: {_isAtMainPrompt}, Override: {_keyboardOverrideEnabled}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPreTranslateMessage: {ex.Message}");
            }
        }

        protected virtual void ExecuteShortcut(int shortcutIndex)
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nDEBUG: ExecuteShortcut called with index {shortcutIndex}");
                
                if (_commandDataList != null)
                {
                    var sortedCommands = _commandDataList.OrderByDescending(c => c.UsageCount).ToList();
                    ed?.WriteMessage($"\nDEBUG: Found {sortedCommands.Count} commands");
                    
                    if (shortcutIndex < sortedCommands.Count)
                    {
                        var command = sortedCommands[shortcutIndex];
                        ed?.WriteMessage($"\nDEBUG: Executing command: {command.CommandName}");
                        
                        var doc = Application.DocumentManager.MdiActiveDocument;
                        if (doc != null)
                        {
                            string cmdToSend = CommandAliasService.GetAlias(command.CommandName);
                            ed?.WriteMessage($"\nDEBUG: Sending to AutoCAD: '{cmdToSend}'");
                            doc.SendStringToExecute(cmdToSend + " ", true, false, false);
                        }
                        else
                        {
                            ed?.WriteMessage($"\nDEBUG: No active document found");
                        }
                    }
                    else
                    {
                        ed?.WriteMessage($"\nDEBUG: Shortcut index {shortcutIndex} >= command count {sortedCommands.Count}");
                    }
                }
                else
                {
                    ed?.WriteMessage($"\nDEBUG: Command data list is null");
                }
            }
            catch (System.Exception ex)
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nDEBUG: Error in ExecuteShortcut: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error in ExecuteShortcut: {ex.Message}");
            }
        }
        #endregion

        #region Shared Data Management
        protected virtual void LoadShortcutKeys()
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nDEBUG: LoadShortcutKeys called");
                
                var settings = _persistenceService?.LoadSettings() ?? new UserSettings();
                ed?.WriteMessage($"\nDEBUG: Settings CustomShortcutKeys count: {settings.CustomShortcutKeys?.Count ?? 0}");
                ed?.WriteMessage($"\nDEBUG: Settings CustomShortcutKeys: [{string.Join(",", settings.CustomShortcutKeys ?? new List<char>())}]");
                
                _shortcutKeys = settings.CustomShortcutKeys ?? UserSettings.GetDefaultShortcutKeys();
                
                ed?.WriteMessage($"\nDEBUG: Loaded {_shortcutKeys?.Count ?? 0} shortcut keys: [{string.Join(",", _shortcutKeys ?? new List<char>())}]");
                
                // Ensure we always have exactly 9 shortcut keys
                if (_shortcutKeys.Count != 9)
                {
                    ed?.WriteMessage($"\nDEBUG: Wrong count ({_shortcutKeys.Count}), using defaults and saving corrected settings");
                    _shortcutKeys = UserSettings.GetDefaultShortcutKeys();
                    ed?.WriteMessage($"\nDEBUG: Default keys: [{string.Join(",", _shortcutKeys ?? new List<char>())}]");
                    
                    // Save the corrected default keys to fix corrupted settings
                    settings.CustomShortcutKeys = new List<char>(_shortcutKeys);
                    _persistenceService?.SaveSettings(settings);
                    ed?.WriteMessage($"\nDEBUG: Corrected settings saved");
                }
            }
            catch (System.Exception ex)
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\nDEBUG: Error loading shortcut keys: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error loading shortcut keys: {ex.Message}");
                _shortcutKeys = UserSettings.GetDefaultShortcutKeys();
            }
        }

        public virtual void UpdateShortcutKeys(List<char> newKeys)
        {
            try
            {
                if (newKeys != null && newKeys.Count == 9)
                {
                    _shortcutKeys = new List<char>(newKeys);
                    
                    // Save to settings
                    var settings = _persistenceService?.LoadSettings() ?? new UserSettings();
                    settings.CustomShortcutKeys = new List<char>(_shortcutKeys);
                    _persistenceService?.SaveSettings(settings);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating shortcut keys: {ex.Message}");
            }
        }

        public virtual void ResetShortcutKeysToDefault()
        {
            try
            {
                _shortcutKeys = UserSettings.GetDefaultShortcutKeys();
                
                // Save to settings
                var settings = _persistenceService?.LoadSettings() ?? new UserSettings();
                settings.CustomShortcutKeys = new List<char>(_shortcutKeys);
                _persistenceService?.SaveSettings(settings);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting shortcut keys: {ex.Message}");
            }
        }

        protected virtual void LoadCommandData()
        {
            try
            {
                _commandDataList = _persistenceService?.LoadCommandData() ?? new List<CommandData>();
                InitializeDefaultCommands();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading command data: {ex.Message}");
            }
        }

        protected virtual void SaveCommandData()
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

        protected virtual void AutoSave()
        {
            try
            {
                if (_commandDataList?.Sum(c => c.UsageCount) % 10 == 0)
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

        #region Abstract Methods - Platform-specific implementation
        protected abstract void OnDisplayUpdateNeeded();
        protected abstract void OnOverrideStateChanged();
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