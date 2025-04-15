using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics;
using System.Globalization;

namespace CoolWear.Converters;
/// <summary>
/// Value Converter để định dạng giá trị DateTime hoặc DateTimeOffset thành chuỗi theo một format nhất định.
/// </summary>
public partial class DateTimeFormatConverter : IValueConverter
{
    public string Format { get; set; } = "dd/MM/yyyy HH:mm"; // Format mặc định

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        DateTime dateTimeInput;

        // Xác định DateTime đầu vào
        if (value is DateTime dt)
        {
            dateTimeInput = dt.ToLocalTime();
        }
        else
        {
            return string.Empty;
        }

        try
        {
            // Định dạng DateTime đã được chuyển sang Local
            return dateTimeInput.ToString(Format, CultureInfo.CurrentCulture);
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"Lỗi định dạng ngày tháng: {ex.Message}. Format='{Format}'");
            return value.ToString() ?? string.Empty; // Trả về giá trị gốc nếu lỗi format
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException("Chuyển đổi ngược từ chuỗi sang DateTime chưa được hỗ trợ.");
}