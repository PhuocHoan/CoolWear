using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Converters;

public partial class CurrencyFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return "";

        int amount = (int)value;
        CultureInfo culture = CultureInfo.GetCultureInfo("vi-VN");  // en-US /en-UK
        string formatted = amount.ToString("#,### đ", culture.NumberFormat);
        return formatted;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
