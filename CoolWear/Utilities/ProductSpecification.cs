using CoolWear.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CoolWear.Utilities;

public class ProductSpecification : GenericSpecification<Product>
{
    public ProductSpecification() =>
        AddCriteria(p => !p.IsDeleted);

    public ProductSpecification(
        string? searchTerm = null,
        int? categoryId = null,
        int? colorId = null,
        int? sizeId = null,
        bool? inStockOnly = null, // true = in stock, false = out of stock, null = all
        int? skip = null,
        int? take = null,
        bool includeDetails = true)
    {
        AddCriteria(p => !p.IsDeleted); // Exclude deleted products

        if (includeDetails)
        {
            IncludeCategory();
            IncludeVariantsWithDetails();
        }

        // Apply Filters based on constructor parameters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            SearchByIdOrName(searchTerm);
        }
        if (categoryId.HasValue)
        {
            WhereCategoryIs(categoryId.Value);
        }
        if (colorId.HasValue)
        {
            HasColor(colorId.Value);
        }
        if (sizeId.HasValue)
        {
            HasSize(sizeId.Value);
        }
        if (inStockOnly.HasValue)
        {
            if (inStockOnly.Value)
            {
                IsInStock();
            }
            else
            {
                IsOutOfStock();
            }
        }

        // Apply Paging if requested
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }

    /// <summary>
    /// Includes category data using string-based include.
    /// </summary>
    public ProductSpecification IncludeCategory()
    {
        AddInclude(nameof(Product.Category)); // Pass "Category" string
        return this;
    }

    /// <summary>
    /// Includes variants and their related Color and Size using string-based includes.
    /// </summary>
    public ProductSpecification IncludeVariantsWithDetails()
    {
        AddInclude(nameof(Product.ProductVariants)); // Pass "ProductVariants" string

        AddInclude($"{nameof(Product.ProductVariants)}.{nameof(ProductVariant.Color)}"); // Pass "ProductVariants.Color"
        AddInclude($"{nameof(Product.ProductVariants)}.{nameof(ProductVariant.Size)}");  // Pass "ProductVariants.Size"

        return this;
    }

    /// <summary>
    /// Includes variants
    /// </summary>
    public ProductSpecification IncludeVariants()
    {
        AddInclude(nameof(Product.ProductVariants)); // Pass "ProductVariants" string

        return this;
    }

    /// <summary>
    /// Creates a specification for products with a specific category name.
    /// Relies on Category being included or EF Core implicitly joining.
    /// Consider calling IncludeCategory() if using this filter often.
    /// </summary>
    public ProductSpecification WithCategoryName(string categoryName)
    {
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            AddCriteria(p => p.Category.CategoryName == categoryName);
        }
        return this;
    }

    /// <summary>
    /// Adds criteria to search by product name or ProductID.
    /// </summary>
    public ProductSpecification SearchByIdOrName(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmedSearchTerm = searchTerm.Trim();
            var pattern = $"%{trimmedSearchTerm}%";

            if (int.TryParse(trimmedSearchTerm, out int id))
            {
                AddCriteria(p => p.ProductId == id || EF.Functions.ILike(p.ProductName, pattern));
            }
            else
            {
                AddCriteria(p => EF.Functions.ILike(p.ProductName, pattern));
            }
        }
        return this;
    }

    /// <summary>
    /// Adds criteria to filter by a specific category ID.
    /// </summary>
    public ProductSpecification WhereCategoryIs(int categoryId)
    {
        AddCriteria(p => p.CategoryId == categoryId);
        return this;
    }

    /// <summary>
    /// Adds criteria to filter products that have a variant with the specified color ID.
    /// </summary>
    public ProductSpecification HasColor(int colorId)
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.ColorId == colorId));
        return this;
    }

    /// <summary>
    /// Adds criteria to filter products that have a variant with the specified size ID.
    /// </summary>
    public ProductSpecification HasSize(int sizeId)
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.SizeId == sizeId));
        return this;
    }

    /// <summary>
    /// Adds criteria to filter products that have at least one variant in stock.
    /// </summary>
    public ProductSpecification IsInStock()
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.StockQuantity > 0));
        return this;
    }

    public ProductSpecification IsOutOfStock()
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.StockQuantity == 0));
        return this;
    }

    public new ProductSpecification ApplyPaging(int skip, int take)
    {
        base.ApplyPaging(skip, take);
        return this;
    }
}