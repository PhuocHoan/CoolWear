// In App.xaml.cs
using CoolWear.Services;
using CoolWear.Views;
using Microsoft.UI.Xaml;

namespace CoolWear;

public partial class App : Application
{
    // Keep the property getter
    public Window? MainWindow { get { return m_window; } }
    private Window? m_window;

    public App()
    {
        this.InitializeComponent();
        ServiceManager.ConfigureServices();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Initial window is LoginWindow
        m_window = new LoginWindow();
        m_window.Activate();
    }

    // *** ADD THIS METHOD ***
    public void SetMainWindow(Window window)
    {
        m_window = window;
        // Optional: You might want to handle closed events of the old window if needed
    }
}