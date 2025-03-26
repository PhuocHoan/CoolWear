using CoolWear.Data;
using CoolWear.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CoolWear.Services;

/// <summary>
/// Unit of Work implementation to manage data services and transaction handling
/// </summary>
public class UnitOfWork : IDisposable
{
    private readonly PostgresContext _context;
    private bool _disposed = false;

    // Data services
    public IDataService<Customer> CustomerService { get; }
    public IDataService<Product> ProductService { get; }
    public IDataService<Order> OrderService { get; }
    public IDataService<OrderItem> OrderItemService { get; }
    public IDataService<ProductCategory> CategoryService { get; }
    public IDataService<ProductColor> ColorService { get; }
    public IDataService<ProductSize> SizeService { get; }
    public IDataService<ProductColorLink> ColorLinkService { get; }
    public IDataService<ProductSizeLink> SizeLinkService { get; }
    public IDataService<PaymentMethod> PaymentMethodService { get; }
    public IDataService<StoreOwner> StoreOwnerService { get; }

    public UnitOfWork(PostgresContext context)
    {
        _context = context;

        // Initialize data services with the same context
        CustomerService = new PostgresDataService<Customer>(_context);
        ProductService = new PostgresDataService<Product>(_context);
        OrderService = new PostgresDataService<Order>(_context);
        OrderItemService = new PostgresDataService<OrderItem>(_context);
        CategoryService = new PostgresDataService<ProductCategory>(_context);
        ColorService = new PostgresDataService<ProductColor>(_context);
        SizeService = new PostgresDataService<ProductSize>(_context);
        ColorLinkService = new PostgresDataService<ProductColorLink>(_context);
        SizeLinkService = new PostgresDataService<ProductSizeLink>(_context);
        PaymentMethodService = new PostgresDataService<PaymentMethod>(_context);
        StoreOwnerService = new PostgresDataService<StoreOwner>(_context);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    // Begin a transaction
    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    // Commit a transaction
    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    // Rollback a transaction
    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    // Dispose pattern implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
