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
public sealed partial class SizesPage : Page
{
    public SizeViewModel ViewModel { get; }

    public SizesPage()
    {
        InitializeComponent();

        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<SizeViewModel>();
            DataContext = ViewModel;
            ViewModel.RequestShowDialog += ShowAddEditSizeDialogAsync;
        }
        catch (Exception) { }
    }

    // Hủy đăng ký khi rời khỏi trang để tránh memory leak
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel != null)
        {
            ViewModel.RequestShowDialog -= ShowAddEditSizeDialogAsync;
        }
    }

    // Tải dữ liệu ban đầu khi điều hướng đến trang
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine("SizesPage: OnNavigatedTo");

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
    private async Task<ContentDialogResult> ShowAddEditSizeDialogAsync()
    {
        // Đảm bảo XamlRoot được thiết lập
        AddEditSizeDialog.XamlRoot = this.XamlRoot;

        // Hiển thị dialog và chờ kết quả
        ContentDialogResult result = await AddEditSizeDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Gọi hàm lưu trong ViewModel
            _ = await ViewModel.SaveSizeAsync();
        }

        return result;
    }
}
