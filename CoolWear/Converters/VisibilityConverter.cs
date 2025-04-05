using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace CoolWear.Converters;

public partial class ZeroIsCollapsedConverter : IValueConverter
{
    // Convert count (int, bool or collection count) to Visibility
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isZero = false;
        if (value is int i)
        {
            isZero = i == 0;
        }
        else if (value is bool b)
        {
            isZero = b == false;
        }
        else if (value is ICollection c) // Check if it's a collection
        {
            isZero = c.Count == 0;
        }

        return isZero ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public partial class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isGreaterThanZero = false;

        if (value is int intValue)
        {
            isGreaterThanZero = intValue > 0;
        }

        return isGreaterThanZero ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException("Cannot convert Visibility back to an integer count.");
}