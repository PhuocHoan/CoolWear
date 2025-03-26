using System;
using System.Collections.Generic;

namespace CoolWear.Model;

/// <summary>
/// Bảng lưu trữ thông tin chủ cửa hàng
/// </summary>
public partial class StoreOwner
{
    /// <summary>
    /// Mã chủ cửa hàng (khóa chính)
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
    /// Mật khẩu đã mã hóa
    /// </summary>
    public string Password { get; set; } = null!;
}
