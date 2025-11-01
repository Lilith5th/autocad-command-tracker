using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoCadCommandTracker.Models
{
    /// <summary>
    /// Represents a display item for UI components showing command information
    /// </summary>
    public class DisplayItem : INotifyPropertyChanged
    {
        private string _displayText;
        private string _alias;
        private int _usageCount;
        private string _itemType;
        private object _rawData;
        private string _shortcut;
        private bool _isPinned;

        public int Index { get; set; }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                OnPropertyChanged();
            }
        }

        public string Alias
        {
            get => _alias;
            set
            {
                _alias = value;
                OnPropertyChanged();
            }
        }

        public int UsageCount
        {
            get => _usageCount;
            set
            {
                _usageCount = value;
                OnPropertyChanged();
            }
        }

        public string ItemType
        {
            get => _itemType;
            set
            {
                _itemType = value;
                OnPropertyChanged();
            }
        }

        public object RawData
        {
            get => _rawData;
            set
            {
                _rawData = value;
                OnPropertyChanged();
            }
        }

        public string Shortcut
        {
            get => _shortcut;
            set
            {
                _shortcut = value;
                OnPropertyChanged();
            }
        }

        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                _isPinned = value;
                OnPropertyChanged();
            }
        }

        // For compatibility with WinForms version (maps to ItemType)
        public string Type
        {
            get => ItemType;
            set => ItemType = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}