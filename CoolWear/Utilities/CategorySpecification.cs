using CoolWear.Models;
using Microsoft.EntityFrameworkCore;

namespace CoolWear.Utilities;
public class CategorySpecification : GenericSpecification<ProductCategory>
{
    public CategorySpecification(
        string? searchTerm = null,
        string? productType = null, // Lọc theo loại sản phẩm
        int? skip = null,
        int? take = null)
    {
        // Lọc theo Loại sản phẩm
        if (!string.IsNullOrEmpty(productType) && productType != "Tất cả loại sản phẩm")
        {
            AddCriteria(c => c.ProductType == productType);
        }

        // Lọc theo Từ khóa tìm kiếm (ID hoặc Tên)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Kiểm tra xem có phải là số không để tìm theo ID
            var trimmedSearchTerm = searchTerm.Trim();
            var pattern = $"%{trimmedSearchTerm}%";

            if (int.TryParse(trimmedSearchTerm, out int id))
            {
                AddCriteria(c => c.CategoryId == id || EF.Functions.ILike(c.CategoryName, pattern));
            }
            else
            {
                AddCriteria(c => EF.Functions.ILike(c.CategoryName, pattern));
            }
        }

        // Áp dụng phân trang nếu có
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }
}