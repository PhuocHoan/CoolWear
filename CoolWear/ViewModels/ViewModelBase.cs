using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Kích hoạt sự kiện PropertyChanged cho thuộc tính được chỉ định.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Đặt thuộc tính được chỉ định và kích hoạt sự kiện PropertyChanged nếu giá trị đã thay đổi.
    /// </summary>
    /// <returns>true nếu giá trị đã thay đổi, false nếu không.</returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null,
        Action? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        onChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Lấy XamlRoot cho ContentDialogs
    /// </summary>
    /// <returns>XamlRoot cho ContentDialogs</returns>
    protected static XamlRoot? GetXamlRootForDialogs()
    {
        if (Application.Current is App app && app.MainWindow?.Content is FrameworkElement rootElement)
        {
            return rootElement.XamlRoot;
        }
        Debug.WriteLine("ERROR: Không thể lấy XamlRoot cho ContentDialog. App, MainWindow, hoặc MainWindow.Content có thể null/không hợp lệ.");
        return null;
    }

    /// <summary>
    /// Hiển thị hộp thoại xác nhận với tiêu đề và nội dung được chỉ định.
    /// </summary>
    /// <returns>Kết quả của hộp thoại</returns>
    protected static async Task<ContentDialogResult> ShowConfirmationDialogAsync(
        string title, string content, string primaryText = "OK", string closeText = "Cancel")
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return ContentDialogResult.None; // Không thể hiển thị hộp thoại

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryText,
            CloseButtonText = closeText,
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };
        return await dialog.ShowAsync();
    }

    /// <summary>
    /// Hiển thị hộp thoại lỗi với tiêu đề và thông báo được chỉ định.
    /// </summary>
    /// <returns>Một tác vụ đại diện cho hoạt động không đồng bộ.</returns>
    public static async Task ShowErrorDialogAsync(string title, string message)
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "Đóng",
            XamlRoot = xamlRoot
        };
        await dialog.ShowAsync();
    }

    /// <summary>
    /// Hiển thị hộp thoại cho biết tính năng được chỉ định chưa được triển khai.
    /// </summary>
    /// <returns>Một tác vụ đại diện cho hoạt động không đồng bộ.</returns>
    protected static async Task ShowNotImplementedDialogAsync(string feature)
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = "Chức Năng Chưa Sẵn Sàng",
            Content = $"Chức năng '{feature}' đang được phát triển.",
            CloseButtonText = "Đóng",
            XamlRoot = xamlRoot
        };
        await dialog.ShowAsync();
    }

    /// <summary>
    /// Hiển thị hộp thoại thành công với tiêu đề và thông báo được chỉ định.
    /// </summary>
    /// <returns>Một tác vụ đại diện cho hoạt động không đồng bộ.</returns>
    protected static async Task ShowSuccessDialogAsync(string title, string message)
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return;
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "Đóng",
            XamlRoot = xamlRoot
        };
        await dialog.ShowAsync();
    }
}
