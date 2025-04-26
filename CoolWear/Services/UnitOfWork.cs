using CoolWear.Data;
using CoolWear.Models;
using System;
using System.Threading.Tasks;

namespace CoolWear.Services;

public partial class UnitOfWork(PostgresContext context) : IUnitOfWork
{
    private bool disposed = false;
    public GenericRepository<ProductCategory> ProductCategories { get; } = new(context);
    public GenericRepository<Product> Products { get; } = new(context);
    public GenericRepository<Customer> Customers { get; } = new(context);
    public GenericRepository<Order> Orders { get; } = new(context);
    public GenericRepository<ProductColor> ProductColors { get; } = new(context);
    public GenericRepository<ProductSize> ProductSizes { get; } = new(context);
    public GenericRepository<ProductVariant> ProductVariants { get; } = new(context);
    public GenericRepository<OrderItem> OrderItems { get; } = new(context);
    public GenericRepository<PaymentMethod> PaymentMethods { get; } = new(context);
    public GenericRepository<StoreOwner> StoreOwners { get; } = new(context);


    // Xóa tất cả các tài nguyên
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Xóa tất cả các tài nguyên được quản lý ở đây
            }

            // Xóa tất cả các tài nguyên không được quản lý
            disposed = true;
            await context.DisposeAsync();
        }
    }

    public async Task<bool> SaveChangesAsync() => await context.SaveChangesAsync() > 0;

    // Bắt đầu một giao dịch
    public async Task BeginTransactionAsync() => await context.Database.BeginTransactionAsync();

    // Cam kết một giao dịch
    public async Task CommitTransactionAsync() => await context.Database.CommitTransactionAsync();

    // Hoàn tác một giao dịch
    public async Task RollbackTransactionAsync() => await context.Database.RollbackTransactionAsync();

    // Trình hủy (finalizer)
    ~UnitOfWork() => Dispose(false);
}
