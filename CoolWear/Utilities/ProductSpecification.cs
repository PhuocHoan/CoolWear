using CoolWear.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CoolWear.Utilities;

public class ProductSpecification : GenericSpecification<Product>
{
    /// <summary>
    /// Creates a default specification for products.
    /// </summary>
    public ProductSpecification() : base() { }

    /// <summary>
    /// Creates a specification for products with a specific category name.
    /// </summary>
    public ProductSpecification WithCategoryName(string categoryName)
    {
        if (!string.IsNullOrEmpty(categoryName))
        {
            AddCriteria(p => p.Category.CategoryName == categoryName);
        }
        return this;
    }

    /// <summary>
    /// Adds a search by product name.
    /// </summary>
    public ProductSpecification SearchByIdOrName(string searchTerm)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (int.TryParse(searchTerm, out int id))
            {
                AddCriteria(p => p.ProductName.Contains(searchTerm) || p.ProductId == id);
            }
            else AddCriteria(p => p.ProductName.Contains(searchTerm));
        }
        return this;
    }

    /// <summary>
    /// Includes category data in the query results.
    /// </summary>
    public ProductSpecification IncludeCategory()
    {
        AddInclude(p => p.Category);
        return this;
    }

    /// <summary>
    /// Includes color data in the query results.
    /// </summary>
    public ProductSpecification IncludeColors()
    {
        AddInclude(p => p.ProductVariants.Select(v => v.Color));
        return this;
    }

    /// <summary>
    /// Includes size data in the query results.
    /// </summary>
    public ProductSpecification IncludeSizes()
    {
        AddInclude(p => p.ProductVariants.Select(v => v.Size));
        return this;
    }
}
