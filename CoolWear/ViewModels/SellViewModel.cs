using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.UI.Dispatching;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public partial class SellViewModel : ViewModelBase
{
    private readonly DispatcherQueue _dispatcherQueue;
    public IUnitOfWork UnitOfWork { get; }
    private ObservableCollection<Product>? _filteredProducts;
    private ObservableCollection<ProductCategory>? _categories;
    private ObservableCollection<ProductVariant>? _productVariants;
    private int _selectedPaymentMethodId = 1;
    private string _selectedStatus = "Hoàn thành";
    private ProductCategory? _selectedCategory;
    private string? _searchTerm;
    private bool _isLoading;
    private ObservableCollection<Product>? _selectedProducts = [];
    private Product? _selectedProduct;
    public bool IsProductSelected => SelectedProduct != null;

    private ObservableCollection<Order>? _orders = [];
    private ObservableCollection<OrderItem>? _ordersItems = [];
    private ObservableCollection<Customer>? _filteredCustomers;
    private bool _isReceiptEnabled;
    private List<int> _selectedVariantIds = [];

    private string? _customerSearchTerm;
    private string? _selectedCustomerName;
    public int Points { get; set; }
    private int? _selectedCustomerPoints;
    private int _pointInput;
    private int _pointUsed;
    private decimal _netTotal;

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
        UnitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        FilteredProducts = [];
        Categories = [];
        OrdersItems = [];
        FilteredCustomers = [];
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
        OnPropertyChanged(nameof(TotalPrice)); // Cập nhật TotalPrice khi collection thay đổi

        if (e.NewItems != null)
        {
            foreach (OrderItem item in e.NewItems)
            {
                // Đính kèm sự kiện PropertyChanged vào các mục mới
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
                // Gỡ bỏ sự kiện PropertyChanged khỏi các mục đã bị xóa
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
            OnPropertyChanged(nameof(TotalPrice)); // Cập nhật TotalPrice khi thuộc tính của mục thay đổi
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
            var dataSpec = new CustomerSpecification();
            var allCustomers = await UnitOfWork.Customers.GetAsync(dataSpec);
            var filtered = string.IsNullOrWhiteSpace(CustomerSearchTerm)
                ? allCustomers
                : [.. allCustomers.Where(c =>
                    (c.CustomerName?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Email?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                  )];

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
            var categoryList = await UnitOfWork.ProductCategories.GetAllAsync();
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

            var products = await UnitOfWork.Products.GetAsync(spec);

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

    public async Task GenerateAndOpenReceiptAsync(Order order)
    {
        try
        {
            // Đảm bảo đơn hàng hợp lệ
            if (order == null || order.OrderItems == null || !order.OrderItems.Any())
            {
                await ShowErrorDialogAsync("Error", "Không tìm thấy đơn hàng hoặc mục nào để tạo hóa đơn.");
                return;
            }

            // Tạo file PDF
            string filePath = await GenerateReceiptPdfAsync(order);

            // Mở file PDF
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
            Debug.WriteLine($"Lỗi khi tạo hoặc mở hóa đơn: {ex.Message}");
            await ShowErrorDialogAsync("Error", "Lỗi tạo hóa đơn.");
        }
    }

    private async Task<string> GenerateReceiptPdfAsync(Order order)
    {
        string fileName = $"Receipt_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

        try
        {
            var storeOwner = (await UnitOfWork.StoreOwners.GetAllAsync()).FirstOrDefault();
            var paymentMethod = await UnitOfWork.PaymentMethods.GetByIdAsync(order.PaymentMethodId);
            string paymentMethodName = paymentMethod?.PaymentMethodName ?? "N/A";

            // Tạo tài liệu PDF mới
            using (PdfDocument document = new())
            {
                document.Info.Title = "Receipt";

                // Thêm trang
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Định nghĩa font
                XFont titleFont = new("Arial", 20);
                XFont regularFont = new("Arial", 12);

                // Vẽ tiêu đề
                gfx.DrawString("Receipt", titleFont, XBrushes.Black,
                    new XRect(0, 30, page.Width, 0), XStringFormats.TopCenter);

                // Vẽ thông tin cửa hàng
                gfx.DrawString($"Tên chủ: {storeOwner!.OwnerName}", regularFont, XBrushes.Black,
                    new XRect(40, 70, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Địa chỉ: {storeOwner.Address}", regularFont, XBrushes.Black,
                    new XRect(40, 90, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Số điện thoại: {storeOwner.Phone}", regularFont, XBrushes.Black,
                    new XRect(40, 110, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Email: {storeOwner.Email}", regularFont, XBrushes.Black,
                    new XRect(40, 130, page.Width, 0), XStringFormats.TopLeft);

                // Vẽ chi tiết đơn hàng
                gfx.DrawString($"Order ID: {order.OrderId}", regularFont, XBrushes.Black,
                    new XRect(40, 170, page.Width, 0), XStringFormats.TopLeft);
                gfx.DrawString($"Ngày: {order.OrderDate:yyyy-MM-dd HH:mm:ss}", regularFont, XBrushes.Black,
                    new XRect(40, 190, page.Width, 0), XStringFormats.TopLeft);

                // Xử lý tên khách hàng dài với việc ngắt dòng
                string customerName = order.Customer?.CustomerName ?? "N/A";
                double maxWidth = page.Width - 80; // Chừa một số lề
                var wrappedLines = WrapText(gfx, $"Khách: {customerName}", regularFont, maxWidth);

                int yOffset = 210; // Vị trí Y bắt đầu cho tên khách hàng
                foreach (var line in wrappedLines)
                {
                    gfx.DrawString(line, regularFont, XBrushes.Black,
                        new XRect(40, yOffset, page.Width, 0), XStringFormats.TopLeft);
                    yOffset += 15; // Di chuyển đến dòng tiếp theo
                }

                gfx.DrawString($"Phương thức thanh toán: {paymentMethodName}", regularFont, XBrushes.Black,
                    new XRect(40, yOffset, page.Width, 0), XStringFormats.TopLeft);

                // Vẽ tiêu đề bảng
                yOffset += 40; // Thêm một số khoảng trống trước bảng
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

                // Vẽ các mục đơn hàng
                yOffset += 20; // Di chuyển xuống dưới tiêu đề
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

                // Vẽ tổng cộng
                yOffset += 30;
                gfx.DrawString($"Số tiền hàng: {order.Subtotal:N0}đ", regularFont, XBrushes.Black,
                    new XRect(40, yOffset, page.Width, 30), XStringFormats.TopLeft);

                gfx.DrawString($"Giảm giá: {order.Subtotal - order.NetTotal:N0}đ", regularFont, XBrushes.Black,
                    new XRect(40, yOffset + 20, page.Width, 30), XStringFormats.TopLeft);

                gfx.DrawString($"Tổng tiền: {order.NetTotal:N0}đ", titleFont, XBrushes.Black,
                    new XRect(40, yOffset + 40, page.Width, 30), XStringFormats.TopLeft);

                // Lưu tài liệu
                document.Save(filePath);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi tạo PDF: {ex.Message}");
            await ShowErrorDialogAsync("Error", "Lỗi tạo hóa đơn PDF.");
            return string.Empty;
        }
    }


    private static List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
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
            Debug.WriteLine("Tạo hóa đơn đã bị vô hiệu hóa.");
        }
    }

    public static int CalculatePoints(int netTotal) => (int)Math.Floor((double)netTotal / 100000);

    public static async Task ShowErrorDialog(string title, string message) => await ShowErrorDialogAsync(title, message);

    public static async Task ShowSuccessDialog(string title, string message) => await ShowSuccessDialogAsync(title, message);
}
