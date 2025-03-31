using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace CoolWear.Views;

public sealed partial class CategoriesPage : Page
{
    public CategoryViewModel ViewModel { get; }

    public CategoriesPage()
    {
        InitializeComponent();

        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<CategoryViewModel>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FATAL ERROR: Could not resolve CategoryViewModel. Ensure it's registered in ServiceManager.ConfigureServices(). Details: {ex}");
            throw; // Rethrow or handle gracefully
        }

        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= Page_Loaded; // Prevent multiple loads

        if (ViewModel != null)
        {
            await ViewModel.LoadCategoriesAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: CategoryViewModel is null in Page_Loaded.");
        }
    }
}