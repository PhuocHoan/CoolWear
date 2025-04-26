using CoolWear.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Linq;

namespace CoolWear.Views;

public sealed partial class DashboardWindow : Window
{
    private readonly INavigationService _navigationService;

    public DashboardWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        _navigationService = ServiceManager.GetKeyedSingleton<INavigationService>();
        _navigationService.AppFrame = container; // Assign the Frame control

        navigation.SelectedItem = navigation.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        Navigate(navigation.SelectedItem as NavigationViewItem);
    }

    private void Navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var invokedItem = args.InvokedItemContainer as NavigationViewItem;
        if (invokedItem?.Tag == null) return;
        string tag = invokedItem.Tag.ToString() ?? string.Empty;

        if (tag == "Logout") { PerformLogout(); }
        else { Navigate(invokedItem); }
    }

    private void Navigate(NavigationViewItem? item)
    {
        if (item?.Tag == null) return;

        string tag = item.Tag.ToString()!;
        string pageTypeName = $"{GetType().Namespace}.{tag}"; // Assumes Views namespace
        Type? pageType = Type.GetType(pageTypeName);

        if (pageType != null)
        {
            _navigationService.Navigate(pageType); // Use service
        }
        else
        {
            Debug.WriteLine($"Error: Page type '{pageTypeName}' not found for tag '{tag}'.");
        }
    }

    private void PerformLogout()
    {
        var loginWindow = new LoginWindow();
        if (Application.Current is App app)
        {
            app.SetMainWindow(loginWindow);
        }
        loginWindow.Activate();

        Close(); // Close the current DashboardWindow
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) { }
}
