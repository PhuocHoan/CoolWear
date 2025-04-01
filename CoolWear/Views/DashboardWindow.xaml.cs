using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Linq;

namespace CoolWear.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DashboardWindow : Window
{
    public DashboardWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Set the initial page
        navigation.SelectedItem = navigation.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        Navigate(navigation.SelectedItem as NavigationViewItem);
    }

    private void Navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var invokedItem = args.InvokedItemContainer as NavigationViewItem;
        if (invokedItem?.Tag == null)
        {
            return;
        }

        string tag = invokedItem.Tag.ToString() ?? string.Empty;

        if (tag == "Logout")
        {
            PerformLogout();
        }
        else
        {
            Navigate(invokedItem);
        }
    }

    private void Navigate(NavigationViewItem? item)
    {
        if (item?.Tag == null) return;

        string tag = item.Tag.ToString()!;
        string pageTypeName = $"{GetType().Namespace}.{tag}";
        Type? pageType = Type.GetType(pageTypeName);

        if (pageType != null)
        {
            if (container.CurrentSourcePageType != pageType)
            {
                container.Navigate(pageType);
            }
        }
        else
        {
            Debug.WriteLine($"Error: Page type '{pageTypeName}' not found for tag '{tag}'.");
        }
    }


    private void PerformLogout()
    {
        Debug.WriteLine("Logout Invoked!");
        var loginWindow = new LoginWindow();
        if (Application.Current is App app)
        {
            app.SetMainWindow(loginWindow);
        }
        loginWindow.Activate();

        Close(); // Close the current DashboardWindow
    }


    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
    }
}
