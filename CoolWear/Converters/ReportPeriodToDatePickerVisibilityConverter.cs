using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Data;
using System;

namespace CoolWear.Converters;

public partial class ReportPeriodToDatePickerVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is ReportPeriod period && parameter is string partName
            ? partName switch
            {
                "Day" => period == ReportPeriod.Daily,
                "Month" => period != ReportPeriod.Yearly,
                "Year" => true,
                _ => true
            }
            : (object)true;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
