using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace CoolWear.Views;

// AccountPage.xaml.cs
public sealed partial class AccountPage : Page
{
    public AccountViewModel ViewModel { get; }

    public AccountPage()
    {
        InitializeComponent();
        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<AccountViewModel>();
        }
        catch (Exception) { }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel != null)
        {
            await ViewModel.LoadAccountInfoAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: AccountViewModel is null in OnNavigatedTo.");
        }
    }
}
