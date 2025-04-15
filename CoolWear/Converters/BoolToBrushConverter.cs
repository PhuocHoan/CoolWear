using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace CoolWear.Converters;

public partial class BoolToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.Red);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Lấy Brush mặc định (FalseBrush) từ parameter
        Brush falseBrush = new SolidColorBrush(Colors.Black);

        if (value is bool isTrue && isTrue)
        {
            return TrueBrush; // Trả về màu đỏ nếu IsDeleted = true
        }
        return falseBrush; // Mặc định trả về màu từ parameter (hoặc màu đen nếu parameter lỗi)
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}