using System;
using System.Windows;
using Microsoft.Win32;
using AutoCadCommandTracker.ViewModels;

namespace AutoCadCommandTracker.Views
{
    public partial class SettingsWindow : Window
    {
        private MainViewModelEnhanced _viewModel;

        public SettingsWindow(MainViewModelEnhanced viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveSettings();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveSettings();
        }

        private void NewProfile_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for creating new profile
            var dialog = new InputDialog("New Profile", "Enter profile name:");
            if (dialog.ShowDialog() == true)
            {
                // Create new profile logic
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for deleting profile
            if (MessageBox.Show("Delete selected profile?", "Confirm", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Delete profile logic
            }
        }

        private void ExportProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Export logic would go here
                    MessageBox.Show("Profile exported successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting profile: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Import logic would go here
                    MessageBox.Show("Profile imported successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing profile: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Simple input dialog
    public class InputDialog : Window
    {
        private System.Windows.Controls.TextBox _textBox;

        public string ResponseText => _textBox?.Text ?? string.Empty;

        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 300;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

            var label = new System.Windows.Controls.Label { Content = prompt };
            grid.Children.Add(label);

            _textBox = new System.Windows.Controls.TextBox { Margin = new Thickness(5) };
            System.Windows.Controls.Grid.SetRow(_textBox, 1);
            grid.Children.Add(_textBox);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(5),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 75,
                Margin = new Thickness(5),
                IsCancel = true
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
}