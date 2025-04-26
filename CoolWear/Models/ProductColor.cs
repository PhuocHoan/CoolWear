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

    /// <summary>
    /// Trạng thái xóa màu sắc, mặc định là chưa (false)
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = [];
    public event PropertyChangedEventHandler? PropertyChanged;
}
