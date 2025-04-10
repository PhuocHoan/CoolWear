using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace CoolWear.Views;

public sealed partial class OrdersPage : Page
{
    public OrderViewModel ViewModel { get; }

    public OrdersPage()
    {
        InitializeComponent();
        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<OrderViewModel>();
            DataContext = ViewModel;
            ViewModel.RequestShowDialog += ShowEditOrderStatusDialogAsync;
        }
        catch (Exception) { }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel != null && !ViewModel.IsLoading)
        {
            await ViewModel.InitializeDataAsync();
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel != null)
        {
            ViewModel.RequestShowDialog -= ShowEditOrderStatusDialogAsync; // Hủy đăng ký
        }
    }

    /// <summary>
    /// Hiển thị dialog chỉnh sửa trạng thái đơn hàng.
    /// </summary>
    private async Task<ContentDialogResult> ShowEditOrderStatusDialogAsync()
    {
        // Đảm bảo XamlRoot được thiết lập
        EditOrderStatusDialog.XamlRoot = this.XamlRoot;

        // Hiển thị dialog và chờ kết quả
        ContentDialogResult result = await EditOrderStatusDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Gọi hàm lưu trong ViewModel
            _ = await ViewModel.SaveOrderStatusAsync();
        }

        return result;
    }
}