using CoolWear.Data;
using CoolWear.Model;
using CoolWear.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.UI.Xaml;
using System;
using System.Configuration;
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

        // Configure user secrets
        ServiceManager.AddKeyedSingleton<IConfiguration>(() =>
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<App>()
                .Build();
        });

        // Config - Dependency injection => Inversion of control
        ServiceManager.AddKeyedSingleton<IDao, PostgresDao>(); // TextDao, PostgresDao, SqlServerDao, RestDao, GraphQLDao
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private Window? m_window;
}
