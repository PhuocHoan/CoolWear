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
        bool? inStockOnly = null, // true = còn hàng, false = hết hàng, null = tất cả
        int? skip = null,
        int? take = null,
        bool includeDetails = true)
    {
        AddCriteria(p => !p.IsDeleted); // Loại bỏ sản phẩm đã bị xóa

        if (includeDetails)
        {
            IncludeCategory();
            IncludeVariantsWithDetails();
        }

        // Áp dụng bộ lọc dựa trên các tham số của hàm tạo
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

        // Áp dụng phân trang nếu được yêu cầu
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }

    /// <summary>
    /// Bao gồm dữ liệu danh mục sử dụng chuỗi bao gồm.
    /// </summary>
    public ProductSpecification IncludeCategory()
    {
        AddInclude(nameof(Product.Category)); // Truyền chuỗi "Category"
        return this;
    }

    /// <summary>
    /// Bao gồm các biến thể và thông tin liên quan về Màu sắc và Kích thước bằng cách sử dụng chuỗi bao gồm.
    /// </summary>
    public ProductSpecification IncludeVariantsWithDetails()
    {
        AddInclude(nameof(Product.ProductVariants)); // Truyền chuỗi "ProductVariants"

        AddInclude($"{nameof(Product.ProductVariants)}.{nameof(ProductVariant.Color)}"); // Truyền chuỗi "ProductVariants.Color"
        AddInclude($"{nameof(Product.ProductVariants)}.{nameof(ProductVariant.Size)}");  // Truyền chuỗi "ProductVariants.Size"

        return this;
    }

    /// <summary>
    /// Bao gồm các biến thể
    /// </summary>
    public ProductSpecification IncludeVariants()
    {
        AddInclude(nameof(Product.ProductVariants)); // Truyền chuỗi "ProductVariants"
        return this;
    }

    /// <summary>
    /// Tạo đặc tả cho các sản phẩm với tên danh mục cụ thể.
    /// Phụ thuộc vào việc bao gồm Category hoặc EF Core tự động join.
    /// Cân nhắc gọi IncludeCategory() nếu sử dụng bộ lọc này thường xuyên.
    /// </summary>
    public ProductSpecification WithCategoryName(string categoryName)
    {
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            AddCriteria(p => p.Category!.CategoryName == categoryName);
        }
        return this;
    }

    /// <summary>
    /// Thêm tiêu chí để tìm kiếm theo tên sản phẩm hoặc Mã sản phẩm.
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
    /// Thêm tiêu chí lọc theo mã danh mục cụ thể.
    /// </summary>
    public ProductSpecification WhereCategoryIs(int categoryId)
    {
        AddCriteria(p => p.CategoryId == categoryId);
        return this;
    }

    /// <summary>
    /// Thêm tiêu chí lọc theo mã màu cụ thể.
    /// </summary>
    public ProductSpecification HasColor(int colorId)
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.ColorId == colorId));
        return this;
    }

    /// <summary>
    /// Thêm tiêu chí lọc theo mã kích thước cụ thể.
    /// </summary>
    public ProductSpecification HasSize(int sizeId)
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.SizeId == sizeId));
        return this;
    }

    /// <summary>
    /// Thêm tiêu chí lọc để chỉ lấy các sản phẩm còn hàng.
    /// </summary>
    public ProductSpecification IsInStock()
    {
        AddCriteria(p => p.ProductVariants.Any(v => v.StockQuantity > 0));
        return this;
    }

    /// <summary>
    /// Thêm tiêu chí lọc để chỉ lấy các sản phẩm hết hàng.
    /// </summary>
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