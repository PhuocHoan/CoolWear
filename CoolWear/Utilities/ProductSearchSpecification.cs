using CoolWear.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Utilities;

public class ProductSearchSpecification : ISpecification<Product>
{
    public Expression<Func<Product, bool>>? Filter { get; }
    public Func<IQueryable<Product>, IOrderedQueryable<Product>>? OrderBy { get; }
    public int? PageNumber { get; }
    public int? PageSize { get; }

    public ProductSearchSpecification(
        string searchTerm,
        int pageNumber,
        int pageSize
    )
    {
        Filter = p => p.ProductName.Contains(searchTerm) && p.StockQuantity > 0;
        OrderBy = q => q.OrderBy(p => p.Price);
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}