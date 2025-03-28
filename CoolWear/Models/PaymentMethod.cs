using System;
using System.Collections.Generic;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin phương thức thanh toán
/// </summary>
public partial class PaymentMethod
{
    /// <summary>
    /// Mã phương thức thanh toán (khóa chính)
    /// </summary>
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Tên phương thức thanh toán
    /// </summary>
    public string PaymentMethodName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
