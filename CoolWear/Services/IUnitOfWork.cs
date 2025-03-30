using CoolWear.Models;
using System;
using System.Threading.Tasks;

namespace CoolWear.Services;

public interface IUnitOfWork : IDisposable
{
    GenericRepository<Product> Products { get; }
    GenericRepository<Customer> Customers { get; }
    GenericRepository<Order> Orders { get; }
    GenericRepository<ProductCategory> ProductCategories { get; }
    GenericRepository<ProductColor> ProductColors { get; }
    GenericRepository<ProductSize> ProductSizes { get; }
    GenericRepository<ProductVariant> ProductVariants { get; }
    GenericRepository<OrderItem> OrderItems { get; }
    GenericRepository<PaymentMethod> PaymentMethods { get; }
    GenericRepository<StoreOwner> StoreOwners { get; }
    Task<bool> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    new void Dispose(); // Implement IDisposable
}
