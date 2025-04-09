using Microsoft.UI.Xaml.Data;
using System;

namespace CoolWear.Converters;
public partial class NullableIntToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Chuyển từ int? sang double? rồi sang double (mặc định là NaN nếu null)
        if (value is int intValue)
        {
            return (double)intValue; // Ép kiểu int sang double
        }
        // Nếu value là null (hoặc không phải int), trả về double.NaN để NumberBox hiển thị trống
        return double.NaN;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // Chuyển từ double sang int?
        if (value is double doubleValue && !double.IsNaN(doubleValue))
        {
            // Kiểm tra xem có thể ép kiểu về int không (loại bỏ phần thập phân)
            if (doubleValue >= int.MinValue && doubleValue <= int.MaxValue)
            {
                // Ép kiểu về int (phần thập phân sẽ bị mất)
                return (int)doubleValue;
            }
        }
        // Nếu giá trị nhập vào không hợp lệ hoặc trống (NaN), trả về null cho int?
        return null;
    }
}