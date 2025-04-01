using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace CoolWear.Views
{
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
            catch (Exception ex)
            {
                Debug.WriteLine($"FATAL ERROR: Could not resolve AccountViewModel. Ensure it's registered in ServiceManager.ConfigureServices(). Details: {ex}");
                throw; // Rethrow or handle gracefully
            }

            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Page_Loaded; // Prevent multiple loads

            if (ViewModel != null)
            {
                await ViewModel.LoadAccountInfoAsync();
            }
            else
            {
                Debug.WriteLine("ERROR: AccountViewModel is null in Page_Loaded.");
            }
        }
    }
}
