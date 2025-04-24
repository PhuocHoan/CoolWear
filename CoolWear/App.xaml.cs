using CoolWear.Services;
using CoolWear.Views;
using Microsoft.UI.Xaml;
using OfficeOpenXml;
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
        // Initial window is LoginWindow
        MainWindow = new LoginWindow();
        MainWindow.Activate();
    }

    public void SetMainWindow(Window window) => MainWindow = window;// optional: might want to handle closed events of the old window if needed
}