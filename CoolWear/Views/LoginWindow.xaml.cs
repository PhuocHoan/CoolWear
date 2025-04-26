using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;

namespace CoolWear.Views;

public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }

    public LoginWindow()
    {
        InitializeComponent();
        ViewModel = ServiceManager.GetKeyedSingleton<LoginViewModel>();
        _ = ViewModel.InitializeDataAsync();
        // Set the title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }
}
