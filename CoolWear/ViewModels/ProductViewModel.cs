using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    public partial class StockFilterOption : INotifyPropertyChanged
    {
        public string DisplayName { get; set; } = string.Empty;

        // Đại diện cho trạng thái bộ lọc:
        // null = Tất cả
        // true = Còn hàng
        // false = Hết hàng
        public bool? Value { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString() => DisplayName;
    }

    // --- Dependencies ---
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService; // Inject service
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ExcelService _excelService = new(); // Thêm instance của ExcelService

    private bool _isResettingFilters = false;

    // --- Constants ---
    private const int DefaultPageSize = 2;

    // --- Backing Fields ---
    private ObservableCollection<Product>? _filteredProducts;
    private ObservableCollection<ProductCategory>? _categories;
    private ObservableCollection<ProductSize>? _sizes;
    private ObservableCollection<ProductColor>? _colors;
    public ObservableCollection<StockFilterOption> StockFilterOptions { get; }

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
    public ObservableCollection<ProductSize>? Sizes
    {
        get => _sizes;
        private set => SetProperty(ref _sizes, value);
    }
    public ObservableCollection<ProductColor>? Colors
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

    // Nếu IsLoading = true, ngăn không cho ApplyFilters chạy
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
    public ICommand LoadProductsCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }
    public ICommand ImportProductsCommand { get; }
    public ICommand ExportProductsCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }


    // --- Constructor ---
    public ProductViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
    {
        _unitOfWork = unitOfWork;
        _navigationService = navigationService; // Lưu trữ service

        // LẤY DISPATCHER QUEUE CỦA LUỒNG UI HIỆN TẠI KHI TẠO VIEWMODEL
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Khởi tạo Collections
        FilteredProducts = [];
        Categories = [];
        Sizes = [];
        Colors = [];

        StockFilterOptions =
        [
            new() { DisplayName = "Tất cả", Value = null },        // Tất cả
            new() { DisplayName = "Còn hàng", Value = true },       // Còn hàng
            new() { DisplayName = "Hết hàng", Value = false }       // Hết hàng
        ];

        // Khởi tạo Commands
        LoadProductsCommand = new AsyncRelayCommand(InitializeDataAsync, CanLoadData);
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
        // Bỏ qua nếu đang loading HOẶC đang reset filter
        if (IsLoading || _isResettingFilters)
        {
            Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Bỏ qua vì IsLoading hoặc IsResettingFilters đang là true.");
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
                Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Kích hoạt ResetPageAndLoadAsync.");
                await ResetPageAndLoadAsync();
                break;
        }
    }

    // --- Data Loading and Filtering ---
    private bool CanLoadData() => !IsLoading;
    public async Task InitializeDataAsync()
    {
        if (!CanLoadData()) return;
        // Tải các tùy chọn bộ lọc trước
        await LoadFilterOptionsAsync();
        await ResetFiltersAndLoadAsync();
    }

    public async Task ResetFiltersAndLoadAsync()
    {
        if (IsLoading) return;
        _isResettingFilters = true; // Bật cờ báo đang reset

        SearchTerm = null;
        SelectedCategory = null;
        SelectedSize = null;
        SelectedColor = null;
        SelectedStockFilter = StockFilterOptions.FirstOrDefault(opt => opt.Value == null);
        CurrentPage = 1;

        _isResettingFilters = false; // TẮT cờ sau khi reset xong
        await LoadProductsAsync();
    }

    // Được gọi bởi các setter của thuộc tính bộ lọc để reset trang và tải lại sản phẩm
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

            var products = await _unitOfWork.Products.GetAsync(dataSpec); // Lệnh này chạy trên luồng nền (background thread), vì có await async

            // === CẬP NHẬT COLLECTION TRÊN LUỒNG UI ===
            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredProducts?.Clear();
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        FilteredProducts?.Add(product);
                    }
                }
                ShowEmptyMessage = !(FilteredProducts?.Any() ?? false);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LỖI Tải Sản Phẩm: {ex}");
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
        List<ProductCategory>? categoriesData = null;
        List<ProductSize>? sizesData = null;
        List<ProductColor>? colorsData = null;
        string? errorMsg = null;

        try
        {
            // --- Tải dữ liệu trên luồng nền ---
            categoriesData = [.. (await _unitOfWork.ProductCategories.GetAllAsync()).OrderBy(c => c.CategoryName)];

            var colorSpec = new ColorSpecification();
            colorsData = [.. (await _unitOfWork.ProductColors.GetAsync(colorSpec)).OrderBy(c => c.ColorName)];

            var sizeSpec = new SizeSpecification();
            sizesData = [.. (await _unitOfWork.ProductSizes.GetAsync(sizeSpec)).OrderBy(s => s.SizeName)];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LỖI Tải Tùy Chọn Bộ Lọc: {ex}");
            errorMsg = $"Không thể tải dữ liệu cho các bộ lọc: {ex.Message}";
            // Không Clear collection ở đây vì đang ở luồng nền
        }
        finally
        {
            IsLoading = false;
        }

        // --- Cập nhật các ObservableCollection trên luồng UI ---
        _dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                Categories?.Clear();
                if (categoriesData != null)
                {
                    foreach (var cat in categoriesData) Categories?.Add(cat);
                }

                Sizes?.Clear();
                if (sizesData != null)
                {
                    foreach (var size in sizesData) Sizes?.Add(size);
                }

                Colors?.Clear();
                if (colorsData != null)
                {
                    foreach (var color in colorsData) Colors?.Add(color);
                }
            }
            catch (Exception uiEx)
            {
                Debug.WriteLine($"Lỗi cập nhật các collection bộ lọc trên luồng UI: {uiEx}");
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                // Dùng _ = để không block luồng UI nếu ShowErrorDialogAsync là async
                _ = ShowErrorDialogAsync("Lỗi Tải Bộ Lọc", errorMsg);
                // Xóa sạch collection nếu có lỗi
                Categories?.Clear();
                Sizes?.Clear();
                Colors?.Clear();
            }
        });
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

    // Helper để cập nhật trạng thái command khi IsLoading hoặc số trang thay đổi
    private void UpdateCommandStates()
    {
        (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (LoadProductsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AddProductCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditProductCommand as AsyncRelayCommand<Product>)?.RaiseCanExecuteChanged();
        (DeleteProductCommand as AsyncRelayCommand<Product>)?.RaiseCanExecuteChanged();
        (ImportProductsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportProductsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    private bool CanAddProduct() => !IsLoading;
    private async Task AddProductAsync()
    {
        if (!CanAddProduct()) return;
        Debug.WriteLine("Điều hướng đến AddEditProductPage (Chế độ Thêm)");
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
        Debug.WriteLine($"Điều hướng đến AddEditProductPage (Chế độ Sửa) cho ProductId: {product!.ProductId}");
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
            productWithVariants = _filteredProducts!.FirstOrDefault(p => p.ProductId == product!.ProductId);

            if (productWithVariants == null)
            {
                await ShowErrorDialogAsync("Lỗi", "Không tìm thấy sản phẩm trong cơ sở dữ liệu để xóa.");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi lấy sản phẩm để kiểm tra xóa: {ex}");
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
                    Debug.WriteLine($"Xóa mềm Product ID: {productWithVariants.ProductId} do có variants trong đơn hàng.");
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
                    Debug.WriteLine($"Xóa cứng Product ID: {productWithVariants.ProductId}.");
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
                Debug.WriteLine($"LỖI Xóa Sản Phẩm (DB): {dbEx}");
                errorMsg = dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503"
                    ? "Lỗi xóa sản phẩm: Không thể xóa vì sản phẩm hoặc biến thể của nó vẫn còn liên kết dữ liệu ở nơi khác (có thể trong đơn hàng chưa xử lý hết hoặc lỗi logic)."
                    : $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}. Thay đổi đã được hoàn tác.";
            }
            catch (Exception ex)
            {
                // Rollback Transaction on Error
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"LỖI Xóa Sản Phẩm: {ex}");
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
    private async Task ImportProductsAsync()
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".xlsx");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Microsoft.UI.Xaml.Application.Current).MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            try
            {
                var products = ExcelService.ImportProductsFromExcel(file.Path);

                // Lấy tên sản phẩm, danh mục, màu sắc và kích thước hiện có từ cơ sở dữ liệu
                var existingProducts = await _unitOfWork.Products.GetAllAsync();
                var existingProductNames = existingProducts
                    .Select(p => p.ProductName.ToLower())
                    .ToHashSet(); // Sử dụng HashSet để tra cứu hiệu quả

                var existingCategoryIds = (await _unitOfWork.ProductCategories.GetAllAsync())
                    .Select(c => c.CategoryId)
                    .ToHashSet();

                var existingColorIds = (await _unitOfWork.ProductColors.GetAllAsync())
                    .Select(c => c.ColorId)
                    .ToHashSet();

                var existingSizeIds = (await _unitOfWork.ProductSizes.GetAllAsync())
                    .Select(s => s.SizeId)
                    .ToHashSet();

                foreach (var product in products)
                {
                    // Kiểm tra xem sản phẩm đã tồn tại theo tên chưa
                    if (existingProductNames.Contains(product.ProductName.ToLower()))
                    {
                        continue; // Bỏ qua các sản phẩm trùng lặp
                    }

                    // Xác thực CategoryId
                    if (product.CategoryId.HasValue && !existingCategoryIds.Contains(product.CategoryId.Value))
                    {
                        continue; // Bỏ qua nếu CategoryId không tồn tại
                    }

                    // Xác thực ProductVariants
                    if (product.ProductVariants.Any(variant =>
                            (variant.ColorId.HasValue && !existingColorIds.Contains(variant.ColorId.Value)) ||
                            (variant.SizeId.HasValue && !existingSizeIds.Contains(variant.SizeId.Value))))
                    {
                        continue; // Bỏ qua nếu bất kỳ ColorId hoặc SizeId nào trong variants không tồn tại
                    }

                    // Kiểm tra các variants trùng lặp trong sản phẩm
                    var duplicateVariants = product.ProductVariants
                        .GroupBy(v => new { v.ColorId, v.SizeId, product.CategoryId })
                        .Where(group => group.Count() > 1)
                        .ToList();

                    if (duplicateVariants.Any())
                    {
                        continue; // Bỏ qua sản phẩm nếu có variants trùng lặp
                    }

                    // Thêm sản phẩm nếu tất cả các xác thực đều đạt
                    await _unitOfWork.Products.AddAsync(product);
                }

                await _unitOfWork.SaveChangesAsync();
                await ShowSuccessDialogAsync("Nhập Thành Công", "Nhập file thành công");
                await LoadProductsAsync(); // Tải lại danh sách sản phẩm
            }
            catch (Exception ex)
            {
                string errorMessage = $"Đã xảy ra lỗi khi nhập: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nLỗi Nội Bộ: {ex.InnerException.Message}";
                }
                await ShowErrorDialogAsync("Nhập Thất Bại", errorMessage);
            }
        }
    }



    private async Task ExportProductsAsync()
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        picker.FileTypeChoices.Add("Tệp Excel", [".xlsx"]);

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Microsoft.UI.Xaml.Application.Current).MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync();
                ExcelService.ExportProductsToExcel(file.Path, [.. products]);
                await ShowSuccessDialogAsync("Xuất Thành Công", "Xuất file thành công.");
            }
            catch (Exception ex)
            {
                string errorMessage = $"Đã xảy ra lỗi khi xuất: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nLỗi Nội Bộ: {ex.InnerException.Message}";
                }
                await ShowErrorDialogAsync("Xuất Thất Bại", errorMessage);
            }
        }
    }

}