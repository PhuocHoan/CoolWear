using Microsoft.UI.Xaml.Controls;
using System;

namespace CoolWear.Services;

public interface INavigationService
{
    bool Navigate(Type sourcePageType);
    bool Navigate(Type sourcePageType, object parameter);
    bool GoBack();
    Frame? AppFrame { get; set; } // Property to hold the main frame
}