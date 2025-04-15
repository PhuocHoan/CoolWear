using CoolWear.Models;
using Microsoft.EntityFrameworkCore;

namespace CoolWear.Utilities;
public class SizeSpecification : GenericSpecification<ProductSize>
{
    public SizeSpecification(
        string? searchTerm = null,
        int? skip = null,
        int? take = null)
    {
        // --- Lọc Cơ bản ---
        // Chỉ lấy size chưa bị xóa
        AddCriteria(s => !s.IsDeleted);

        // Lọc theo Từ khóa tìm kiếm (ID hoặc Tên)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Kiểm tra xem có phải là số không để tìm theo ID
            var trimmedSearchTerm = searchTerm.Trim();
            var pattern = $"%{trimmedSearchTerm}%";

            if (int.TryParse(trimmedSearchTerm, out int id))
            {
                AddCriteria(c => c.SizeId == id || EF.Functions.ILike(c.SizeName, pattern));
            }
            else
            {
                AddCriteria(c => EF.Functions.ILike(c.SizeName, pattern));
            }
        }

        // Áp dụng phân trang nếu có
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }
}