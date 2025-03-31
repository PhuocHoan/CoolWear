using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FATAL ERROR: Could not resolve ProductViewModel. Ensure it's registered in ServiceManager.ConfigureServices(). Details: {ex}");
            throw; // Rethrow or handle gracefully
        }

        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe to prevent potential multiple loads if page is re-navigated to
        // without being destroyed and reconstructed.
        Loaded -= Page_Loaded;

        if (ViewModel != null)
        {
            await ViewModel.LoadProductsAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: ViewModel is null in Page_Loaded.");
        }
    }
}