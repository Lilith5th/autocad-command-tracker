using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace _2017_test_binding.Models
{
    public enum InputType
    {
        Number,
        Text,
        Point,
        Angle,
        Expression
    }

    public class InputValue : INotifyPropertyChanged
    {
        private object _value;
        private InputType _type;
        private int _usageCount;
        private DateTime _lastUsed;

        public InputValue(object value, InputType type)
        {
            Value = value;
            Type = type;
            UsageCount = 1;
            LastUsed = DateTime.Now;
        }

        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
            }
        }

        public InputType Type
        {
            get => _type;
            set
            {
                _type = value;
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

        public DateTime LastUsed
        {
            get => _lastUsed;
            set
            {
                _lastUsed = value;
                OnPropertyChanged();
            }
        }

        public string DisplayValue
        {
            get
            {
                switch (Type)
                {
                    case InputType.Number:
                        return Value.ToString();
                    case InputType.Text:
                        return $"\"{Value}\"";
                    case InputType.Point:
                        if (Value is Point3D point)
                            return $"({point.X:F2}, {point.Y:F2}, {point.Z:F2})";
                        return Value.ToString();
                    case InputType.Angle:
                        return $"{Value}°";
                    case InputType.Expression:
                        return $"={Value}";
                    default:
                        return Value.ToString();
                }
            }
        }

        public void IncrementUsage()
        {
            UsageCount++;
            LastUsed = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public struct Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}