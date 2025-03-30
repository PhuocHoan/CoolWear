using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng biến thể sản phẩm
/// </summary>
public partial class ProductVariant : INotifyPropertyChanged
{
    /// <summary>
    /// Mã biến thể sản phẩm, khóa chính, tự động tăng
    /// </summary>
    public int VariantId { get; set; }

    /// <summary>
    /// Mã sản phẩm, khóa ngoại
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Mã màu sắc, khóa ngoại
    /// </summary>
    public int ColorId { get; set; }

    /// <summary>
    /// Mã kích thước, khóa ngoại
    /// </summary>
    public int SizeId { get; set; }

    /// <summary>
    /// Số lượng tồn kho
    /// </summary>
    public int StockQuantity { get; set; }

    public virtual ProductColor Color { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ProductSize Size { get; set; } = null!;

    public event PropertyChangedEventHandler? PropertyChanged;
}
