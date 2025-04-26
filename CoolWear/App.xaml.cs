using CoolWear.Services;
using CoolWear.Views;
using Microsoft.UI.Xaml;
namespace CoolWear;

public partial class App : Application
{
    public Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        ServiceManager.ConfigureServices();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Window ban đầu là LoginWindow
        MainWindow = new LoginWindow();
        MainWindow.Activate();
    }

    public void SetMainWindow(Window window) => MainWindow = window;
}