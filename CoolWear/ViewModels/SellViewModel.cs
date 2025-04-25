using System;
using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public class SellViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;
    public IUnitOfWork UnitOfWork => _unitOfWork;
    private ObservableCollection<Product>? _filteredProducts;
    private ObservableCollection<ProductCategory>? _categories;
    private ObservableCollection<ProductVariant>? _productVariants;
    private int _selectedPaymentMethodId = 1;
    private string _selectedStatus = "Hoàn thành";
    private ProductCategory? _selectedCategory;
    private string? _searchTerm;
    private bool _isLoading;
    private ObservableCollection<Product>? _selectedProducts = new();
    private Product? _selectedProduct;
    public bool IsProductSelected => SelectedProduct != null;

    private ObservableCollection<Order>? _orders = new();
    private ObservableCollection<OrderItem>? _ordersItems = new();
    private ObservableCollection<Customer>? _filteredCustomers;
    private bool _isReceiptEnabled;
    private List<int> _selectedVariantIds = new();

    private decimal _totalPrice;
    private string? _customerSearchTerm;
    private string? _selectedCustomerName;
    public int Points { get; set; }
    private int? _selectedCustomerPoints;
    private int _pointInput;
    private int _pointUsed;
    private decimal _netTotal;
    private decimal _subTotal;


    public int? SelectedCustomerPoints
    {
        get => _selectedCustomerPoints;
        set => SetProperty(ref _selectedCustomerPoints, value);
    }
    public int PointInput
    {
        get => _pointInput;
        set
        {
            if (value < 0) value = 0;
            int maxPoints = Math.Min(SelectedCustomerPoints ?? 0, (int)(SubTotal / 1000));
            if (value > maxPoints) value = maxPoints;

            if (_pointInput != value)
            {
                _pointInput = value;
                OnPropertyChanged(nameof(PointInput));
            }
            NetTotal = TotalPrice - (PointInput * 1000);
        }
    }
    public int PointUsed
    {
        get => _pointUsed;
        private set => SetProperty(ref _pointUsed, value);
    }
    public decimal NetTotal
    {
        get => _netTotal;
        private set => SetProperty(ref _netTotal, value);
    }
    public decimal SubTotal
    {
        get => OrdersItems?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;
        private set => OnPropertyChanged(nameof(TotalPrice));
    }

    public bool IsReceiptEnabled
    {
        get => _isReceiptEnabled;
        set => SetProperty(ref _isReceiptEnabled, value);
    }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (value == "Hoàn thành" || value == "Đang xử lý")
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }
    }
    public int SelectedPaymentMethodId
    {
        get => _selectedPaymentMethodId;
        set
        {
            if (value == 1 || value == 2) 
            {
                _selectedPaymentMethodId = value;
                OnPropertyChanged(nameof(SelectedPaymentMethodId));
            }
        }
    }
    public decimal TotalPrice
    {
        get => OrdersItems?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;
        private set => OnPropertyChanged(nameof(TotalPrice));
    }

    public string? CustomerSearchTerm
    {
        get => _customerSearchTerm;
        set => SetProperty(ref _customerSearchTerm, value);
    }

    public ObservableCollection<Customer>? FilteredCustomers
    {
        get => _filteredCustomers;
        private set => SetProperty(ref _filteredCustomers, value);
    }

    public ObservableCollection<Product>? FilteredProducts
    {
        get => _filteredProducts;
        private set => SetProperty(ref _filteredProducts, value);
    }

    public ObservableCollection<ProductCategory>? Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }

    public ProductCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public ObservableCollection<Product>? SelectedProducts
    {
        get => _selectedProducts;
        private set => SetProperty(ref _selectedProducts, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public string? SelectedCustomerName
    {
        get => _selectedCustomerName;
        set => SetProperty(ref _selectedCustomerName, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<ProductVariant>? ProductVariants
    {
        get => _productVariants;
        set => SetProperty(ref _productVariants, value);
    }
    public ObservableCollection<Order>? Orders
    {
        get => _orders;
        set => SetProperty(ref _orders, value);
    }

    public ObservableCollection<OrderItem>? OrdersItems
    {
        get => _ordersItems;
        set => SetProperty(ref _ordersItems, value);
        
    }

    public List<int> SelectedVariantIds
    {
        get => _selectedVariantIds;
        set => SetProperty(ref _selectedVariantIds, value);
    }


    public SellViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        FilteredProducts = new();
        Categories = new();
        OrdersItems = new ObservableCollection<OrderItem>();
        FilteredCustomers = new ObservableCollection<Customer>();
        OrdersItems.CollectionChanged += OrdersItems_CollectionChanged;


        PropertyChanged += OnPropertyChanged;
    }

    protected async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {

        if (e.PropertyName == nameof(SelectedCategory) || e.PropertyName == nameof(SearchTerm))
        {
            await LoadProductsAsync();
        }

        if (e.PropertyName == nameof(CustomerSearchTerm))
        {
            await LoadCustomersAsync();
        }

    }

    private void OrdersItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalPrice)); // Update TotalPrice when collection changes

        if (e.NewItems != null)
        {
            foreach (OrderItem item in e.NewItems)
            {
                // Attach PropertyChanged event to new items
                if (item is INotifyPropertyChanged notifyItem)
                {
                    notifyItem.PropertyChanged += OrderItem_PropertyChanged;
                }
            }
        }

        if (e.OldItems != null)
        {
            foreach (OrderItem item in e.OldItems)
            {
                // Detach PropertyChanged event from removed items
                if (item is INotifyPropertyChanged notifyItem)
                {
                    notifyItem.PropertyChanged -= OrderItem_PropertyChanged;
                }
            }
        }
    }

    private void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderItem.Quantity) || e.PropertyName == nameof(OrderItem.UnitPrice))
        {
            OnPropertyChanged(nameof(TotalPrice)); // Update TotalPrice when item properties change
        }
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();

    }

    public async Task LoadCustomersAsync()
    {
        IsLoading = true;

        try
        {
            var allCustomers = await _unitOfWork.Customers.GetAllAsync();
            var filtered = string.IsNullOrWhiteSpace(CustomerSearchTerm)
                ? allCustomers
                : allCustomers.Where(c =>
                    (c.CustomerName?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Email?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                  ).ToList();

            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredCustomers?.Clear();
                foreach (var customer in filtered)
                {
                    FilteredCustomers?.Add(customer);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi tải khách hàng: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;

        try
        {
            var categoryList = await _unitOfWork.ProductCategories.GetAllAsync();
            var sorted = categoryList.OrderBy(c => c.CategoryName).ToList();

            Debug.WriteLine($"Categories loaded: {string.Join(", ", sorted.Select(c => c.CategoryName))}");

            _dispatcherQueue.TryEnqueue(() =>
            {
                Categories?.Clear();
                foreach (var category in sorted)
                {
                    Categories?.Add(category);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading categories: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;

        try
        {
            var spec = new ProductSpecification(
                searchTerm: SearchTerm,
                categoryId: SelectedCategory?.CategoryId,
                includeDetails: true
            );

            var products = await _unitOfWork.Products.GetAsync(spec);

            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredProducts?.Clear();
                foreach (var product in products)
                {
                    FilteredProducts?.Add(product);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading products: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> CreateOrderAsync(int? customerId)
    {
        try
        {
            var variants = await _unitOfWork.ProductVariants.GetAllAsync();
            var selectedVariants = variants.Where(v => SelectedVariantIds.Contains(v.VariantId)).ToList();

            if (!selectedVariants.Any())
            {
                Debug.WriteLine("Không có biến thể sản phẩm nào được chọn để tạo đơn hàng.");
                return false;
            }

            var newOrder = new Order
            {
                OrderDate = DateTime.Now,
                CustomerId = customerId,
                Subtotal = 0,
                PaymentMethodId = SelectedPaymentMethodId,
                PointUsed = 0,
                Status = SelectedStatus,
                NetTotal = 0
            };

            foreach (var variant in selectedVariants)
            {

                int unitPrice = 100_000;

                var item = new OrderItem
                {
                    VariantId = variant.VariantId,
                    Quantity = 1,
                    UnitPrice = unitPrice
                };

                newOrder.OrderItems.Add(item);
                newOrder.Subtotal += item.Quantity * item.UnitPrice;
            }

            newOrder.NetTotal = newOrder.Subtotal;

            await _unitOfWork.Orders.AddAsync(newOrder);
            var saved = await _unitOfWork.SaveChangesAsync();

            if (saved)
            {
                Debug.WriteLine($"Đơn hàng đã được tạo với {newOrder.OrderItems.Count} sản phẩm.");
                Orders?.Add(newOrder);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi tạo đơn hàng: {ex}");

            return false;
        }
    }

    public async Task GenerateAndOpenReceiptAsync(Order order)
    {
        try
        {
            // Ensure the order is valid
            if (order == null || order.OrderItems == null || !order.OrderItems.Any())
            {
                await ShowErrorDialogAsync("Error", "No order or items found to generate a receipt.");
                return;
            }

            // Generate the PDF file
            string filePath = await GenerateReceiptPdfAsync(order);

            // Open the PDF file
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error generating or opening receipt: {ex.Message}");
            await ShowErrorDialogAsync("Error", "Lỗi tạo hóa đơn.");
        }
    }

    private async Task<string> GenerateReceiptPdfAsync(Order order)
    {
        string fileName = $"Receipt_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

        try
        {
            var storeOwner = (await _unitOfWork.StoreOwners.GetAllAsync()).FirstOrDefault();
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(order.PaymentMethodId);
            string paymentMethodName = paymentMethod?.PaymentMethodName ?? "N/A";

            // Create a new PDF document
            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Receipt";

                // Add a page
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Define fonts
                XFont titleFont = new XFont("Arial", 20);
                XFont regularFont = new XFont("Arial", 12);

                // Draw title
                gfx.DrawString("Receipt", titleFont, XBrushes.Black,
                    new XRect(0, 30, page.Width, 0), XStringFormats.TopCenter);

                // Draw shop information
                gfx.DrawString($"Tên chủ: {storeOwner.OwnerName}", regularFont, XBrushes.Black,
                    new XRect(40, 70, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Địa chỉ: {storeOwner.Address}", regularFont, XBrushes.Black,
                    new XRect(40, 90, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Số điện thoại: {storeOwner.Phone}", regularFont, XBrushes.Black,
                    new XRect(40, 110, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Email: {storeOwner.Email}", regularFont, XBrushes.Black,
                    new XRect(40, 130, page.Width, 0), XStringFormats.TopLeft);

                // Draw order details
                gfx.DrawString($"Order ID: {order.OrderId}", regularFont, XBrushes.Black,
                    new XRect(40, 170, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Ngày: {order.OrderDate:yyyy-MM-dd HH:mm:ss}", regularFont, XBrushes.Black,
                    new XRect(40, 190, page.Width, 0), XStringFormats.TopLeft);

                // Handle long customer names with wrapping
                string customerName = order.Customer?.CustomerName ?? "N/A";
                double maxWidth = page.Width - 80; // Allow some margin
                var wrappedLines = WrapText(gfx, $"Khách: {customerName}", regularFont, maxWidth);

                int yOffset = 210; // Starting Y position for customer name
                foreach (var line in wrappedLines)
                {
                    gfx.DrawString(line, regularFont, XBrushes.Black,
                        new XRect(40, yOffset, page.Width, 0), XStringFormats.TopLeft);
                    yOffset += 15; // Move to the next line
                }

                gfx.DrawString($"Phương thức thanh toán: {paymentMethodName}", regularFont, XBrushes.Black,
                    new XRect(40, yOffset, page.Width, 0), XStringFormats.TopLeft);

                // Draw table header
                yOffset += 40; // Add some space before the table
                gfx.DrawString("Món", regularFont, XBrushes.Black,
                    new XRect(40, yOffset, 200, 0), XStringFormats.TopLeft);
                gfx.DrawString("Màu", regularFont, XBrushes.Black,
                    new XRect(140, yOffset, 100, 0), XStringFormats.TopLeft);
                gfx.DrawString("Size", regularFont, XBrushes.Black,
                    new XRect(240, yOffset, 100, 0), XStringFormats.TopLeft);
                gfx.DrawString("Số Lượng", regularFont, XBrushes.Black,
                    new XRect(340, yOffset, 100, 0), XStringFormats.TopLeft);
                gfx.DrawString("Giá", regularFont, XBrushes.Black,
                    new XRect(440, yOffset, 100, 0), XStringFormats.TopLeft);

                // Draw order items
                yOffset += 20; // Move below the header
                int lineHeight = 20;

                foreach (var item in order.OrderItems)
                {
                    var wrappedProductName = WrapText(gfx, item.Variant.Product.ProductName, regularFont, 90);
                    foreach (var line in wrappedProductName)
                    {
                        gfx.DrawString(line, regularFont, XBrushes.Black,
                            new XRect(40, yOffset, 90, lineHeight), XStringFormats.TopLeft);
                        yOffset += lineHeight;
                    }

                    gfx.DrawString(item.Variant.Color?.ColorName ?? "N/A", regularFont, XBrushes.Black,
                        new XRect(140, yOffset - lineHeight, 100, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(item.Variant.Size?.SizeName ?? "N/A", regularFont, XBrushes.Black,
                        new XRect(240, yOffset - lineHeight, 100, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(item.Quantity.ToString(), regularFont, XBrushes.Black,
                        new XRect(340, yOffset - lineHeight, 100, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString($"{item.UnitPrice:N0}đ", regularFont, XBrushes.Black,
                        new XRect(440, yOffset - lineHeight, 100, lineHeight), XStringFormats.TopLeft);
                }

                // Draw total
                yOffset += 30;
                gfx.DrawString($"Số tiền hàng: {order.Subtotal:N0}đ", regularFont, XBrushes.Black,
                    new XRect(40, yOffset, page.Width, 30), XStringFormats.TopLeft);

                gfx.DrawString($"Giảm giá: {order.Subtotal - order.NetTotal:N0}đ", regularFont, XBrushes.Black,
                    new XRect(40, yOffset + 20, page.Width, 30), XStringFormats.TopLeft);

                gfx.DrawString($"Tổng tiền: {order.NetTotal:N0}đ", titleFont, XBrushes.Black,
                    new XRect(40, yOffset + 40, page.Width, 30), XStringFormats.TopLeft);

                // Save the document
                document.Save(filePath);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error generating PDF: {ex.Message}");
            await ShowErrorDialogAsync("Error", "Failed to generate the receipt PDF.");
            return string.Empty;
        }
    }


    private List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');

        string currentLine = string.Empty;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            if (gfx.MeasureString(testLine, font).Width <= maxWidth)
            {
                currentLine = testLine;
            }
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    public async Task GenerateAndOpenReceiptIfEnabledAsync(Order order)
    {
        if (IsReceiptEnabled)
        {
            await GenerateAndOpenReceiptAsync(order);
        }
        else
        {
            Debug.WriteLine("Receipt generation is disabled.");
        }
    }

    public static int CalculatePoints(int netTotal) => (int)Math.Floor((double)netTotal / 100000);

    public async Task ShowErrorDialog(string title, string message)
    {
        await ShowErrorDialogAsync(title, message);
    }

    public async Task ShowSuccessDialog(string title, string message)
    {
        await ShowSuccessDialogAsync(title, message);
    }
}
