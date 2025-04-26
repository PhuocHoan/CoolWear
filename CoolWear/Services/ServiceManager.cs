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
        // Cấu hình bí mật người dùng
        AddKeyedSingleton<IConfiguration>(() => new ConfigurationBuilder()
                .AddUserSecrets<ServiceManager>()
                .Build());

        // Đăng ký PostgresDao
        try
        {
            var configuration = GetKeyedSingleton<IConfiguration>();

            string connectionString = configuration.GetConnectionString("PostgresDatabase")!;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Chuỗi kết nối 'PostgresDatabase' không được tìm thấy trong cấu hình bí mật người dùng.");
            }

            // Cấu hình DbContext
            AddKeyedSingleton(() =>
                new PooledDbContextFactory<PostgresContext>(
                new DbContextOptionsBuilder<PostgresContext>()
                .UseNpgsql(connectionString)
                .Options).CreateDbContext());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}");
            throw;
        }

        var context = GetKeyedSingleton<PostgresContext>();

        // Đăng ký Dịch vụ Điều hướng
        AddKeyedSingleton<INavigationService, NavigationService>();

        var navigationService = GetKeyedSingleton<INavigationService>();

        // Đăng ký Đơn vị công việc
        AddKeyedSingleton<IUnitOfWork>(() => new UnitOfWork(context));

        // Đăng ký LoginViewModel
        AddKeyedSingleton(() => new LoginViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký ProductViewModel
        AddKeyedSingleton(() => new ProductViewModel(GetKeyedSingleton<IUnitOfWork>(), navigationService));

        // Đăng ký CategoryViewModel
        AddKeyedSingleton(() => new CategoryViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký ColorViewModel
        AddKeyedSingleton(() => new ColorViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký SizeViewModel
        AddKeyedSingleton(() => new SizeViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký AccountViewModel
        AddKeyedSingleton(() => new AccountViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký CustomerViewModel
        AddKeyedSingleton(() => new CustomerViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký OrderViewModel
        AddKeyedSingleton(() => new OrderViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký ReportViewModel
        AddKeyedSingleton(() => new ReportViewModel(GetKeyedSingleton<IUnitOfWork>()));

        // Đăng ký SellViewModel
        AddKeyedSingleton(() => new SellViewModel(GetKeyedSingleton<IUnitOfWork>()));
    }
}
