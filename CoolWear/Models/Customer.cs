using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin khách hàng
/// </summary>
public partial class Customer : INotifyPropertyChanged
{
    /// <summary>
    /// Mã khách hàng (khóa chính)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Tên khách hàng
    /// </summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>
    /// Email khách hàng
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Số điện thoại khách hàng
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ khách hàng
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Ngày tạo tài khoản
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Điểm tích lũy
    /// </summary>
    public int Points { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
