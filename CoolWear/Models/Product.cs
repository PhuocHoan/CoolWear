using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng sản phẩm
/// </summary>
public partial class Product : INotifyPropertyChanged
{
    /// <summary>
    /// Mã sản phẩm, khóa chính, tự động tăng
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Tên sản phẩm, duy nhất
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Giá nhập sản phẩm
    /// </summary>
    public int ImportPrice { get; set; }

    /// <summary>
    /// Giá bán sản phẩm
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// Mã danh mục sản phẩm, khóa ngoại
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Đường dẫn hình ảnh công khai
    /// </summary>
    public string PublicId { get; set; } = null!;

    /// <summary>
    /// Trạng thái xóa sản phẩm, mặc định là chưa (false)
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual ProductCategory? Category { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
