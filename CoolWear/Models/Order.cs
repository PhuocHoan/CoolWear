using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin đơn hàng
/// </summary>
public partial class Order : INotifyPropertyChanged
{
    /// <summary>
    /// Mã đơn hàng (khóa chính)
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Ngày đặt hàng
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Mã khách hàng (khóa ngoại)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Tổng số tiền
    /// </summary>
    public int TotalAmount { get; set; }

    /// <summary>
    /// Mã phương thức thanh toán (khóa ngoại)
    /// </summary>
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Trạng thái hoàn tiền
    /// </summary>
    public bool IsRefunded { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public event PropertyChangedEventHandler? PropertyChanged;
}
