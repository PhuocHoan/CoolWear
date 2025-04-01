using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FATAL ERROR: Could not resolve SizeViewModel. Ensure it's registered in ServiceManager.ConfigureServices(). Details: {ex}");
            throw; // Rethrow or handle gracefully
        }

        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= Page_Loaded; // Prevent multiple loads

        if (ViewModel != null)
        {
            await ViewModel.LoadSizesAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: SizeViewModel is null in Page_Loaded.");
        }
    }
}
