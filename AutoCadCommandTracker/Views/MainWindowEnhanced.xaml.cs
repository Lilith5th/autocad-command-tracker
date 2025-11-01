using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AutoCadCommandTracker.ViewModels;

namespace AutoCadCommandTracker.Views
{
    public partial class MainWindowEnhanced : Window
    {
        private MainViewModelEnhanced _viewModel;
        private bool _isUpdatingFromViewModel = false;
        private DispatcherTimer _autocadMonitorTimer;
        private bool _wasAutoCadMinimized = false;

        // Win32 API imports for window state monitoring
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public MainWindowEnhanced(MainViewModelEnhanced viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            _viewModel = viewModel;
            DataContext = _viewModel;

            InitializeComponent();

            // Subscribe to view model events
            _viewModel.SettingsRequested += OnSettingsRequested;
            _viewModel.StatisticsRequested += OnStatisticsRequested;

            // Subscribe to property changes to force window width/height updates
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Subscribe to window size changes to save resized widths
            this.SizeChanged += Window_SizeChanged;

            // Start monitoring AutoCAD window state
            StartAutoCadMonitoring();
        }

        private void StartAutoCadMonitoring()
        {
            // Create timer to check AutoCAD window state every 500ms
            _autocadMonitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _autocadMonitorTimer.Tick += AutoCadMonitorTimer_Tick;
            _autocadMonitorTimer.Start();
        }

        private void AutoCadMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Get AutoCAD's main window handle
                var acApp = Autodesk.AutoCAD.ApplicationServices.Application.MainWindow;
                if (acApp == null)
                    return;

                IntPtr acWindowHandle = acApp.Handle;

                // Check if AutoCAD is minimized
                bool isMinimized = IsIconic(acWindowHandle);

                // Only act on state changes (minimize/restore)
                if (isMinimized && !_wasAutoCadMinimized)
                {
                    // AutoCAD just minimized - hide our window
                    this.Hide();
                    _wasAutoCadMinimized = true;
                }
                else if (!isMinimized && _wasAutoCadMinimized)
                {
                    // AutoCAD just restored - show our window
                    this.Show();
                    _wasAutoCadMinimized = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error monitoring AutoCAD state: {ex.Message}");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Don't save size changes triggered by ViewModel (only manual resizes)
            if (_isUpdatingFromViewModel)
                return;

            // Save the manually resized width to the appropriate mode setting
            if (e.WidthChanged)
            {
                _viewModel.UpdateResizedWidth(this.ActualWidth);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Prevent infinite loop with guard flag
            if (_isUpdatingFromViewModel)
                return;

            // Force window dimensions to update when ViewModel properties change
            // This ensures the window responds to width/height changes from compact view toggle
            if (e.PropertyName == "WindowWidth")
            {
                try
                {
                    _isUpdatingFromViewModel = true;
                    this.Width = _viewModel.WindowWidth;
                }
                finally
                {
                    _isUpdatingFromViewModel = false;
                }
            }
            else if (e.PropertyName == "WindowHeight")
            {
                try
                {
                    _isUpdatingFromViewModel = true;
                    this.Height = _viewModel.WindowHeight;
                }
                finally
                {
                    _isUpdatingFromViewModel = false;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowSettingsCommand.Execute(null);
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowStatisticsCommand.Execute(null);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Stop the monitoring timer
            if (_autocadMonitorTimer != null)
            {
                _autocadMonitorTimer.Stop();
                _autocadMonitorTimer.Tick -= AutoCadMonitorTimer_Tick;
            }

            // Save window position and size
            _viewModel.SaveWindowState();

            // Hide instead of close to keep the instance alive
            e.Cancel = true;
            Hide();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.ExecuteSelectedCommand.Execute(null);
        }

        private void SetCustomShortcut_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _viewModel.SelectedItem;
            if (selectedItem == null || string.IsNullOrEmpty(selectedItem.DisplayText))
            {
                MessageBox.Show("Please select a command first.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Only allow for commands, not values
            if (selectedItem.ItemType != "Command")
            {
                MessageBox.Show("Custom shortcuts can only be set for commands, not values.",
                    "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create and show the WPF shortcut input window
            var dialog = new ShortcutInputWindow(selectedItem.DisplayText);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.ShortcutKey))
            {
                _viewModel.SetCustomShortcut(selectedItem.DisplayText, dialog.ShortcutKey);
                MessageBox.Show($"Custom shortcut '{dialog.ShortcutKey}' set for {selectedItem.DisplayText}",
                    "Shortcut Set", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearCustomShortcut_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _viewModel.SelectedItem;
            if (selectedItem == null || string.IsNullOrEmpty(selectedItem.DisplayText))
            {
                MessageBox.Show("Please select a command first.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_viewModel.ClearCustomShortcut(selectedItem.DisplayText))
            {
                MessageBox.Show($"Custom shortcut cleared for {selectedItem.DisplayText}",
                    "Shortcut Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No custom shortcut found for {selectedItem.DisplayText}",
                    "No Shortcut", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            Close();
        }

        private void OnSettingsRequested(object sender, EventArgs e)
        {
            var settingsWindow = new SettingsWindow(_viewModel);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void OnStatisticsRequested(object sender, EventArgs e)
        {
            var statsWindow = new StatisticsWindow(_viewModel);
            statsWindow.Owner = this;
            statsWindow.ShowDialog();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Set up keyboard hooks for global shortcuts
            SetupKeyboardHooks();
        }

        private void SetupKeyboardHooks()
        {
            // Implementation for global keyboard shortcuts
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Build the pressed key combination
            string pressedCombo = "";
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                pressedCombo += "Ctrl+";
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                pressedCombo += "Alt+";
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                pressedCombo += "Shift+";
            pressedCombo += e.Key.ToString();

            // First, try to execute custom command shortcuts
            if (_viewModel.ExecuteCustomShortcut(pressedCombo))
            {
                e.Handled = true;
                return;
            }

            // Fall back to grid position shortcuts (Ctrl+Q, Ctrl+W, etc.)
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Q:
                        _viewModel.ExecuteCommandAtIndex(0);
                        e.Handled = true;
                        break;
                    case Key.W:
                        _viewModel.ExecuteCommandAtIndex(1);
                        e.Handled = true;
                        break;
                    case Key.E:
                        _viewModel.ExecuteCommandAtIndex(2);
                        e.Handled = true;
                        break;
                    case Key.A:
                        _viewModel.ExecuteCommandAtIndex(3);
                        e.Handled = true;
                        break;
                    case Key.S:
                        _viewModel.ExecuteCommandAtIndex(4);
                        e.Handled = true;
                        break;
                    case Key.D:
                        _viewModel.ExecuteCommandAtIndex(5);
                        e.Handled = true;
                        break;
                    case Key.Z:
                        _viewModel.ExecuteCommandAtIndex(6);
                        e.Handled = true;
                        break;
                    case Key.X:
                        _viewModel.ExecuteCommandAtIndex(7);
                        e.Handled = true;
                        break;
                    case Key.C:
                        _viewModel.ExecuteCommandAtIndex(8);
                        e.Handled = true;
                        break;
                }
            }
        }
    }
}