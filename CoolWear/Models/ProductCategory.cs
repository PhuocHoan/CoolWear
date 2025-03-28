using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin các danh mục sản phẩm
/// </summary>
public partial class ProductCategory : INotifyPropertyChanged
{
    /// <summary>
    /// ID duy nhất của danh mục (khóa chính)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tên danh mục sản phẩm
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Loại sản phẩm (áo, quần) thuộc danh mục
    /// </summary>
    public string ProductType { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
