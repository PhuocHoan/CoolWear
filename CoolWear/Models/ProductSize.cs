using System;
using System.Collections.Generic;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin kích thước sản phẩm
/// </summary>
public partial class ProductSize
{
    /// <summary>
    /// Mã kích thước (khóa chính)
    /// </summary>
    public int SizeId { get; set; }

    /// <summary>
    /// Tên kích thước
    /// </summary>
    public string SizeName { get; set; } = null!;

    public virtual ICollection<ProductSizeLink> ProductSizeLinks { get; set; } = new List<ProductSizeLink>();
}
