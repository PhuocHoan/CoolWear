using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace CoolWear.Views;

public sealed partial class ProductsPage : Page
{
    public ProductViewModel ViewModel { get; }

    public ProductsPage()
    {
        InitializeComponent();
        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<ProductViewModel>();
            DataContext = ViewModel;
        }
        catch (Exception) { }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine("ProductsPage: OnNavigatedTo");

        if (ViewModel != null)
        {
            if (ViewModel.IsLoading) // Prevent re-entrancy
            {
                return;
            }

            await ViewModel.InitializeDataAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: ViewModel is null in OnNavigatedTo.");
        }
    }
}