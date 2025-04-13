using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using CoolWear.ViewModels;
using CoolWear.Services;
using Microsoft.UI.Xaml.Navigation;
using CoolWear.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace CoolWear.Views
{
    public sealed partial class SellPage : Page
    {
        // Add this property to the SellPage class
        private IComponentConnector Bindings { get; set; }

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
}
