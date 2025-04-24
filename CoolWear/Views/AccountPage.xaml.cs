using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAccountInfoAsync();
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        string oldPassword = OldPasswordBox.Password;
        string newPassword = NewPasswordBox.Password;
        string repeatPassword = RepeatPasswordBox.Password;
        bool isSuccess = await ViewModel.ChangePasswordAsync(oldPassword, newPassword, repeatPassword);
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
