using System;
using System.Collections.Generic;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin chi tiết đơn hàng
/// </summary>
public partial class OrderItem
{
    /// <summary>
    /// Mã chi tiết đơn hàng (khóa chính)
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// Mã đơn hàng (khóa ngoại)
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Mã sản phẩm (khóa ngoại)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Số lượng
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Đơn giá
    /// </summary>
    public int UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
