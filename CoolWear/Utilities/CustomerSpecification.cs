using CoolWear.Models;
using System;
using System.Linq.Expressions;

namespace CoolWear.Utilities;

public class CustomerSpecification : GenericSpecification<Customer>
{
    public CustomerSpecification(
        string? searchTerm = null,
        int? minPoints = null,       // Điểm tối thiểu
        int? maxPoints = null,       // Điểm tối đa
        DateTime? startDateUtc = null,  // Ngày bắt đầu
        DateTime? endDateUtc = null,    // Ngày kết thúc
        int? skip = null,
        int? take = null)
    {
        // --- Lọc Cơ bản ---
        // Chỉ lấy khách hàng chưa bị xóa
        AddCriteria(c => !c.IsDeleted);

        // --- Lọc theo Điểm Thưởng ---
        if (minPoints.HasValue)
        {
            AddCriteria(c => c.Points >= minPoints.Value);
        }
        if (maxPoints.HasValue)
        {
            // Nếu chỉ có maxPoints, hoặc minPoints <= maxPoints
            if (!minPoints.HasValue || minPoints.Value <= maxPoints.Value)
            {
                AddCriteria(c => c.Points <= maxPoints.Value);
            }
            // Có thể thêm logic xử lý nếu min > max (ví dụ: không trả về gì hoặc báo lỗi)
        }

        // --- Lọc theo Ngày Tạo ---
        if (startDateUtc.HasValue)
        {
            // Lấy từ đầu ngày startDateUtc
            AddCriteria(c => c.CreateDate >= startDateUtc.Value.Date);
        }
        if (endDateUtc.HasValue)
        {
            // Lấy đến hết ngày endDateUtc
            var endOfDayUtc = endDateUtc.Value.Date.AddDays(1);
            AddCriteria(c => c.CreateDate < endOfDayUtc);
        }

        // --- Lọc theo Từ khóa Tìm kiếm ---
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmedSearchTerm = searchTerm.Trim().ToLowerInvariant();
            // Tạo biểu thức OR
            Expression<Func<Customer, bool>> searchPredicate = c => false; // Khởi tạo là false

            // Tìm theo ID (nếu là số)
            if (int.TryParse(trimmedSearchTerm, out int id))
            {
                searchPredicate = searchPredicate.Or(c => c.CustomerId == id);
            }

            // Tìm theo Tên (chứa, không phân biệt hoa thường)
            searchPredicate = searchPredicate.Or(c => c.CustomerName.ToLower().Contains(trimmedSearchTerm));

            // Tìm theo Email (chứa, không phân biệt hoa thường, kiểm tra null trước)
            searchPredicate = searchPredicate.Or(c => c.Email != null && c.Email.ToLower().Contains(trimmedSearchTerm));

            // Tìm theo Phone
            searchPredicate = searchPredicate.Or(c => c.Phone.Contains(trimmedSearchTerm));

            AddCriteria(searchPredicate);
        }

        // --- Áp dụng Phân trang ---
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }
}