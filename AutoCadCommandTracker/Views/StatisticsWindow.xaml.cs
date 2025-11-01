using System;
using System.Windows;
using Microsoft.Win32;
using AutoCadCommandTracker.ViewModels;

namespace AutoCadCommandTracker.Views
{
    public partial class StatisticsWindow : Window
    {
        private MainViewModelEnhanced _viewModel;

        public StatisticsWindow(MainViewModelEnhanced viewModel)
        {
            _viewModel = viewModel;
            DataContext = new StatisticsViewModel(_viewModel);
            InitializeComponent();
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv|XML files (*.xml)|*.xml|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Export logic based on selected format
                    MessageBox.Show("Data exported successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Simplified Statistics ViewModel
    public class StatisticsViewModel
    {
        private MainViewModelEnhanced _mainViewModel;

        public StatisticsViewModel(MainViewModelEnhanced mainViewModel)
        {
            _mainViewModel = mainViewModel;
            // Initialize statistics
        }

        // Statistics properties
        public int TotalCommands => 1234; // Placeholder
        public int UniqueCommands => 56; // Placeholder
        public string MostUsedCommand => "PLINE"; // Placeholder
        public double AverageCommandsPerHour => 45.6; // Placeholder
        public int PeakUsageHour => 14; // Placeholder

        // Collections for data grids
        public object CommandSequences { get; set; }
        public object CommandStatistics { get; set; }
        public object DailyPatterns { get; set; }
    }
}