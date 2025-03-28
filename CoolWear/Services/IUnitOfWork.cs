using CoolWear.Models;
using System;
using System.Threading.Tasks;

namespace CoolWear.Services;

public interface IUnitOfWork : IDisposable
{
    GenericRepository<Product> Products { get; }
    GenericRepository<Customer> Customers { get; }
    GenericRepository<Order> Orders { get; }
    Task<bool> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    new void Dispose(); // Implement IDisposable
}
