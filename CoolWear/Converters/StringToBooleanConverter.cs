using Microsoft.UI.Xaml.Data;
using System;

namespace CoolWear.Converters;

public class StringToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue && parameter is string paramString)
        {
            return stringValue == paramString;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            return boolValue ? paramString : null;
        }
        return null;
    }
}
