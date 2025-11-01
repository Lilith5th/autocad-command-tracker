using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoCadCommandTracker.Models
{
    public class CommandData : INotifyPropertyChanged
    {
        private string _commandName;
        private int _usageCount;
        private List<InputValue> _inputValues;
        private DateTime _lastUsed;
        private List<string> _followedByCommands;
        private string _category;

        public CommandData(string commandName)
        {
            CommandName = commandName;
            UsageCount = 0;
            InputValues = new List<InputValue>();
            LastUsed = DateTime.Now;
            FollowedByCommands = new List<string>();
        }

        public string CommandName
        {
            get => _commandName;
            set
            {
                _commandName = value;
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

        public List<InputValue> InputValues
        {
            get => _inputValues;
            set
            {
                _inputValues = value;
                OnPropertyChanged();
            }
        }

        public DateTime LastUsed
        {
            get => _lastUsed;
            set
            {
                _lastUsed = value;
                OnPropertyChanged();
            }
        }

        public List<string> FollowedByCommands
        {
            get => _followedByCommands;
            set
            {
                _followedByCommands = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public void IncrementUsage()
        {
            UsageCount++;
            LastUsed = DateTime.Now;
        }

        public void AddFollowedCommand(string commandName)
        {
            if (!string.IsNullOrEmpty(commandName) && !FollowedByCommands.Contains(commandName))
            {
                FollowedByCommands.Add(commandName);
            }
        }

        public void AddInputValue(string value, InputType type)
        {
            if (string.IsNullOrEmpty(value)) return;

            var existingValue = InputValues.Find(v => v.Value.ToString() == value && v.Type == type);
            if (existingValue != null)
            {
                existingValue.IncrementUsage();
            }
            else
            {
                InputValues.Add(new InputValue(value, type));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}