using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng chủ cửa hàng
/// </summary>
public partial class StoreOwner : INotifyPropertyChanged
{
    /// <summary>
    /// Mã chủ cửa hàng, khóa chính, tự động tăng
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// Tên chủ cửa hàng
    /// </summary>
    public string OwnerName { get; set; } = null!;

    /// <summary>
    /// Email chủ cửa hàng
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Số điện thoại chủ cửa hàng
    /// </summary>
    public string Phone { get; set; } = null!;

    /// <summary>
    /// Địa chỉ chủ cửa hàng
    /// </summary>
    public string Address { get; set; } = null!;

    /// <summary>
    /// Tên đăng nhập
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Mã hóa mật khẩu
    /// </summary>
    public string Entropy { get; set; } = null!;

    public event PropertyChangedEventHandler? PropertyChanged;
}
