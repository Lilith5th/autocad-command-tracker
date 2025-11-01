using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AutoCadCommandTracker.Models;
using AutoCadCommandTracker.Services;

namespace AutoCadCommandTracker.ViewModels
{
    public class MainViewModelEnhanced : INotifyPropertyChanged
    {
        private readonly DataPersistenceService _persistenceService;
        private ObservableCollection<DisplayItem> _displayData;
        private ObservableCollection<DisplayItem> _filteredDisplayData;
        private string _searchText;
        private string _windowTitle = "Commands";
        private string _statusText = "Ready";
        private string _profileName = "Default";
        private DisplayItem _selectedItem;
        private UserSettings _settings;
        private bool _isAtMainPrompt = true;

        public event EventHandler SettingsRequested;
        public event EventHandler StatisticsRequested;

        public MainViewModelEnhanced()
        {
            _persistenceService = new DataPersistenceService();
            _displayData = new ObservableCollection<DisplayItem>();
            _filteredDisplayData = new ObservableCollection<DisplayItem>();
            
            LoadSettings();
            InitializeCommands();
        }

        #region Properties

        public ObservableCollection<DisplayItem> FilteredDisplayData
        {
            get => _filteredDisplayData;
            set
            {
                _filteredDisplayData = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterDisplayData();
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public string ProfileName
        {
            get => _profileName;
            set
            {
                _profileName = value;
                OnPropertyChanged();
            }
        }

        public DisplayItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public double WindowOpacity
        {
            get => _settings?.WindowOpacity ?? 0.6;
            set
            {
                if (_settings != null)
                {
                    _settings.WindowOpacity = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public double WindowLeft
        {
            get => _settings?.WindowLeft ?? 100;
            set
            {
                if (_settings != null)
                {
                    _settings.WindowLeft = value;
                    OnPropertyChanged();
                }
            }
        }

        public double WindowTop
        {
            get => _settings?.WindowTop ?? 100;
            set
            {
                if (_settings != null)
                {
                    _settings.WindowTop = value;
                    OnPropertyChanged();
                }
            }
        }

        public double WindowWidth
        {
            get => _settings?.WindowWidth ?? 200;
            set
            {
                if (_settings != null)
                {
                    _settings.WindowWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public double WindowHeight
        {
            get => _settings?.WindowHeight ?? 220;
            set
            {
                if (_settings != null)
                {
                    _settings.WindowHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDarkTheme => _settings?.IsDarkTheme ?? false;

        public bool IsResizable
        {
            get => _settings?.IsResizable ?? true;
            set
            {
                if (_settings != null)
                {
                    _settings.IsResizable = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResizeModeValue));
                }
            }
        }

        public object ResizeModeValue => IsResizable ? System.Windows.ResizeMode.CanResizeWithGrip : System.Windows.ResizeMode.NoResize;

        public bool SynchronizeWithAutoCAD
        {
            get => _settings?.SynchronizeWithAutoCAD ?? true;
            set
            {
                if (_settings != null)
                {
                    _settings.SynchronizeWithAutoCAD = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public bool IsCompactView
        {
            get => _settings?.IsCompactView ?? false;
            set
            {
                if (_settings != null)
                {
                    _settings.IsCompactView = value;

                    // Use stored widths for compact and expanded modes
                    _settings.WindowWidth = value ? _settings.CompactWidth : _settings.ExpandedWidth;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CompactViewButtonText));
                    OnPropertyChanged(nameof(SearchBarVisible));
                    OnPropertyChanged(nameof(WindowWidth));
                    SaveSettings();
                }
            }
        }

        public string CompactViewButtonText => IsCompactView ? "⊟" : "⊞";

        public bool SearchBarVisible => !IsCompactView;

        #endregion

        #region Commands

        public ICommand ShowSettingsCommand => new RelayCommand(() => SettingsRequested?.Invoke(this, EventArgs.Empty));
        public ICommand ShowStatisticsCommand => new RelayCommand(() => StatisticsRequested?.Invoke(this, EventArgs.Empty));
        public ICommand ExecuteSelectedCommand => new RelayCommand(ExecuteSelected);
        public ICommand SetLightThemeCommand => new RelayCommand(SetLightTheme);
        public ICommand SetDarkThemeCommand => new RelayCommand(SetDarkTheme);
        public ICommand ToggleCompactViewCommand => new RelayCommand(ToggleCompactView);

        #endregion

        #region Methods

        private void InitializeCommands()
        {
            ShowSettingsCommand.ToString(); // Force initialization
            ShowStatisticsCommand.ToString();
            ExecuteSelectedCommand.ToString();
        }

        private void LoadSettings()
        {
            _settings = _persistenceService.LoadSettings();
            OnPropertyChanged(nameof(WindowOpacity));
            OnPropertyChanged(nameof(WindowLeft));
            OnPropertyChanged(nameof(WindowTop));
            OnPropertyChanged(nameof(WindowWidth));
            OnPropertyChanged(nameof(WindowHeight));
            OnPropertyChanged(nameof(IsDarkTheme));
            OnPropertyChanged(nameof(IsResizable));
            OnPropertyChanged(nameof(IsCompactView));
            OnPropertyChanged(nameof(CompactViewButtonText));
            OnPropertyChanged(nameof(SearchBarVisible));
        }

        public void SaveSettings()
        {
            _persistenceService.SaveSettings(_settings);
        }

        public void SaveWindowState()
        {
            SaveSettings();
        }

        public void UpdateResizedWidth(double newWidth)
        {
            if (_settings != null)
            {
                // Save the new width to the appropriate mode setting
                if (_settings.IsCompactView)
                {
                    _settings.CompactWidth = newWidth;
                }
                else
                {
                    _settings.ExpandedWidth = newWidth;
                }

                _settings.WindowWidth = newWidth;
                SaveSettings();
            }
        }

        public void UpdateDisplayData(List<CommandData> commands, bool isAtMainPrompt)
        {
            _isAtMainPrompt = isAtMainPrompt;
            _displayData.Clear();
            
            // Handle null commands list
            if (commands == null)
            {
                commands = new List<CommandData>();
            }
            
            int index = 1;
            // Use 3x3 grid layout: Q-W-E, A-S-D, Z-X-C (consistent across keyboard layouts)
            var shortcutKeys = new[] { 'Q', 'W', 'E', 'A', 'S', 'D', 'Z', 'X', 'C' };
            foreach (var cmd in commands.OrderByDescending(c => c.UsageCount))
            {
                // Check for custom shortcut first, then fall back to grid position shortcut
                string shortcut = "";
                if (_settings?.CustomCommandShortcuts != null && _settings.CustomCommandShortcuts.ContainsKey(cmd.CommandName))
                {
                    shortcut = _settings.CustomCommandShortcuts[cmd.CommandName];
                }
                else if (index <= shortcutKeys.Length)
                {
                    shortcut = $"Ctrl+{shortcutKeys[index - 1]}";
                }

                var displayItem = new DisplayItem
                {
                    Index = index++,
                    DisplayText = cmd.CommandName,
                    UsageCount = cmd.UsageCount,
                    ItemType = "Command",
                    RawData = cmd,
                    Shortcut = shortcut,
                    IsPinned = false
                };

                // Subscribe to property changes to re-sort when pinned status changes
                displayItem.PropertyChanged += DisplayItem_PropertyChanged;

                _displayData.Add(displayItem);
            }

            WindowTitle = isAtMainPrompt ? "Commands" : "Values";
            FilterDisplayData();
        }

        public void UpdateDisplayDataWithValues(List<InputValue> values)
        {
            _displayData.Clear();

            int index = 1;
            foreach (var val in values.OrderByDescending(v => v.UsageCount))
            {
                var displayItem = new DisplayItem
                {
                    Index = index++,
                    DisplayText = val.DisplayValue,
                    UsageCount = val.UsageCount,
                    ItemType = val.Type.ToString(),
                    RawData = val,
                    Shortcut = "", // No shortcuts for values
                    IsPinned = false
                };

                // Subscribe to property changes to re-sort when pinned status changes
                displayItem.PropertyChanged += DisplayItem_PropertyChanged;

                _displayData.Add(displayItem);
            }

            WindowTitle = "Values";
            FilterDisplayData();
        }

        private void DisplayItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Re-sort when pin status changes
            if (e.PropertyName == nameof(DisplayItem.IsPinned))
            {
                FilterDisplayData();
            }
        }

        private void FilterDisplayData()
        {
            _filteredDisplayData.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _displayData
                : _displayData.Where(d => d.DisplayText.ToLower().Contains(SearchText.ToLower()));

            // Sort: Pinned items first, then by usage count
            var sorted = filtered.OrderByDescending(item => item.IsPinned)
                                 .ThenByDescending(item => item.UsageCount);

            int index = 1;
            // Use 3x3 grid layout: Q-W-E, A-S-D, Z-X-C (consistent across keyboard layouts)
            var shortcutKeys = new[] { 'Q', 'W', 'E', 'A', 'S', 'D', 'Z', 'X', 'C' };
            foreach (var item in sorted)
            {
                item.Index = index;

                // Update shortcut based on custom shortcut or new filtered index for commands
                if (item.ItemType == "Command")
                {
                    // Check for custom shortcut first
                    if (_settings?.CustomCommandShortcuts != null && _settings.CustomCommandShortcuts.ContainsKey(item.DisplayText))
                    {
                        item.Shortcut = _settings.CustomCommandShortcuts[item.DisplayText];
                    }
                    else if (index <= shortcutKeys.Length)
                    {
                        item.Shortcut = $"Ctrl+{shortcutKeys[index - 1]}";
                    }
                    else
                    {
                        item.Shortcut = "";
                    }
                }
                else
                {
                    item.Shortcut = "";
                }
                index++;
                _filteredDisplayData.Add(item);
            }

            StatusText = $"{_filteredDisplayData.Count} items";
        }

        private void ExecuteSelected()
        {
            if (SelectedItem != null)
            {
                ExecuteCommandAtIndex(SelectedItem.Index - 1);
            }
        }

        public void ExecuteCommandAtIndex(int index)
        {
            if (index >= 0 && index < _filteredDisplayData.Count)
            {
                var item = _filteredDisplayData[index];
                // Send command to AutoCAD
                SendToAutoCAD(item.DisplayText);
            }
        }

        private void SendToAutoCAD(string command)
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    // Use alias if it's a command
                    var commandToSend = CommandAliasService.GetExecutionCommand(command);
                    doc.SendStringToExecute(commandToSend + " ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending command to AutoCAD: {ex.Message}");
            }
        }

        public void SetCustomShortcut(string commandName, string shortcutKey)
        {
            if (_settings != null && !string.IsNullOrEmpty(commandName) && !string.IsNullOrEmpty(shortcutKey))
            {
                _settings.CustomCommandShortcuts[commandName] = shortcutKey;
                _persistenceService.SaveSettings(_settings);

                // Refresh display to show new shortcut
                FilterDisplayData();
            }
        }

        public bool ClearCustomShortcut(string commandName)
        {
            if (_settings != null && !string.IsNullOrEmpty(commandName))
            {
                if (_settings.CustomCommandShortcuts.ContainsKey(commandName))
                {
                    _settings.CustomCommandShortcuts.Remove(commandName);
                    _persistenceService.SaveSettings(_settings);

                    // Refresh display to update shortcut column
                    FilterDisplayData();
                    return true;
                }
            }
            return false;
        }

        public bool ExecuteCustomShortcut(string shortcutKey)
        {
            if (_settings != null && _settings.CustomCommandShortcuts != null)
            {
                foreach (var kvp in _settings.CustomCommandShortcuts)
                {
                    if (kvp.Value == shortcutKey)
                    {
                        // Find the command in the current display data
                        var commandItem = _filteredDisplayData.FirstOrDefault(item =>
                            item.DisplayText == kvp.Key && item.ItemType == "Command");

                        if (commandItem != null)
                        {
                            SendToAutoCAD(commandItem.DisplayText);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void SwitchTheme()
        {
            if (_settings != null)
            {
                _settings.IsDarkTheme = !_settings.IsDarkTheme;
                _persistenceService.SaveSettings(_settings);
                OnPropertyChanged(nameof(IsDarkTheme));
                SaveSettings();
            }
        }

        private void SetLightTheme()
        {
            if (_settings != null)
            {
                _settings.IsDarkTheme = false;
                _persistenceService.SaveSettings(_settings);
                OnPropertyChanged(nameof(IsDarkTheme));
                SaveSettings();
            }
        }

        private void SetDarkTheme()
        {
            if (_settings != null)
            {
                _settings.IsDarkTheme = true;
                _persistenceService.SaveSettings(_settings);
                OnPropertyChanged(nameof(IsDarkTheme));
                SaveSettings();
            }
        }

        private void ToggleCompactView()
        {
            IsCompactView = !IsCompactView;
        }

        public void OnOverrideStateChanged(bool isEnabled)
        {
            try
            {
                // Update any visual indicators based on override state
                // This could update border color, status text, etc.
                StatusText = isEnabled ? "Override Enabled" : "Override Disabled";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating override state in view model: {ex.Message}");
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


    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}