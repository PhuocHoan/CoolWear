using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Data;
using System;

namespace CoolWear.Converters;
public partial class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ReportPeriod period)
        {
            switch (period)
            {
                case ReportPeriod.Daily: return "Ngày";
                case ReportPeriod.Monthly: return "Tháng";
                case ReportPeriod.Yearly: return "Năm";
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        // Thường không cần ConvertBack cho hiển thị Enum
        throw new NotImplementedException();
}