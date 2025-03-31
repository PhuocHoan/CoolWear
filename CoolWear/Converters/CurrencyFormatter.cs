using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace CoolWear.Converters;

public class CurrencyFormatConverter : IValueConverter
{
    // Convert value (int/decimal) to formatted currency string (e.g., "10.000 đ")
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int i)
        {
            return i.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        }
        if (value is decimal d)
        {
            return d.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        }
        if (value is double db)
        {
            return db.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        }
        return value?.ToString() ?? string.Empty; // Fallback
    }

    // Converting back is usually not needed for display converters
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}