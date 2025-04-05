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
    /// Raise the PropertyChanged event for the specified property.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Set the specified property and raise the PropertyChanged event if the value has changed.
    /// </summary>
    /// <returns>true if the value was changed, false otherwise.</returns>
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
    /// Get the XamlRoot for ContentDialogs
    /// </summary>
    /// <returns>The XamlRoot for ContentDialogs</returns>
    protected static XamlRoot? GetXamlRootForDialogs()
    {
        if (Application.Current is App app && app.MainWindow?.Content is FrameworkElement rootElement)
        {
            return rootElement.XamlRoot;
        }
        Debug.WriteLine("ERROR: Could not obtain XamlRoot for ContentDialog. App, MainWindow, or MainWindow.Content might be null/invalid.");
        return null;
    }

    /// <summary>
    /// Show a confirmation dialog with the specified title and content.
    /// </summary>
    /// <returns>The result of the dialog</returns>
    protected static async Task<ContentDialogResult> ShowConfirmationDialogAsync(
        string title, string content, string primaryText = "OK", string closeText = "Cancel")
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return ContentDialogResult.None; // Cannot show dialog

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
    /// Show an error dialog with the specified title and message.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
    /// Show a dialog indicating that the specified feature is not implemented.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
    /// Show a success dialog with the specified title and message.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
