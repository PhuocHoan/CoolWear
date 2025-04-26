using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace CoolWear.Views;

public sealed partial class AddEditProductPage : Page
{
    public AddEditProductViewModel ViewModel { get; private set; }

    public AddEditProductPage()
    {
        InitializeComponent();

        try
        {
            var unitOfWork = ServiceManager.GetKeyedSingleton<IUnitOfWork>();

            ViewModel = new AddEditProductViewModel(unitOfWork);
            DataContext = ViewModel;

            ViewModel.SaveCompleted += ViewModel_OperationCompleted;
            ViewModel.Cancelled += ViewModel_OperationCompleted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FATAL ERROR creating AddEditProductPage: {ex}");
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (ViewModel == null)
        {
            Debug.WriteLine("OnNavigatedTo: ViewModel is null, cannot proceed.");
            return;
        }

        try
        {
            await ViewModel.LoadLookupsAsync();

            if (e.Parameter is int productId && productId > 0)
            {
                await ViewModel.LoadProductAsync(productId);
            }
            else
            {
                ViewModel.SetAddMode();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR in OnNavigatedTo: {ex}");
            await ViewModelBase.ShowErrorDialogAsync("Lỗi Tải Trang", $"Không thể tải dữ liệu cho trang: {ex.Message}");
        }
    }

    private void ViewModel_OperationCompleted(object? sender, EventArgs e)
    {
        // Navigate back when save or cancel is signaled by the ViewModel
        if (Frame.CanGoBack)
        {
            Debug.WriteLine("Navigating back from AddEditProductPage.");
            Frame.GoBack();
        }
        else
        {
            Debug.WriteLine("Cannot go back from AddEditProductPage.");
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        // Unsubscribe from events to prevent memory leaks if page is cached
        if (ViewModel != null)
        {
            ViewModel.SaveCompleted -= ViewModel_OperationCompleted;
            ViewModel.Cancelled -= ViewModel_OperationCompleted;
        }
    }
}