using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng khách hàng
/// </summary>
public partial class Customer : INotifyPropertyChanged
{
    /// <summary>
    /// Mã khách hàng, khóa chính, tự động tăng
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
    /// Ngày tạo tài khoản, mặc định là thời điểm hiện tại
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Điểm tích lũy của khách hàng, mặc định là 0
    /// </summary>
    public int Points { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
