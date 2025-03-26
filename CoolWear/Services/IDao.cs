using CoolWear.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Services;
public interface IDao
{
    IRepository<Customer> CustomerRepository { get; }
    IRepository<Product> ProductRepository { get; }
    IRepository<Order> OrderRepository { get; }
    IRepository<OrderItem> OrderItemRepository { get; }
    IRepository<ProductCategory> ProductCategoryRepository { get; }
    IRepository<ProductColor> ProductColorRepository { get; }
    IRepository<ProductSize> ProductSizeRepository { get; }
    IRepository<PaymentMethod> PaymentMethodRepository { get; }
    IRepository<ProductColorLink> ProductColorLinkRepository { get; }
    IRepository<ProductSizeLink> ProductSizeLinkRepository { get; }
    IRepository<StoreOwner> StoreOwnerRepository { get; }
}