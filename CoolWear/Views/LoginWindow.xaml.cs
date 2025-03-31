// In LoginWindow.xaml.cs
using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;

namespace CoolWear.Views;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; } = ServiceManager.GetKeyedSingleton<LoginViewModel>();

    public LoginWindow()
    {
        InitializeComponent();

        // Set the title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }
}
