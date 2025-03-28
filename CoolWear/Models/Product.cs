using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng lưu trữ thông tin sản phẩm
/// </summary>
public partial class Product : INotifyPropertyChanged
{
    /// <summary>
    /// Mã sản phẩm (khóa chính)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Giá nhập
    /// </summary>
    public int ImportPrice { get; set; }

    /// <summary>
    /// Giá bán
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Mã danh mục (khóa ngoại)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Đường dẫn tới ảnh sản phẩm
    /// </summary>
    public string PublicId { get; set; } = null!;

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductColorLink> ProductColorLinks { get; set; } = new List<ProductColorLink>();

    public virtual ICollection<ProductSizeLink> ProductSizeLinks { get; set; } = new List<ProductSizeLink>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
