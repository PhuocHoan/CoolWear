using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CoolWear.Utilities;

/// <summary>
/// Giao diện để triển khai mẫu Đặc tả với khả năng truy vấn động.
/// </summary>
/// <typeparam name="T">Loại thực thể mà đặc tả này áp dụng.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Bộ sưu tập các biểu thức lọc sẽ được áp dụng cho truy vấn.
    /// </summary>
    IEnumerable<Expression<Func<T, bool>>> Criteria { get; }

    /// <summary>
    /// Bộ sưu tập các biểu thức bao gồm để tải trước các thực thể liên quan.
    /// </summary>
    IEnumerable<string> IncludeStrings { get; } // Bao gồm dựa trên chuỗi

    // --- Phân trang ---
    /// <summary>
    /// Lấy số lượng mục cần lấy (kích thước trang).
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Lấy số lượng mục cần bỏ qua.
    /// </summary>
    int Skip { get; }
}
