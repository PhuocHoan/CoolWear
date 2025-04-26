using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace CoolWear.Converters;

public partial class ZeroIsCollapsedConverter : IValueConverter
{
    // Chuyển đổi count (int, bool hoặc collection count) sang Visibility
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isZero = false;
        if (value is int i)
        {
            isZero = i == 0;
        }
        else if (value is bool b)
        {
            isZero = b == false;
        }
        else if (value is ICollection c) // Check nếu nó là collection
        {
            isZero = c.Count == 0;
        }

        return isZero ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public partial class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isGreaterThanZero = false;

        if (value is int intValue)
        {
            isGreaterThanZero = intValue > 0;
        }

        return isGreaterThanZero ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException("Cannot convert Visibility back to an integer count.");
}

public partial class InverseBoolConverter : IValueConverter
{
    /// <summary>
    /// Chuyển đổi giá trị boolean gốc sang giá trị boolean ngược lại.
    /// </summary>
    /// <returns>Giá trị boolean đã đảo ngược, hoặc false nếu đầu vào không phải bool.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Kiểm tra xem giá trị đầu vào có phải là kiểu bool không
        if (value is bool b)
        {
            // Nếu là bool, trả về giá trị phủ định (ngược lại)
            return !b;
        }
        // Nếu đầu vào không phải bool, trả về false làm giá trị mặc định an toàn
        return false;
    }

    /// <summary>
    /// Chuyển đổi giá trị boolean đã đảo ngược trở lại giá trị gốc.
    /// </summary>
    /// <returns>Giá trị boolean gốc, hoặc false nếu đầu vào không phải bool.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // Kiểm tra xem giá trị đầu vào có phải là kiểu bool không
        if (value is bool b)
        {
            // Nếu là bool, trả về giá trị phủ định (ngược lại) một lần nữa để về gốc
            return !b;
        }
        // Nếu đầu vào không phải bool, trả về false
        return false;
    }
}