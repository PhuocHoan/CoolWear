using CoolWear.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Services;

public class MockDao : IDao
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

    public MockDao()
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

        // Add sample data
        InitializeCustomers();
    }

    private void InitializeCustomers()
    {
        CustomerRepository.AddRange(
        [
            new() { CustomerId = 1, CustomerName = "Nguyễn Văn A", Address = "Hà Nội", Phone = "0123456789" },
            new() { CustomerId = 2, CustomerName = "Trần Thị B", Address = "Hồ Chí Minh", Phone = "0987654321" },
            new() { CustomerId = 3, CustomerName = "Lê Văn C", Address = "Đà Nẵng", Phone = "0123456789" },
            new() { CustomerId = 4, CustomerName = "Phạm Thị D", Address = "Hải Phòng", Phone = "0987654321" },
            new() { CustomerId = 5, CustomerName = "Nguyễn Văn E", Address = "Hà Nội", Phone = "0123456789" }
        ]);
    }
}
