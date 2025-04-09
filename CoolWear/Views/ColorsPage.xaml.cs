using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoolWear.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ColorsPage : Page
{
    public ColorViewModel ViewModel { get; }

    public ColorsPage()
    {
        InitializeComponent();

        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<ColorViewModel>();
            DataContext = ViewModel;
            ViewModel.RequestShowDialog += ShowAddEditColorDialogAsync;
        }
        catch (Exception) { }
    }

    // Hủy đăng ký khi rời khỏi trang để tránh memory leak
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel != null)
        {
            ViewModel.RequestShowDialog -= ShowAddEditColorDialogAsync;
        }
    }

    // Tải dữ liệu ban đầu khi điều hướng đến trang
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine("ColorsPage: OnNavigatedTo");

        if (ViewModel != null && !ViewModel.IsLoading)
        {
            await ViewModel.InitializeDataAsync();
        }
        else if (ViewModel == null)
        {
            Debug.WriteLine("LỖI: ViewModel bị null trong OnNavigatedTo.");
        }
    }

    /// <summary>
    /// Phương thức được gọi bởi ViewModel để hiển thị ContentDialog Add/Edit.
    /// </summary>
    /// <returns>Kết quả người dùng nhấn nút (Primary, Secondary, None).</returns>
    private async Task<ContentDialogResult> ShowAddEditColorDialogAsync()
    {
        // Đảm bảo XamlRoot được thiết lập
        AddEditColorDialog.XamlRoot = this.XamlRoot;

        // Hiển thị dialog và chờ kết quả
        ContentDialogResult result = await AddEditColorDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Gọi hàm lưu trong ViewModel
            _ = await ViewModel.SaveColorAsync();
        }

        return result;
    }
}
