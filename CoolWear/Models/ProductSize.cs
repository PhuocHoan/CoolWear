using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng kích thước sản phẩm
/// </summary>
public partial class ProductSize : INotifyPropertyChanged
{
    /// <summary>
    /// Mã kích thước, khóa chính, tự động tăng
    /// </summary>
    public int SizeId { get; set; }

    /// <summary>
    /// Tên kích thước, duy nhất
    /// </summary>
    public string SizeName { get; set; } = null!;

    /// <summary>
    /// Trạng thái xóa kích thước, mặc định là chưa (false)
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
