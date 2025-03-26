using System;
using System.Collections.Generic;

namespace CoolWear.Model;

/// <summary>
/// Bảng lưu trữ danh mục sản phẩm (loại áo/quần)
/// </summary>
public partial class ProductCategory
{
    /// <summary>
    /// Mã danh mục (khóa chính)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tên danh mục
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Loại sản phẩm
    /// </summary>
    public string ProductType { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
