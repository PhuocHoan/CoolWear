using CoolWear.Services;
using CoolWear.View;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;

namespace CoolWear;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        // Configure services
        ServiceManager.ConfigureServices();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {

        m_window = new LoginWindow();
        m_window.Activate();
    }

    private Window? m_window;
}
