using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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

        // Optional: Select the first item on startup
        // navigation.SelectedItem = navigation.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        // Navigate(navigation.SelectedItem as NavigationViewItem); // Call helper method
    }

    private void Navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        // Settings item is hidden, so no need for IsSettingsInvoked check unless you re-enable it

        var invokedItem = args.InvokedItemContainer as NavigationViewItem;
        if (invokedItem?.Tag == null) // Ensure item and tag exist
        {
            return;
        }

        string tag = invokedItem.Tag.ToString() ?? string.Empty;

        // *** HANDLE LOGOUT TAG ***
        if (tag == "Logout")
        {
            PerformLogout(); // Call a separate method for logout logic
        }
        // *** HANDLE NAVIGATION TAGS ***
        else
        {
            Navigate(invokedItem); // Navigate to the page based on the tag
        }
    }

    // Helper method for navigation
    private void Navigate(NavigationViewItem? item)
    {
        if (item?.Tag == null) return;

        string tag = item.Tag.ToString()!;
        string pageTypeName = $"{GetType().Namespace}.{tag}"; // Assumes pages are in the same namespace as the window
        Type? pageType = Type.GetType(pageTypeName);

        if (pageType != null)
        {
            // Navigate only if the page is different from the current one
            if (container.CurrentSourcePageType != pageType)
            {
                container.Navigate(pageType);
            }
        }
        else
        {
            Debug.WriteLine($"Error: Page type '{pageTypeName}' not found for tag '{tag}'.");
            // Optionally show an error to the user or navigate to an error page
        }
    }


    private void PerformLogout()
    {
        Debug.WriteLine("Logout Invoked!");
        // --- Add your actual logout logic here ---
        // 1. Clear any stored credentials (if applicable)
        //    var localStorage = Windows.Storage.ApplicationData.Current.LocalSettings;
        //    localStorage.Values.Remove("Username");
        //    localStorage.Values.Remove("Password");
        //    localStorage.Values.Remove("Entropy");
        // 2. Navigate back to the LoginWindow
        var loginWindow = new LoginWindow(); // Create a new instance
        if (Application.Current is App app)
        {
            app.SetMainWindow(loginWindow); // IMPORTANT: Update the App's main window reference
        }
        loginWindow.Activate();

        // 3. Close the current DashboardWindow
        this.Close();
    }


    // SelectionChanged might be used for updating title or other UI elements,
    // but navigation should primarily happen in ItemInvoked.
    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // Example: Update window title based on selection (optional)
        // if (args.SelectedItem is NavigationViewItem selectedItem && selectedItem.Content != null)
        // {
        //     this.Title = $"CoolWear - {selectedItem.Content}";
        // }
    }
}
