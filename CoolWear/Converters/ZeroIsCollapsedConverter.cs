using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections; // Required for ICollection

namespace CoolWear.Converters;

public class ZeroIsCollapsedConverter : IValueConverter
{
    // Convert count (int or collection count) to Visibility
    // 0 -> Collapsed, >0 -> Visible (unless parameter="Invert")
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
        // Add other numeric types if needed (e.g., long, double)

        bool invert = parameter as string == "Invert";

        // XOR: If isZero and not invert -> true (collapse)
        //      If not zero and not invert -> false (visible)
        //      If isZero and invert -> false (visible)
        //      If not zero and invert -> true (collapse)
        bool shouldCollapse = isZero ^ invert;

        return shouldCollapse ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}