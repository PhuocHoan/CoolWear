﻿using CoolWear.Models;
using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
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
                    if (variant.Variant.StockQuantity > 0)
                    {
                        var orderItem = new OrderItem
                        {
                            VariantId = variantId,
                            Quantity = 1, // Default quantity
                            UnitPrice = variant.Product.Price
                        };

                        // Subscribe to Quantity changes for validation
                        orderItem.PropertyChanged += (s, args) =>
                        {
                            if (args.PropertyName == nameof(OrderItem.Quantity))
                            {
                                ValidateQuantity(orderItem, variant.Variant.StockQuantity);
                            }
                        };

                        ViewModel.OrdersItems.Add(orderItem);
                    }
                    else
                    {
                        Debug.WriteLine($"[WARN] VariantId = {variantId} is out of stock.");
                        SellViewModel.ShowErrorDialog("Out of Stock", $"Sản phẩm {variantId} hết hàng.");
                    }
                }
                else
                {
                    Debug.WriteLine($"[WARN] Không tìm thấy biến thể với VariantId = {variantId}");
                }
            }
        }
    }

    private void ClearCategory_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Clear Selection button clicked.");
        ViewModel.SelectedCategory = null;
    }

    private void ValidateQuantity(OrderItem orderItem, int stockQuantity)
    {
        if (orderItem.Quantity > stockQuantity)
        {
            Debug.WriteLine($"[WARN] Quantity exceeds stock. Resetting to maximum available: {stockQuantity}");
            orderItem.Quantity = stockQuantity;

            // Notify the user
            _ = SellViewModel.ShowErrorDialog("Stock Limit Exceeded",
                $"Số lượng không thể vượt quá tồn kho ({stockQuantity}).");
        }
        else if (orderItem.Quantity < 1)
        {
            Debug.WriteLine("[WARN] Quantity cannot be less than 1. Resetting to 1.");
            orderItem.Quantity = 1;
        }
    }

    private void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int variantId)
        {
            var itemToRemove = ViewModel.OrdersItems!.FirstOrDefault(i => i.VariantId == variantId);

            if (itemToRemove != null)
            {

                ViewModel.OrdersItems!.Remove(itemToRemove);
                Debug.WriteLine($"✅ Đã xóa sản phẩm với VariantId: {variantId}");

                ViewModel.SelectedVariantIds.Remove(variantId);
                Debug.WriteLine($"✅ Đã xóa VariantId: {variantId} khỏi SelectedVariantIds.");

                if (_selectedOrderItem == itemToRemove)
                {
                    _selectedOrderItem = null;
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
        if (ViewModel.OrdersItems!.Count == 0)
        {
            Debug.WriteLine("Không có sản phẩm nào để thanh toán.");
            await SellViewModel.ShowErrorDialog("Error", "Không có sản phẩm nào để thanh toán.");
            return;
        }

        try
        {
            int subtotal = ViewModel.OrdersItems.Sum(i => i.Quantity * i.UnitPrice);
            int pointUsed = ViewModel.PointInput;
            int netTotal = subtotal - pointUsed * 1000;

            var newOrder = new Order
            {
                OrderDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
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
                // Decrease StockQuantity for the corresponding ProductVariant
                var variant = await ViewModel.UnitOfWork.ProductVariants.GetByIdAsync(item.VariantId);
                if (variant != null)
                {
                    variant.StockQuantity -= item.Quantity;
                    if (variant.StockQuantity < 0)
                    {
                        variant.StockQuantity = 0; // Ensure stock doesn't go negative
                    }

                    await ViewModel.UnitOfWork.ProductVariants.UpdateAsync(variant);
                }
            }

            await ViewModel.UnitOfWork.SaveChangesAsync();

            if (_selectedCustomerId.HasValue)
            {
                var customer = await ViewModel.UnitOfWork.Customers.GetByIdAsync(_selectedCustomerId.Value);
                if (customer != null)
                {

                    int pointsEarned = SellViewModel.CalculatePoints(netTotal);
                    if (ViewModel.SelectedStatus == "Hoàn thành")
                        customer.Points += pointsEarned;
                    customer.Points -= pointUsed;

                    await ViewModel.UnitOfWork.Customers.UpdateAsync(customer);
                    await ViewModel.UnitOfWork.SaveChangesAsync();

                    Debug.WriteLine($"✅ Điểm đã được cộng: {pointsEarned}. Tổng điểm hiện tại: {customer.Points}");
                }
            }

            ViewModel.OrdersItems.Clear();
            ViewModel.SelectedVariantIds.Clear();
            ViewModel.SelectedPaymentMethodId = 1;
            ViewModel.SelectedStatus = "Hoàn thành";
            Debug.WriteLine($"✅ SelectedStatus reset to: {ViewModel.SelectedStatus}");

            await ViewModel.InitializeAsync();
            Debug.WriteLine("✅ Thanh toán thành công!");
            await SellViewModel.ShowSuccessDialog("Success", "Thanh toán thành công.");
            if (ViewModel.IsReceiptEnabled)
            {
                Debug.WriteLine("Generating receipt...");
                await ViewModel.GenerateAndOpenReceiptAsync(newOrder);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Lỗi khi lưu đơn hàng: {ex.Message}");
            await SellViewModel.ShowErrorDialog("Error", "Lỗi thanh toán.");
        }
    }
    private void SelectCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int customerId)
        {
            if (_selectedCustomerId == customerId)
            {
                _selectedCustomerId = null;
                ViewModel.SelectedCustomerName = null;
                ViewModel.SelectedCustomerPoints = null; // Reset points
                Debug.WriteLine("✅ Đã bỏ chọn khách hàng.");
            }
            else
            {
                _selectedCustomerId = customerId;
                Debug.WriteLine($"✅ Khách hàng được chọn có ID: {_selectedCustomerId}");

                var customer = ViewModel.FilteredCustomers?.FirstOrDefault(c => c.CustomerId == customerId);
                if (customer != null)
                {
                    ViewModel.SelectedCustomerName = customer.CustomerName;
                    ViewModel.SelectedCustomerPoints = customer.Points; // Set points
                    Debug.WriteLine($"Tên khách hàng: {customer.CustomerName}, Điểm: {customer.Points}");
                }
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
