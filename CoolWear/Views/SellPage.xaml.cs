using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace CoolWear.Views;

public sealed partial class SellPage : Page
{
    public SellViewModel ViewModel { get; private set; }

    public SellPage()
    {
        InitializeComponent();

        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<SellViewModel>();
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing SellPage: {ex.Message}");
        }
    }

    private void VariantIdClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int variantId)
        {
            Debug.WriteLine($"[DEBUG] Clicked VariantId: {variantId}");
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine("Sell: OnNavigatedTo");

        if (ViewModel != null)
        {
            if (ViewModel.IsLoading)
            {
                return;
            }

            await ViewModel.InitializeAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: ViewModel is null in OnNavigatedTo.");
        }
    }
}
