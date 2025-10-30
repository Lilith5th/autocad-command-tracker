using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using _2017_test_binding.Models;
using _2017_test_binding.Services;

namespace _2017_test_binding.ViewModels
{
    public class MainViewModelEnhanced : INotifyPropertyChanged
    {
        private readonly DataPersistenceService _persistenceService;
        private readonly CommandAnalyticsService _analyticsService;
        private ObservableCollection<DisplayItem> _displayData;
        private ObservableCollection<DisplayItem> _filteredDisplayData;
        private string _searchText;
        private string _windowTitle = "Commands";
        private string _statusText = "Ready";
        private string _profileName = "Default";
        private DisplayItem _selectedItem;
        private UserSettings _settings;
        private bool _isAtMainPrompt = true;

        public event EventHandler CloseRequested;
        public event EventHandler SettingsRequested;
        public event EventHandler StatisticsRequested;

        public MainViewModelEnhanced()
        {
            _persistenceService = new DataPersistenceService();
            _analyticsService = new CommandAnalyticsService();
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

        public bool IsDarkTheme => _settings?.Theme == "Dark";

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

        #endregion

        #region Commands

        public ICommand ShowSettingsCommand => new RelayCommand(() => SettingsRequested?.Invoke(this, EventArgs.Empty));
        public ICommand ShowStatisticsCommand => new RelayCommand(() => StatisticsRequested?.Invoke(this, EventArgs.Empty));
        public ICommand ExecuteSelectedCommand => new RelayCommand(ExecuteSelected);
        public ICommand SetLightThemeCommand => new RelayCommand(SetLightTheme);
        public ICommand SetDarkThemeCommand => new RelayCommand(SetDarkTheme);

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
        }

        public void SaveSettings()
        {
            _persistenceService.SaveSettings(_settings);
        }

        public void SaveWindowState()
        {
            SaveSettings();
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
                var shortcut = index <= shortcutKeys.Length ? $"Ctrl+{shortcutKeys[index - 1]}" : "";
                _displayData.Add(new DisplayItem
                {
                    Index = index++,
                    DisplayText = cmd.CommandName,
                    UsageCount = cmd.UsageCount,
                    ItemType = "Command",
                    RawData = cmd,
                    Shortcut = shortcut
                });
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
                _displayData.Add(new DisplayItem
                {
                    Index = index++,
                    DisplayText = val.DisplayValue,
                    UsageCount = val.UsageCount,
                    ItemType = val.Type.ToString(),
                    RawData = val,
                    Shortcut = "" // No shortcuts for values
                });
            }

            WindowTitle = "Values";
            FilterDisplayData();
        }

        private void FilterDisplayData()
        {
            _filteredDisplayData.Clear();
            
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _displayData
                : _displayData.Where(d => d.DisplayText.ToLower().Contains(SearchText.ToLower()));

            int index = 1;
            // Use 3x3 grid layout: Q-W-E, A-S-D, Z-X-C (consistent across keyboard layouts)
            var shortcutKeys = new[] { 'Q', 'W', 'E', 'A', 'S', 'D', 'Z', 'X', 'C' };
            foreach (var item in filtered)
            {
                item.Index = index;
                // Update shortcut based on new filtered index for commands
                if (item.ItemType == "Command" && index <= shortcutKeys.Length)
                {
                    item.Shortcut = $"Ctrl+{shortcutKeys[index - 1]}";
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

        public void SwitchTheme()
        {
            if (_settings != null)
            {
                _settings.Theme = _settings.Theme == "Dark" ? "Light" : "Dark";
                OnPropertyChanged(nameof(IsDarkTheme));
                SaveSettings();
            }
        }

        private void SetLightTheme()
        {
            if (_settings != null)
            {
                _settings.Theme = "Light";
                OnPropertyChanged(nameof(IsDarkTheme));
                SaveSettings();
            }
        }

        private void SetDarkTheme()
        {
            if (_settings != null)
            {
                _settings.Theme = "Dark";
                OnPropertyChanged(nameof(IsDarkTheme));
                SaveSettings();
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

    public class DisplayItem
    {
        public int Index { get; set; }
        public string DisplayText { get; set; }
        public string Alias { get; set; }
        public int UsageCount { get; set; }
        public string ItemType { get; set; }
        public object RawData { get; set; }
        public string Shortcut { get; set; }
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