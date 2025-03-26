using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoolWear.Converters;

public partial class TelephoneFormatter : IValueConverter
{
    public string Separator { get; set; } = "-";
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return "";
        }

        string telephone = (string)value;

        string formatted = Regex.Replace(
            telephone,
            @"(\d{4,5})(\d{3})(\d{3})",
            $"$1-$2-$3"
        );
        formatted = formatted.Replace("-", Separator);

        return formatted;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
