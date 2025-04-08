using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    public partial class StockFilterOption : INotifyPropertyChanged
    {
        public string DisplayName { get; set; } = string.Empty;

        // Represents the filter state:
        // null = All
        // true = In Stock
        // false = Out of Stock
        public bool? Value { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString() => DisplayName;
    }

    // --- Dependencies ---
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService; // Inject service

    // --- Constants ---
    private const int DefaultPageSize = 2;

    // --- Backing Fields ---
    private FullObservableCollection<Product>? _filteredProducts;
    private FullObservableCollection<ProductCategory>? _categories;
    private FullObservableCollection<ProductSize>? _sizes;
    private FullObservableCollection<ProductColor>? _colors;
    public FullObservableCollection<StockFilterOption> StockFilterOptions { get; }

    private ProductCategory? _selectedCategory;
    private ProductSize? _selectedSize;
    private ProductColor? _selectedColor;
    private StockFilterOption? _selectedStockFilter;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // --- Pagination Fields ---
    private int _currentPage = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalItems;
    private int _totalPages;

    // --- Properties ---
    public FullObservableCollection<Product>? FilteredProducts
    {
        get => _filteredProducts;
        private set => SetProperty(ref _filteredProducts, value);
    }
    public FullObservableCollection<ProductCategory>? Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }
    public FullObservableCollection<ProductSize>? Sizes
    {
        get => _sizes;
        private set => SetProperty(ref _sizes, value);
    }
    public FullObservableCollection<ProductColor>? Colors
    {
        get => _colors;
        private set => SetProperty(ref _colors, value);
    }

    // --- Filter Properties ---
    public ProductCategory? SelectedCategory
    {
        get => _selectedCategory; set => SetProperty(ref _selectedCategory, value);
    }
    public ProductSize? SelectedSize
    {
        get => _selectedSize; set => SetProperty(ref _selectedSize, value);
    }
    public ProductColor? SelectedColor
    {
        get => _selectedColor; set => SetProperty(ref _selectedColor, value);
    }
    public string? SearchTerm
    {
        get => _searchTerm; set => SetProperty(ref _searchTerm, value);
    }

    public StockFilterOption? SelectedStockFilter
    {
        get => _selectedStockFilter; set => SetProperty(ref _selectedStockFilter, value);
    }

    // If IsLoading = true, prevent ApplyFilters from running
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value, nameof(IsLoading), UpdateCommandStates); }
    public bool ShowEmptyMessage { get => _showEmptyMessage; private set => SetProperty(ref _showEmptyMessage, value); }

    // --- Pagination Properties ---
    public int CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value, nameof(CurrentPage), UpdateCommandStates);
    }
    public int PageSize
    {
        get => _pageSize; set => SetProperty(ref _pageSize, value);
    }
    public int TotalItems
    {
        get => _totalItems;
        private set => SetProperty(ref _totalItems, value);
    }
    public int TotalPages
    {
        get => _totalPages;
        private set => SetProperty(ref _totalPages, value, nameof(TotalPages), UpdateCommandStates);
    }

    // --- Commands ---
    public AsyncRelayCommand LoadDataCommand { get; }
    public AsyncRelayCommand AddProductCommand { get; }
    public AsyncRelayCommand<Product> EditProductCommand { get; }
    public AsyncRelayCommand<Product> DeleteProductCommand { get; }
    public AsyncRelayCommand ImportProductsCommand { get; }
    public AsyncRelayCommand ExportProductsCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }


    // --- Constructor ---
    public ProductViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
    {
        _unitOfWork = unitOfWork;
        _navigationService = navigationService; // Store service

        // Initialize Collections
        FilteredProducts = [];
        Categories = [];
        Sizes = [];
        Colors = [];

        StockFilterOptions =
        [
            new() { DisplayName = "Tất cả", Value = null },        // All
            new() { DisplayName = "Còn hàng", Value = true },       // In Stock
            new() { DisplayName = "Hết hàng", Value = false }       // Out of Stock
        ];

        // Initialize Commands
        LoadDataCommand = new AsyncRelayCommand(InitializeDataAsync, CanLoadData);
        AddProductCommand = new AsyncRelayCommand(AddProductAsync, CanAddProduct);
        EditProductCommand = new AsyncRelayCommand<Product>(EditProductAsync, CanEditProduct);
        DeleteProductCommand = new AsyncRelayCommand<Product>(DeleteProductAsync, CanDeleteProduct);
        ImportProductsCommand = new AsyncRelayCommand(ImportProductsAsync, () => !IsLoading); // Chưa cài đặt
        ExportProductsCommand = new AsyncRelayCommand(ExportProductsAsync, () => !IsLoading); // Chưa cài đặt
        PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, CanGoToPreviousPage);
        NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, CanGoToNextPage);

        PropertyChanged += ViewModel_PropertyChanged;
    }

    // --- Event Handler for Filter Changes ---
    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsLoading)
        {
            Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Ignored because IsLoading is true.");
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(SelectedCategory):
            case nameof(SelectedSize):
            case nameof(SelectedColor):
            case nameof(SearchTerm):
            case nameof(SelectedStockFilter):
            case nameof(PageSize):
                Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Triggering ResetPageAndLoadAsync.");
                await ResetPageAndLoadAsync();
                break;
        }
    }

    // --- Data Loading and Filtering ---
    private bool CanLoadData() => !IsLoading;
    public async Task InitializeDataAsync()
    {
        if (!CanLoadData()) return;
        // Load filter options first
        await LoadFilterOptionsAsync();
        await ResetFiltersAndLoadAsync();
    }

    public async Task ResetFiltersAndLoadAsync()
    {
        if (IsLoading) return;

        SearchTerm = null;
        SelectedCategory = null;
        SelectedSize = null;
        SelectedColor = null;
        SelectedStockFilter = StockFilterOptions.FirstOrDefault(opt => opt.Value == null);
        CurrentPage = 1;

        await LoadProductsAsync();
    }

    // Called by filter property setters to reset page and reload products
    private async Task ResetPageAndLoadAsync()
    {
        if (IsLoading)
        {
            return;
        }
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    public async Task LoadProductsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;

        try
        {
            var countSpec = new ProductSpecification(
                searchTerm: SearchTerm,
                categoryId: SelectedCategory?.CategoryId,
                colorId: SelectedColor?.ColorId,
                sizeId: SelectedSize?.SizeId,
                inStockOnly: SelectedStockFilter?.Value,
                includeDetails: false
            );
            TotalItems = await _unitOfWork.Products.CountAsync(countSpec);

            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            int skip = (CurrentPage - 1) * PageSize;
            var dataSpec = new ProductSpecification(
                searchTerm: SearchTerm,
                categoryId: SelectedCategory?.CategoryId,
                colorId: SelectedColor?.ColorId,
                sizeId: SelectedSize?.SizeId,
                inStockOnly: SelectedStockFilter?.Value,
                skip: skip,
                take: PageSize,
                includeDetails: true
            );

            var products = await _unitOfWork.Products.GetAsync(dataSpec);

            FilteredProducts?.Clear();
            foreach (var product in products)
            {
                FilteredProducts?.Add(product);
            }

            ShowEmptyMessage = TotalItems == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Products: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách sản phẩm: {ex.Message}");
            ShowEmptyMessage = true;
            FilteredProducts?.Clear();
            TotalItems = 0;
            TotalPages = 1;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadFilterOptionsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            // --- Execute sequentially ---
            var categories = await _unitOfWork.ProductCategories.GetAllAsync();
            var sizes = await _unitOfWork.ProductSizes.GetAllAsync();
            var colors = await _unitOfWork.ProductColors.GetAllAsync();

            // --- Populate Collections ---
            Categories?.Clear();
            if (categories != null) // Check if data was actually retrieved
            {
                foreach (var cat in categories.OrderBy(c => c.CategoryName))
                {
                    Categories?.Add(cat);
                }
            }

            Sizes?.Clear();
            if (sizes != null)
            {
                foreach (var size in sizes.OrderBy(s => s.SizeName))
                {
                    Sizes?.Add(size);
                }
            }

            Colors?.Clear();
            if (colors != null)
            {
                foreach (var color in colors.OrderBy(c => c.ColorName))
                {
                    Colors?.Add(color);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Filter Options: {ex}");
            Categories?.Clear();
            Sizes?.Clear();
            Colors?.Clear();
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách bộ lọc: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Pagination Command Implementations ---
    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private async Task GoToPreviousPageAsync()
    {
        if (CanGoToPreviousPage())
        {
            CurrentPage--;
            await LoadProductsAsync();
        }
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private async Task GoToNextPageAsync()
    {
        if (CanGoToNextPage())
        {
            CurrentPage++;
            await LoadProductsAsync();
        }
    }

    // Helper to update command states when IsLoading or Page numbers change
    private void UpdateCommandStates()
    {
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        LoadDataCommand.RaiseCanExecuteChanged();
        AddProductCommand.RaiseCanExecuteChanged();
        EditProductCommand.RaiseCanExecuteChanged();
        DeleteProductCommand.RaiseCanExecuteChanged();
        ImportProductsCommand.RaiseCanExecuteChanged();
        ExportProductsCommand.RaiseCanExecuteChanged();
    }

    private bool CanAddProduct() => !IsLoading;
    private async Task AddProductAsync()
    {
        if (!CanAddProduct()) return;
        Debug.WriteLine("Navigating to AddEditProductPage (Add Mode)");
        bool navigated = _navigationService.Navigate(typeof(Views.AddEditProductPage));
        if (!navigated)
        {
            await ShowErrorDialogAsync("Lỗi Điều Hướng", "Không thể điều hướng đến trang thêm sản phẩm.");
        }
    }

    private bool CanEditProduct(Product? product) => !IsLoading && product != null;
    private async Task EditProductAsync(Product? product)
    {
        if (!CanEditProduct(product)) return;
        Debug.WriteLine($"Navigating to AddEditProductPage (Edit Mode) for ProductId: {product.ProductId}");
        bool navigated = _navigationService.Navigate(typeof(Views.AddEditProductPage), product.ProductId);
        if (!navigated)
        {
            await ShowErrorDialogAsync("Lỗi Điều Hướng", "Không thể điều hướng đến trang sửa sản phẩm.");
        }
    }

    private bool CanDeleteProduct(Product? product) => !IsLoading && product != null;
    private async Task DeleteProductAsync(Product? product)
    {
        if (!CanDeleteProduct(product)) return;

        Product? productWithVariants = null;
        try
        {
            IsLoading = true;
            productWithVariants = _filteredProducts!.FirstOrDefault(p => p.ProductId == product.ProductId);

            if (productWithVariants == null)
            {
                await ShowErrorDialogAsync("Lỗi", "Không tìm thấy sản phẩm trong cơ sở dữ liệu để xóa.");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching product for delete check: {ex}");
            await ShowErrorDialogAsync("Lỗi", $"Không thể tải chi tiết sản phẩm để kiểm tra xóa: {ex.Message}");
            return;
        }
        finally
        {
            // Tạm thời tắt loading nếu chỉ tải để kiểm tra, sẽ bật lại nếu xác nhận xóa
            IsLoading = false;
        }


        // Xác nhận xóa với thông tin số lượng variants đã tải
        int activeVariantCount = productWithVariants.ProductVariants.Count(v => !v.IsDeleted); // Đếm variant đang hoạt động
        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Sản Phẩm",
            $"Bạn có chắc muốn xóa '{productWithVariants.ProductName}' và {activeVariantCount} phiên bản đang hoạt động của nó không?", // Thông báo rõ hơn
            "Xóa",
            "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true; // Bật lại loading để thực hiện xóa
            string? errorMsg = null;
            bool needsReload = false; // Flag để biết có cần tải lại list không

            // --- Bắt đầu Transaction ---
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Kiểm tra Ràng buộc OrderItem
                bool hasVariantInOrder = false;
                if (productWithVariants.ProductVariants.Any()) // Chỉ kiểm tra nếu có variant
                {
                    var variantIds = productWithVariants.ProductVariants.Where(v => !v.IsDeleted).Select(v => v.VariantId).ToList();
                    // Kiểm tra xem có BẤT KỲ variant nào chưa bị xóa mềm trong danh sách ID này tồn tại trong OrderItems không
                    hasVariantInOrder = await _unitOfWork.OrderItems.AnyAsync(oi => variantIds.Contains(oi.VariantId));
                }

                if (hasVariantInOrder)
                {
                    // --- SOFT DELETE ---
                    Debug.WriteLine($"Soft deleting Product ID: {productWithVariants.ProductId} due to variants in orders.");
                    productWithVariants.IsDeleted = true;

                    // Xóa mềm các variant
                    foreach (var variant in productWithVariants.ProductVariants)
                    {
                        variant.IsDeleted = true;
                    }
                    // Đánh dấu Product là đã sửa đổi (bao gồm cả thay đổi IsDeleted của variants)
                    await _unitOfWork.Products.UpdateAsync(productWithVariants);
                }
                else
                {
                    // --- HARD DELETE ---
                    Debug.WriteLine($"Hard deleting Product ID: {productWithVariants.ProductId}.");
                    await _unitOfWork.Products.DeleteAsync(productWithVariants);
                }

                // Lưu Thay đổi
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    // Commit Transaction
                    await _unitOfWork.CommitTransactionAsync();
                    await ShowSuccessDialogAsync("Xóa Thành Công", "Sản phẩm đã được xóa thành công.");
                    needsReload = true; // Đánh dấu cần tải lại danh sách
                }
                else
                {
                    // Rollback Transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    errorMsg = "Không thể lưu thay đổi xóa sản phẩm vào cơ sở dữ liệu. Thay đổi đã được hoàn tác.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback Transaction on Error
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"ERROR Deleting Product (DB): {dbEx}");
                errorMsg = dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503"
                    ? "Lỗi xóa sản phẩm: Không thể xóa vì sản phẩm hoặc biến thể của nó vẫn còn liên kết dữ liệu ở nơi khác (có thể trong đơn hàng chưa xử lý hết hoặc lỗi logic)."
                    : $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}. Thay đổi đã được hoàn tác.";
            }
            catch (Exception ex)
            {
                // Rollback Transaction on Error
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"ERROR Deleting Product: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn khi xóa: {ex.Message}. Thay đổi đã được hoàn tác.";
            }
            finally
            {
                IsLoading = false; // Tắt loading sau khi hoàn tất (thành công hoặc lỗi)

                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Sản Phẩm", errorMsg);
                }

                // Tải lại Dữ liệu nếu xóa thành công
                if (needsReload)
                {
                    await LoadProductsAsync(); // Load lại trang hiện tại
                }
            }
        }
    }
    private async Task ImportProductsAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportProductsAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}