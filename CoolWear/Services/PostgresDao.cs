using CoolWear.Data;
using CoolWear.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Services;
public class PostgresDao : IDao
{
    // Use read-only auto-properties to ensure repositories can't be reassigned
    public IRepository<Customer> CustomerRepository { get; }
    public IRepository<Product> ProductRepository { get; } 
    public IRepository<Order> OrderRepository { get; }
    public IRepository<OrderItem> OrderItemRepository { get; }
    public IRepository<ProductCategory> ProductCategoryRepository { get; }
    public IRepository<ProductColor> ProductColorRepository { get; }
    public IRepository<ProductSize> ProductSizeRepository { get; }
    public IRepository<PaymentMethod> PaymentMethodRepository { get; }
    public IRepository<ProductColorLink> ProductColorLinkRepository { get; }
    public IRepository<ProductSizeLink> ProductSizeLinkRepository { get; }
    public IRepository<StoreOwner> StoreOwnerRepository { get; }

    public PostgresDao()
    {
        try
        {
            var configuration = ServiceManager.GetKeyedSingleton<IConfiguration>();

            // Update this line to use the correct path to the connection string
            string connectionString = configuration.GetConnectionString("PostgresDatabase")!;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgresDatabase' not found in configuration.");
            }

            // Configure DbContext
            ServiceManager.AddKeyedSingleton<PostgresContext>(() =>
                new PooledDbContextFactory<PostgresContext>(
                new DbContextOptionsBuilder<PostgresContext>()
                .UseNpgsql(connectionString)
                .Options).CreateDbContext());
                
            // Initialize repositories
            CustomerRepository = new DbRepository<Customer>();
            ProductRepository = new DbRepository<Product>();
            OrderRepository = new DbRepository<Order>();
            OrderItemRepository = new DbRepository<OrderItem>();
            ProductCategoryRepository = new DbRepository<ProductCategory>();
            ProductColorRepository = new DbRepository<ProductColor>();
            ProductSizeRepository = new DbRepository<ProductSize>();
            PaymentMethodRepository = new DbRepository<PaymentMethod>();
            ProductColorLinkRepository = new DbRepository<ProductColorLink>();
            ProductSizeLinkRepository = new DbRepository<ProductSizeLink>();
            StoreOwnerRepository = new DbRepository<StoreOwner>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database connection error: {ex.Message}");
            throw; // Re-throw to ensure the application knows there's a problem
        }
    }
    
    // Inner class to handle DB-specific repository implementation
    private class DbRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly PostgresContext _context;
        private readonly DbSet<TEntity> _dbSet;
        
        public DbRepository()
        {
            _context = ServiceManager.GetKeyedSingleton<PostgresContext>();
            _dbSet = _context.Set<TEntity>();
        }
        
        public void Add(TEntity entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }
        
        public void AddRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
            _context.SaveChanges();
        }
        
        public int Count()
        {
            return _dbSet.Count();
        }
        
        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
        
        public void DeleteById(int id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
                Delete(entity);
        }
        
        public IEnumerable<TEntity> Find(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }
        
        public IEnumerable<TEntity> GetAll()
        {
            return _dbSet.ToList();
        }
        
        public TEntity? GetById(int id)
        {
            return _dbSet.Find(id);
        }
        
        public void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }
        
        public void UpdateById(int id, TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
