using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using _2017_test_binding.ViewModels;

namespace _2017_test_binding.Views
{
    public partial class MainWindowEnhanced : Window
    {
        private MainViewModelEnhanced _viewModel;

        public MainWindowEnhanced(MainViewModelEnhanced viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            _viewModel = viewModel;
            DataContext = _viewModel;
            
            InitializeComponent();

            // Subscribe to view model events
            _viewModel.CloseRequested += OnCloseRequested;
            _viewModel.SettingsRequested += OnSettingsRequested;
            _viewModel.StatisticsRequested += OnStatisticsRequested;
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
                    case Key.Y:
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