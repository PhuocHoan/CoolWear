using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace CoolWear.Services;

public class NavigationService : INavigationService
{
    public Frame? AppFrame { get; set; } // Set this from DashboardWindow

    public bool Navigate(Type sourcePageType)
    {
        if (AppFrame != null && AppFrame.CurrentSourcePageType != sourcePageType)
        {
            return AppFrame.Navigate(sourcePageType);
        }
        Debug.WriteLine($"NavigationService: Navigate failed. Frame is null or already on page {sourcePageType.Name}.");
        return false;
    }

    public bool Navigate(Type sourcePageType, object parameter)
    {
        if (AppFrame != null) // Allow re-navigating to same page with different param
        {
            return AppFrame.Navigate(sourcePageType, parameter);
        }
        Debug.WriteLine($"NavigationService: Navigate with parameter failed. Frame is null.");
        return false;
    }

    public bool GoBack()
    {
        if (AppFrame != null && AppFrame.CanGoBack)
        {
            AppFrame.GoBack();
            return true;
        }
        Debug.WriteLine("NavigationService: GoBack failed. Frame is null or cannot go back.");
        return false;
    }
}