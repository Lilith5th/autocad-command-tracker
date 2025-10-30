using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace _2017_test_binding.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _displayData;
        private double _windowOpacity = 0.6;
        private string _currentTheme = "Light";
        private bool _isResizable = true;
        private double _windowLeft;
        private double _windowTop;
        private double _windowWidth = 200;
        private double _windowHeight = 220;

        public ObservableCollection<string> DisplayData
        {
            get => _displayData;
            set
            {
                _displayData = value;
                OnPropertyChanged();
            }
        }

        public double WindowOpacity
        {
            get => _windowOpacity;
            set
            {
                _windowOpacity = Math.Max(0.1, Math.Min(1.0, value));
                OnPropertyChanged();
            }
        }

        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        public bool IsDarkTheme => CurrentTheme == "Dark";

        public bool IsResizable
        {
            get => _isResizable;
            set
            {
                _isResizable = value;
                OnPropertyChanged();
            }
        }

        public double WindowLeft
        {
            get => _windowLeft;
            set
            {
                _windowLeft = value;
                OnPropertyChanged();
            }
        }

        public double WindowTop
        {
            get => _windowTop;
            set
            {
                _windowTop = value;
                OnPropertyChanged();
            }
        }

        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = Math.Max(150, value);
                OnPropertyChanged();
            }
        }

        public double WindowHeight
        {
            get => _windowHeight;
            set
            {
                _windowHeight = Math.Max(100, value);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}