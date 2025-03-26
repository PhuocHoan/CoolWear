using System;
using System.Collections.Generic;

namespace CoolWear.Model;

/// <summary>
/// Bảng lưu trữ thông tin màu sắc sản phẩm
/// </summary>
public partial class ProductColor
{
    /// <summary>
    /// Mã màu (khóa chính)
    /// </summary>
    public int ColorId { get; set; }

    /// <summary>
    /// Tên màu
    /// </summary>
    public string ColorName { get; set; } = null!;

    public virtual ICollection<ProductColorLink> ProductColorLinks { get; set; } = new List<ProductColorLink>();
}
