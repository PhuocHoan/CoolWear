using CoolWear.Models;
using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace CoolWear.Views;

public sealed partial class SellPage : Page
{
    public SellViewModel ViewModel { get; private set; }

    public SellPage()
    {
        InitializeComponent();

        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<SellViewModel>();
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing SellPage: {ex.Message}");
        }
    }

    private int? _selectedCustomerId;
    private OrderItem? _selectedOrderItem;
    public decimal TotalPrice { get; private set; }

    private void VariantIdClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int variantId)
        {
            Debug.WriteLine($"[DEBUG] Clicked VariantId: {variantId}");

            if (!ViewModel.SelectedVariantIds.Contains(variantId))
            {
                ViewModel.SelectedVariantIds.Add(variantId);

                var variant = ViewModel.FilteredProducts
                                       .SelectMany(p => p.ProductVariants.Select(v => new { Product = p, Variant = v }))
                                       .FirstOrDefault(x => x.Variant.VariantId == variantId);

                if (variant != null)
                {
                    var orderItem = new OrderItem
                    {
                        VariantId = variantId,
                        Quantity = 1,
                        UnitPrice = variant.Product.Price
                    };

                    ViewModel.OrdersItems.Add(orderItem);

                }
                else
                {
                    Debug.WriteLine($"[WARN] Không tìm thấy biến thể với VariantId = {variantId}");
                }
            }
        }
    }

    private void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int variantId)
        {
            // Tìm sản phẩm trong OrdersItems dựa vào VariantId
            var itemToRemove = ViewModel.OrdersItems.FirstOrDefault(i => i.VariantId == variantId);

            if (itemToRemove != null)
            {
                // Xóa OrderItem khỏi danh sách
                ViewModel.OrdersItems.Remove(itemToRemove);
                Debug.WriteLine($"✅ Đã xóa sản phẩm với VariantId: {variantId}");

                // Xóa VariantId khỏi danh sách SelectedVariantIds để cho phép chọn lại
                ViewModel.SelectedVariantIds.Remove(variantId);
                Debug.WriteLine($"✅ Đã xóa VariantId: {variantId} khỏi SelectedVariantIds.");

                // Cập nhật lại trạng thái lựa chọn sau khi xóa (nếu có)
                if (_selectedOrderItem == itemToRemove)
                {
                    _selectedOrderItem = null;  // Deselect nếu sản phẩm vừa xóa đang được chọn
                    Debug.WriteLine("✅ Đã bỏ chọn sản phẩm vừa xóa.");
                }
            }
            else
            {
                Debug.WriteLine($"❌ Không tìm thấy sản phẩm với VariantId: {variantId}");
            }
        }
    }



    private async void Checkout_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.OrdersItems.Count == 0)
        {
            Debug.WriteLine("Không có sản phẩm nào để thanh toán.");
            return;
        }

        try
        {

            int subtotal = ViewModel.OrdersItems.Sum(i => i.Quantity * i.UnitPrice);
            int pointUsed = 0; 
            int netTotal = subtotal - pointUsed;


            var newOrder = new Order
            {
                OrderDate = DateTime.Now,
                Subtotal = subtotal,
                NetTotal = netTotal,
                PointUsed = pointUsed,
                Status = ViewModel.SelectedStatus,
                CustomerId = _selectedCustomerId,
                PaymentMethodId = ViewModel.SelectedPaymentMethodId, 
            };

            await ViewModel.UnitOfWork.Orders.AddAsync(newOrder);

            await ViewModel.UnitOfWork.SaveChangesAsync();


            foreach (var item in ViewModel.OrdersItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = newOrder.OrderId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };

                await ViewModel.UnitOfWork.OrderItems.AddAsync(orderItem);
            }

            await ViewModel.UnitOfWork.SaveChangesAsync();


            ViewModel.OrdersItems.Clear();
            ViewModel.SelectedVariantIds.Clear();

            Debug.WriteLine("✅ Thanh toán thành công!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Lỗi khi lưu đơn hàng: {ex.Message}");
        }
    }



    private void SelectCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int customerId)
        {
            _selectedCustomerId = customerId;
            Debug.WriteLine($"✅ Khách hàng được chọn có ID: {_selectedCustomerId}");
          
            var customer = ViewModel.FilteredCustomers?.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer != null)
            {
                Debug.WriteLine($"Tên khách hàng: {customer.CustomerName}");
                ViewModel.SelectedCustomerName = customer.CustomerName;
            }
        }
    }


    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine("Sell: OnNavigatedTo");

        if (ViewModel != null)
        {
            if (ViewModel.IsLoading)
            {
                return;
            }

            await ViewModel.InitializeAsync();
        }
        else
        {
            Debug.WriteLine("ERROR: ViewModel is null in OnNavigatedTo.");
        }
    }
}
