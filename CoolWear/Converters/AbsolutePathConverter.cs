using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Converters;

public partial class AbsolutePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return "";

        string filename = (string)value;
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        string path = $"{folder}Assets/{filename}";
        return path;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
