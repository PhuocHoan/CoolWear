using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng danh mục sản phẩm
/// </summary>
public partial class ProductCategory : INotifyPropertyChanged
{
    /// <summary>
    /// Mã danh mục sản phẩm, khóa chính
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tên danh mục sản phẩm, duy nhất
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Loại sản phẩm (áo, quần,...)
    /// </summary>
    public string ProductType { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
