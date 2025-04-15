using CoolWear.Models;
using System;

namespace CoolWear.Utilities;
public class OrderItemSpecification : GenericSpecification<OrderItem>
{
    public OrderItemSpecification() { }
    public OrderItemSpecification TopSellingItems(DateTime startDateUtc, DateTime endDateUtc)
    {
        var startUnspecified = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Unspecified);
        var endUnspecified = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Unspecified);
        // Lọc OrderItem của các đơn hàng Hoàn thành trong khoảng thời gian
        AddCriteria(oi => oi.Order.Status == "Hoàn thành" &&
                          oi.Order.OrderDate >= startUnspecified &&
                          oi.Order.OrderDate < endUnspecified);

        // Include thông tin ProductVariant và Product để lấy tên
        AddInclude(nameof(OrderItem.Variant));
        AddInclude($"{nameof(OrderItem.Variant)}.{nameof(ProductVariant.Product)}");
        return this;
    }
}