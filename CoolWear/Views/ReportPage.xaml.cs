using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace CoolWear.Views;

public sealed partial class ReportPage : Page
{
    public ReportViewModel ViewModel { get; }

    public ReportPage()
    {
        InitializeComponent();
        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<ReportViewModel>();
        }
        catch (Exception) { }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel != null && !ViewModel.IsLoading)
        {
            // Gọi hàm tải dữ liệu chính khi trang được điều hướng đến
            await ViewModel.LoadReportDataAsync();
        }
    }

    /// <summary>
    /// Xử lý khi lựa chọn trên RadioButtons thay đổi.
    /// </summary>
    private void PeriodRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Kiểm tra xem có phải RadioButtons và ViewModel có tồn tại không
        if (sender is RadioButtons radioButtons && ViewModel != null)
        {
            // Kiểm tra xem SelectedItem có phải là kiểu ReportPeriod hợp lệ không
            if (radioButtons.SelectedItem is ReportPeriod selectedPeriodValue)
            {
                if (ViewModel.SelectedPeriod != selectedPeriodValue)
                {
                    ViewModel.SelectedPeriod = selectedPeriodValue;
                }
            }
            else
            {
                Debug.WriteLine("Lỗi: SelectedItem của RadioButtons không phải là ReportPeriod hợp lệ.");
            }
        }
    }
}