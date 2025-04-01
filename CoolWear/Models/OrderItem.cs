using System;
using System.Collections.Generic;

namespace CoolWear.Models;

/// <summary>
/// Bảng chi tiết đơn hàng
/// </summary>
public partial class OrderItem
{
    /// <summary>
    /// Mã chi tiết đơn hàng, khóa chính, tự động tăng
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// Mã đơn hàng, khóa ngoại
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Mã biến thể sản phẩm, khóa ngoại
    /// </summary>
    public int VariantId { get; set; }

    /// <summary>
    /// Số lượng sản phẩm
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Đơn giá
    /// </summary>
    public int UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
