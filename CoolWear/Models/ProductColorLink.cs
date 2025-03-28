using System;
using System.Collections.Generic;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ liên kết giữa sản phẩm và màu sắc
/// </summary>
public partial class ProductColorLink
{
    /// <summary>
    /// Mã liên kết (khóa chính)
    /// </summary>
    public int ProductColorId { get; set; }

    /// <summary>
    /// Mã sản phẩm (khóa ngoại)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Mã màu (khóa ngoại)
    /// </summary>
    public int ColorId { get; set; }

    public virtual ProductColor Color { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
