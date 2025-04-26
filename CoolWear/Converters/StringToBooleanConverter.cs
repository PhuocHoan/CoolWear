using Microsoft.UI.Xaml.Data;
using System;

namespace CoolWear.Converters;

public partial class StringToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is string stringValue && parameter is string paramString ? stringValue == paramString : (object)false;

    public object? ConvertBack(object value, Type targetType, object parameter, string language) => value is bool boolValue && parameter is string paramString ? boolValue ? paramString : null : (object?)null;
}
