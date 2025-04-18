using System;
using Microsoft.UI.Xaml.Data;

namespace CoolWear.Converters;

public partial class IntToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue && parameter is string paramString && int.TryParse(paramString, out int paramValue))
        {
            return intValue == paramValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue && parameter is string paramString && int.TryParse(paramString, out int paramValue))
        {
            return boolValue ? paramValue : 0;
        }
        return 0;
    }
}
