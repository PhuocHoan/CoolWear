using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng màu sắc sản phẩm
/// </summary>
public partial class ProductColor : INotifyPropertyChanged
{
    /// <summary>
    /// Mã màu sắc, khóa chính, tự động tăng
    /// </summary>
    public int ColorId { get; set; }

    /// <summary>
    /// Tên màu sắc, duy nhất
    /// </summary>
    public string ColorName { get; set; } = null!;

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public event PropertyChangedEventHandler? PropertyChanged;
}
