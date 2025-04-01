using System;
using System.Collections.Generic;

namespace CoolWear.Models;

/// <summary>
/// Bảng đơn hàng
/// </summary>
public partial class Order
{
    /// <summary>
    /// Mã đơn hàng, khóa chính, tự động tăng
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Ngày đặt hàng, mặc định là thời điểm hiện tại
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Mã khách hàng, khóa ngoại (có thể null nếu khách hàng không đăng nhập)
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Tổng tiền đơn hàng trước khi giảm giá
    /// </summary>
    public int Subtotal { get; set; }

    /// <summary>
    /// Mã phương thức thanh toán, khóa ngoại
    /// </summary>
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Số điểm sử dụng, mặc định là 0
    /// </summary>
    public int PointUsed { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng, mặc định là &quot;Đang xử lý&quot;
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Tổng tiền đơn hàng sau khi giảm giá
    /// </summary>
    public int? NetTotal { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
