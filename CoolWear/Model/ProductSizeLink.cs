using System;
using System.Collections.Generic;

namespace CoolWear.Model;

/// <summary>
/// Bảng lưu trữ liên kết giữa sản phẩm và kích thước
/// </summary>
public partial class ProductSizeLink
{
    /// <summary>
    /// Mã liên kết (khóa chính)
    /// </summary>
    public int ProductSizeId { get; set; }

    /// <summary>
    /// Mã sản phẩm (khóa ngoại)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Mã kích thước (khóa ngoại)
    /// </summary>
    public int SizeId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductSize Size { get; set; } = null!;
}
