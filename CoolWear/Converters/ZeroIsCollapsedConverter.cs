using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace CoolWear.Converters;

public partial class ZeroIsCollapsedConverter : IValueConverter
{
    // Convert count (int or collection count) to Visibility
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isZero = false;
        if (value is int i)
        {
            isZero = i == 0;
        }
        else if (value is ICollection c) // Check if it's a collection
        {
            isZero = c.Count == 0;
        }

        return isZero ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}