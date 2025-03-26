using CoolWear.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Services;

public class TextDao : IDao
{
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
    public TextDao()
    {
        // Initialize all repositories
        CustomerRepository = new Repository<Customer>();
        ProductRepository = new Repository<Product>();
        OrderRepository = new Repository<Order>();
        OrderItemRepository = new Repository<OrderItem>();
        ProductCategoryRepository = new Repository<ProductCategory>();
        ProductColorRepository = new Repository<ProductColor>();
        ProductSizeRepository = new Repository<ProductSize>();
        PaymentMethodRepository = new Repository<PaymentMethod>();
        ProductColorLinkRepository = new Repository<ProductColorLink>();
        ProductSizeLinkRepository = new Repository<ProductSizeLink>();
        StoreOwnerRepository = new Repository<StoreOwner>();

        InitializeData("Customers.txt", CustomerRepository);
    }

    private static void InitializeData<T>(string file, IRepository<T> repository) where T : class, new()
    {
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(folder, file);

        if (!File.Exists(path))
            return;

        string[] lines = File.ReadAllLines(path);

        // Skip header line if it exists
        var dataLines = lines.Length > 1 ? lines.Skip(1) : lines;

        // Get property names from first line if it's a header
        string[] headers = lines[0].Split(',');
        var properties = typeof(T).GetProperties();

        foreach (string line in dataLines)
        {
            string[] values = line.Split(',');

            // Create new entity
            T entity = new();

            // Map values to properties
            for (int i = 0; i < Math.Min(values.Length, headers.Length); i++)
            {
                var property = properties.FirstOrDefault(p =>
                    p.Name.Equals(headers[i], StringComparison.OrdinalIgnoreCase));

                if (property != null && values[i] != null)
                {
                    object convertedValue = Convert.ChangeType(values[i], property.PropertyType);
                    property.SetValue(entity, convertedValue);
                }
            }

            repository.Add(entity);
        }
    }
}
