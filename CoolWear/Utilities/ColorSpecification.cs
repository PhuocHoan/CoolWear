using CoolWear.Models;
using Microsoft.EntityFrameworkCore;

namespace CoolWear.Utilities;
public class ColorSpecification : GenericSpecification<ProductColor>
{
    public ColorSpecification(
        string? searchTerm = null,
        int? skip = null,
        int? take = null)
    {
        // --- Lọc Cơ bản ---
        // Chỉ lấy color chưa bị xóa
        AddCriteria(c => !c.IsDeleted);

        // Lọc theo Từ khóa tìm kiếm (ID hoặc Tên)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Kiểm tra xem có phải là số không để tìm theo ID
            var trimmedSearchTerm = searchTerm.Trim();
            var pattern = $"%{trimmedSearchTerm}%";

            if (int.TryParse(trimmedSearchTerm, out int id))
            {
                AddCriteria(c => c.ColorId == id || EF.Functions.ILike(c.ColorName, pattern));
            }
            else
            {
                AddCriteria(c => EF.Functions.ILike(c.ColorName, pattern));
            }
        }

        // Áp dụng phân trang nếu có
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }
}