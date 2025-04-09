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
            dateTimeInput = dt;
        }
        else if (value is DateTimeOffset dto)
        {
            // Nếu là DateTimeOffset, lấy giờ Local hoặc Utc tùy nhu cầu
            dateTimeInput = dto.LocalDateTime; // Lấy giờ local từ Offset
        }
        else
        {
            return string.Empty;
        }

        DateTime dateTimeToFormat;

        // --- XỬ LÝ CHUYỂN ĐỔI MÚI GIỜ ---
        switch (dateTimeInput.Kind)
        {
            case DateTimeKind.Utc:
                // Nếu Kind là UTC, chuyển sang Local
                dateTimeToFormat = dateTimeInput.ToLocalTime();
                break;
            case DateTimeKind.Local:
                // Nếu Kind đã là Local, dùng luôn
                dateTimeToFormat = dateTimeInput;
                break;
            case DateTimeKind.Unspecified:
            default:
                // *** Giả định quan trọng ***: Nếu Kind là Unspecified (khi đọc từ DB 'timestamp without time zone')
                // thì giá trị này thực chất đang lưu giờ UTC. Cần chuyển nó sang Local.
                // Để làm điều này, trước tiên phải chỉ định nó là UTC.
                var utcDateTime = DateTime.SpecifyKind(dateTimeInput, DateTimeKind.Utc);
                dateTimeToFormat = utcDateTime.ToLocalTime();
                break;
        }
        // --- KẾT THÚC XỬ LÝ MÚI GIỜ ---


        string formatString = parameter as string ?? this.Format;

        try
        {
            // Định dạng DateTime đã được chuyển sang Local
            return dateTimeToFormat.ToString(formatString, CultureInfo.CurrentCulture);
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"Lỗi định dạng ngày tháng: {ex.Message}. Format='{formatString}'");
            return value.ToString() ?? string.Empty; // Trả về giá trị gốc nếu lỗi format
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException("Chuyển đổi ngược từ chuỗi sang DateTime chưa được hỗ trợ.");
}