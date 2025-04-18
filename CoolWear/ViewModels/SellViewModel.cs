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

namespace CoolWear.ViewModels;

public class SellViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;
    public IUnitOfWork UnitOfWork => _unitOfWork;
    private ObservableCollection<Product>? _filteredProducts;
    private ObservableCollection<ProductCategory>? _categories;
    private ObservableCollection<ProductVariant>? _productVariants;
    private int _selectedPaymentMethodId;
    private string _selectedStatus = "Đang xử lý";
    private ProductCategory? _selectedCategory;
    private string? _searchTerm;
    private bool _isLoading;
    private ObservableCollection<Product>? _selectedProducts = new();
    private Product? _selectedProduct;
    public bool IsProductSelected => SelectedProduct != null;

    private ObservableCollection<Order>? _orders = new();
    private ObservableCollection<OrderItem>? _ordersItems = new();
    private ObservableCollection<Customer>? _filteredCustomers;

    private List<int> _selectedVariantIds = new();

    private decimal _totalPrice;
    private string? _customerSearchTerm;
    private string? _selectedCustomerName;

    public string SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }
    public int SelectedPaymentMethodId
    {
        get => _selectedPaymentMethodId;
        set
        {
            if (SetProperty(ref _selectedPaymentMethodId, value))
            {
                Debug.WriteLine($"SelectedPaymentMethodId updated to: {value}");
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
        if (IsLoading) return;

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

}
