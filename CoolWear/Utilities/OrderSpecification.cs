using CoolWear.Models;
using System;

namespace CoolWear.Utilities;

public class OrderSpecification : GenericSpecification<Order>
{
    public OrderSpecification() { }
    public OrderSpecification(
        string? searchTerm = null,   // Tìm theo OrderId
        string? status = null,       // Lọc theo trạng thái
        DateTime? startDateUtc = null, // Lọc theo ngày tạo (UTC)
        DateTime? endDateUtc = null,   // Lọc theo ngày tạo (UTC)
        int? paymentMethodId = null, // Lọc theo ID Phương thức thanh toán
        int? minNetTotal = null,   // Lọc theo Tổng tiền tối thiểu
        int? maxNetTotal = null,   // Lọc theo Tổng tiền tối đa
        int? skip = null,
        int? take = null,
        bool includeDetails = true)
    {
        // Lọc theo Trạng thái
        if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
        {
            AddCriteria(o => o.Status == status);
        }

        // Lọc theo Ngày Tạo (so sánh với giá trị UTC)
        if (startDateUtc.HasValue)
        {
            var startDateUnspecified = DateTime.SpecifyKind(startDateUtc.Value, DateTimeKind.Unspecified);
            AddCriteria(o => o.OrderDate >= startDateUnspecified);
        }
        if (endDateUtc.HasValue)
        {
            var endDayUnspecified = DateTime.SpecifyKind(endDateUtc.Value, DateTimeKind.Unspecified);
            AddCriteria(o => o.OrderDate < endDayUnspecified);
        }

        // Lọc theo Từ khóa Tìm kiếm (chỉ theo OrderId)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            if (int.TryParse(searchTerm.Trim(), out int id))
            {
                AddCriteria(o => o.OrderId == id);
            }
            else
            {
                AddCriteria(o => false); // Không tìm thấy nếu không phải ID
            }
        }

        // Lọc theo Phương thức thanh toán
        if (paymentMethodId.HasValue)
        {
            AddCriteria(o => o.PaymentMethodId == paymentMethodId.Value);
        }

        // Lọc theo Khoảng Tổng tiền (NetTotal)
        if (minNetTotal.HasValue)
        {
            AddCriteria(o => o.NetTotal >= minNetTotal.Value);
        }
        if (maxNetTotal.HasValue)
        {
            if (!minNetTotal.HasValue || minNetTotal.Value <= maxNetTotal.Value)
            {
                AddCriteria(o => o.NetTotal <= maxNetTotal.Value);
            }
        }

        // Include các thông tin liên quan nếu cần hiển thị
        if (includeDetails)
        {
            AddInclude(nameof(Order.Customer)); // Include khách hàng (có thể null)
            AddInclude(nameof(Order.PaymentMethod)); // Include phương thức thanh toán
            // Include OrderItems và các thông tin liên quan của nó
            AddInclude($"{nameof(Order.OrderItems)}");
            AddInclude($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Variant)}");
            AddInclude($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Variant)}.{nameof(ProductVariant.Product)}"); // Lấy tên SP
            AddInclude($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Variant)}.{nameof(ProductVariant.Color)}"); // Lấy màu
            AddInclude($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Variant)}.{nameof(ProductVariant.Size)}");  // Lấy size
        }

        // Áp dụng Phân trang
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }

    public OrderSpecification CompletedOrderRevenue(DateTime startDateUtc, DateTime endDateUtc)
    {
        var startUnspecified = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Unspecified);
        var endUnspecified = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Unspecified);
        // Lọc đơn hàng hoàn thành trong khoảng thời gian
        AddCriteria(o => o.Status == "Hoàn thành" &&
                             o.OrderDate >= startUnspecified &&
                             o.OrderDate < endUnspecified);

        // Include thông tin cần thiết để tính lợi nhuận
        AddInclude(nameof(Order.OrderItems));
        AddInclude($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Variant)}");
        AddInclude($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Variant)}.{nameof(ProductVariant.Product)}");
        return this;
    }

    public OrderSpecification OrderStatusCount(DateTime startDateUtc, DateTime endDateUtc)
    {
        var startUnspecified = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Unspecified);
        var endUnspecified = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Unspecified);
        // Chỉ lọc theo khoảng thời gian
        AddCriteria(o => o.OrderDate >= startUnspecified && o.OrderDate < endUnspecified); // Chỉ để lấy trạng thái đơn hàng
        return this;
    }
}