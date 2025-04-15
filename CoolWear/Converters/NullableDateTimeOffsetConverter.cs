// Converters/NullableDateTimeOffsetConverter.cs
using Microsoft.UI.Xaml.Data;
using System;

namespace CoolWear.Converters;

public partial class NullableDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Nếu giá trị từ ViewModel là DateTimeOffset?, trả về chính nó
        if (value is DateTimeOffset dto)
        {
            return dto;
        }
        // Nếu giá trị từ ViewModel là null, trả về giá trị mặc định cho DatePicker
        // (ví dụ: ngày giờ hiện tại)
        return DateTimeOffset.Now;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // Giá trị từ DatePicker luôn là DateTimeOffset (non-nullable)
        // Chỉ cần trả về giá trị đó, kiểu DateTimeOffset? trong ViewModel sẽ chấp nhận
        if (value is DateTimeOffset dto)
        {
            return dto;
        }
        // Trường hợp không mong muốn, trả về null
        return null;
    }
}