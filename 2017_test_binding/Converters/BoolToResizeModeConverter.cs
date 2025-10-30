using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace _2017_test_binding.Converters
{
    public class BoolToResizeModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isResizable)
            {
                return isResizable ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
            }
            return ResizeMode.CanResizeWithGrip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResizeMode resizeMode)
            {
                return resizeMode == ResizeMode.CanResizeWithGrip || resizeMode == ResizeMode.CanResize;
            }
            return true;
        }
    }
}