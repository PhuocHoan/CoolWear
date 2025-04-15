using CoolWear.Data;
using CoolWear.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoolWear.Services;

public sealed class ServiceManager
{
    private static readonly Dictionary<string, object> _singletons = [];
    public static void AddKeyedSingleton<IParent, Child>()
    {
        Type parent = typeof(IParent);
        Type child = typeof(Child);
        _singletons[parent.Name] = Activator.CreateInstance(child)!;
    }

    public static void AddKeyedSingleton<IService>(Func<IService> factory)
    {
        Type serviceType = typeof(IService);
        _singletons[serviceType.Name] = factory()!;
    }

    public static IParent GetKeyedSingleton<IParent>()
    {
        Type parent = typeof(IParent);
        return (IParent)_singletons[parent.Name];
    }

    public static void ConfigureServices()
    {
        // Configure user secrets
        AddKeyedSingleton<IConfiguration>(() => new ConfigurationBuilder()
                .AddUserSecrets<ServiceManager>()
                .Build());

        // Register PostgresDao
        try
        {
            var configuration = GetKeyedSingleton<IConfiguration>();

            string connectionString = configuration.GetConnectionString("PostgresDatabase")!;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgresDatabase' not found in user secrets configuration.");
            }

            // Configure DbContext
            AddKeyedSingleton(() =>
                new PooledDbContextFactory<PostgresContext>(
                new DbContextOptionsBuilder<PostgresContext>()
                .UseNpgsql(connectionString)
                .Options).CreateDbContext());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database connection error: {ex.Message}");
            throw;
        }

        var context = GetKeyedSingleton<PostgresContext>();

        // Register Navigation Service
        AddKeyedSingleton<INavigationService, NavigationService>();

        var navigationService = GetKeyedSingleton<INavigationService>();

        // Register UnitOfWork
        AddKeyedSingleton<IUnitOfWork>(() => new UnitOfWork(context));

        // Register LoginViewModel
        AddKeyedSingleton(() => new LoginViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register ProductViewModel
        AddKeyedSingleton(() => new ProductViewModel(GetKeyedSingleton<IUnitOfWork>(), navigationService));

        // Register CategoryViewModel
        AddKeyedSingleton(() => new CategoryViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register ColorViewModel
        AddKeyedSingleton(() => new ColorViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register SizeViewModel
        AddKeyedSingleton(() => new SizeViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register AccountViewModel
        AddKeyedSingleton(() => new AccountViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register CustomerViewModel
        AddKeyedSingleton(() => new CustomerViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register OrderViewModel
        AddKeyedSingleton(() => new OrderViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Register ReportViewModel
        AddKeyedSingleton(() => new ReportViewModel(GetKeyedSingleton<IUnitOfWork>()));
    }
}
