using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoolWear.Models;

/// <summary>
/// Bảng phương thức thanh toán
/// </summary>
public partial class PaymentMethod : INotifyPropertyChanged
{
    /// <summary>
    /// Mã phương thức thanh toán, khóa chính, tự động tăng
    /// </summary>
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Tên phương thức thanh toán, duy nhất
    /// </summary>
    public string PaymentMethodName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
