using CoolWear.Models;
using CoolWear.Services;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CoolWear.Utilities
{
    public class ProductSpecification : GenericSpecification<Product>
    {
        public ProductSpecification() { }

        public ProductSpecification(bool includeAllDetails)
        {
            if (includeAllDetails)
            {
                IncludeCategory();
                IncludeVariantsWithDetails();
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
        /// Adds criteria to search by product name or ID.
        /// </summary>
        public ProductSpecification SearchByIdOrName(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var trimmedSearchTerm = searchTerm.Trim();
                if (int.TryParse(trimmedSearchTerm, out int id))
                {
                    AddCriteria(p => p.ProductId == id || p.ProductName.Contains(trimmedSearchTerm, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    AddCriteria(p => p.ProductName.Contains(trimmedSearchTerm, StringComparison.CurrentCultureIgnoreCase));
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
    }
}