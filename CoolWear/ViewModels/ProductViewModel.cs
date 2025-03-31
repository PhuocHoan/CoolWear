using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private List<Product>? _allProducts;
    private FullObservableCollection<Product>? _filteredProducts;
    private FullObservableCollection<ProductCategory>? _categories;
    private FullObservableCollection<ProductSize>? _sizes;
    private FullObservableCollection<ProductColor>? _colors;
    private ProductCategory? _selectedCategory;
    private ProductSize? _selectedSize;
    private ProductColor? _selectedColor;
    private bool _filterInStockOnly = true;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

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
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value)) ApplyFilters();
        }
    }

    public ProductSize? SelectedSize
    {
        get => _selectedSize;
        set { if (SetProperty(ref _selectedSize, value)) ApplyFilters(); }
    }

    public ProductColor? SelectedColor
    {
        get => _selectedColor;
        set { if (SetProperty(ref _selectedColor, value)) ApplyFilters(); }
    }

    public bool FilterInStockOnly
    {
        get => _filterInStockOnly;
        set { if (SetProperty(ref _filterInStockOnly, value)) ApplyFilters(); }
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set { if (SetProperty(ref _searchTerm, value)) ApplyFilters(); }
    }

    // If IsLoading = true, prevent ApplyFilters from running
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool ShowEmptyMessage
    {
        get => _showEmptyMessage;
        set => SetProperty(ref _showEmptyMessage, value);
    }

    // --- Commands ---
    public ICommand LoadProductsCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }
    public ICommand EditVariantCommand { get; }
    public ICommand DeleteVariantCommand { get; }
    public ICommand ImportProductsCommand { get; }
    public ICommand ExportProductsCommand { get; }


    // --- Constructor ---
    public ProductViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        // Initialize Collections
        FilteredProducts = [];
        Categories = [];
        Sizes = [];
        Colors = [];

        // Initialize Commands
        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
        AddProductCommand = new AsyncRelayCommand(AddProductAsync); // Stubbed
        EditProductCommand = new AsyncRelayCommand<Product>(EditProductAsync); // Stubbed
        DeleteProductCommand = new AsyncRelayCommand<Product>(DeleteProductAsync);
        EditVariantCommand = new AsyncRelayCommand<ProductVariant>(EditVariantAsync); // Stubbed
        DeleteVariantCommand = new AsyncRelayCommand<ProductVariant>(DeleteVariantAsync);
        ImportProductsCommand = new AsyncRelayCommand(ImportProductsAsync); // Stubbed
        ExportProductsCommand = new AsyncRelayCommand(ExportProductsAsync); // Stubbed
    }

    // --- Data Loading and Filtering ---
    public async Task LoadProductsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;
        FilteredProducts?.Clear();
        string errorMessage = string.Empty;
        List<Product>? loadedProducts = null; // Temporary variable

        try
        {
            var spec = new ProductSpecification(true);
            var products = await _unitOfWork.Products.GetAsync(spec);
            loadedProducts = [.. products]; // Load into temporary list first
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Products: {ex}");
            errorMessage = $"Không thể tải danh sách sản phẩm: {ex.Message}";
            _allProducts = []; // Ensure list is not null on error
                               // Don't ApplyFilters here on error yet
        }
        finally
        {
            _allProducts = loadedProducts ?? []; // Assign loaded data (or empty list) to the main backing field

            // Load filter options AFTER data is potentially available
            LoadFilterOptions(_allProducts);

            IsLoading = false; // Set loading state OFF *BEFORE* applying filters

            ApplyFilters(); // Apply filters AFTER loading is complete and IsLoading is false

            if (!string.IsNullOrEmpty(errorMessage))
            {
                await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", errorMessage);
                // ApplyFilters() was already called, potentially showing empty message correctly
            }
            // *** END CHANGE AREA ***
        }
    }

    private void LoadFilterOptions(List<Product>? products)
    {
        // Use dispatcher if this might be called from non-UI thread in other scenarios
        Categories?.Clear();
        Sizes?.Clear();
        Colors?.Clear();

        // Add "All" options (represented by null selection in ComboBox)
        // The ComboBox PlaceholderText handles the visual representation of "All"

        if (products != null && products.Any())
        {
            var distinctCategories = products.Where(p => p.Category != null)
                                            .Select(p => p.Category)
                                            .DistinctBy(c => c.CategoryId)
                                            .OrderBy(c => c.CategoryName)
                                            .ToList();
            foreach (var cat in distinctCategories) Categories?.Add(cat);

            var distinctSizes = products.SelectMany(p => p.ProductVariants?.Select(v => v.Size) ?? Enumerable.Empty<ProductSize>())
                                        .Where(s => s != null)
                                        .DistinctBy(s => s.SizeId)
                                        .OrderBy(s => s.SizeName) // TODO: Add custom sort logic if needed (S, M, L...)
                                        .ToList();
            foreach (var size in distinctSizes) Sizes?.Add(size);

            var distinctColors = products.SelectMany(p => p.ProductVariants?.Select(v => v.Color) ?? Enumerable.Empty<ProductColor>())
                                         .Where(c => c != null)
                                         .DistinctBy(c => c.ColorId)
                                         .OrderBy(c => c.ColorName)
                                         .ToList();
            foreach (var color in distinctColors) Colors?.Add(color);
        }
        // No need to reset SelectedItem here, null represents "All"
    }

    private void ApplyFilters()
    {
        // Now this check works correctly because IsLoading is false when called after loading finishes
        if (_allProducts == null /* || IsLoading */) // You can even remove the IsLoading check here if you ensure ApplyFilters is only called when not loading
        {
            FilteredProducts?.Clear();
            // Only set ShowEmptyMessage based on whether _allProducts is null/empty
            ShowEmptyMessage = !_allProducts?.Any() ?? true;
            return;
        }

        IEnumerable<Product> filtered = _allProducts;

        // Apply filters sequentially
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            string lowerSearch = SearchTerm.ToLowerInvariant().Trim();
            filtered = filtered.Where(p => p.ProductName.ToLowerInvariant().Contains(lowerSearch) ||
                                            p.ProductId.ToString().Contains(lowerSearch) ||
                                            p.ProductVariants.Any(v => v.VariantId.ToString().Contains(lowerSearch))); // Assuming VariantId acts as SKU
        }

        if (SelectedCategory != null)
        {
            filtered = filtered.Where(p => p.CategoryId == SelectedCategory.CategoryId);
        }

        if (SelectedColor != null)
        {
            filtered = filtered.Where(p => p.ProductVariants.Any(v => v.ColorId == SelectedColor.ColorId));
        }

        if (SelectedSize != null)
        {
            filtered = filtered.Where(p => p.ProductVariants.Any(v => v.SizeId == SelectedSize.SizeId));
        }

        if (FilterInStockOnly)
        {
            filtered = filtered.Where(p => p.ProductVariants.Any(v => v.StockQuantity > 0));
        }

        // Update the bound collection efficiently
        var results = filtered.ToList();
        FilteredProducts?.Clear();
        foreach (var product in results)
        {
            FilteredProducts?.Add(product);
        }

        // Update empty message based on the *final filtered* results
        ShowEmptyMessage = !(FilteredProducts?.Any() ?? false);
    }

    // --- Command Implementations ---
    // (All command implementation methods remain the same)
    private async Task AddProductAsync()
    {
        // TODO: Navigate to AddEditProductPage/ViewModel
        await ShowNotImplementedDialogAsync("Thêm Sản Phẩm Mới");
        // On successful add, call await LoadProductsAsync();
    }

    private async Task EditProductAsync(Product? product)
    {
        if (product == null) return;
        // TODO: Navigate to AddEditProductPage/ViewModel, passing product.ProductId
        await ShowNotImplementedDialogAsync($"Chỉnh Sửa: {product.ProductName}");
        // On successful edit, call await LoadProductsAsync(); or update the item in _allProducts and refilter
    }

    private async Task DeleteProductAsync(Product? product)
    {
        if (product == null) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Sản Phẩm",
            $"Bạn có chắc muốn xóa '{product.ProductName}' và tất cả các phiên bản ({product.ProductVariants.Count}) của nó không? Dữ liệu sẽ mất vĩnh viễn.",
            "Xóa",
            "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            try
            {
                // Use the simple DeleteAsync(entity) which uses Remove()
                await _unitOfWork.Products.DeleteAsync(product);
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    // Remove from the source list and update UI
                    _allProducts?.Remove(product);
                    FilteredProducts?.Remove(product); // Also remove from filtered list directly
                    ShowEmptyMessage = !(FilteredProducts?.Any() ?? false);
                }
                else
                {
                    errorMsg = "Không thể lưu thay đổi vào cơ sở dữ liệu.";
                }
            }
            catch (DbUpdateException dbEx) // Catch potential FK constraint issues if cascade isn't perfect
            {
                Debug.WriteLine($"ERROR Deleting Product (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Deleting Product: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Sản Phẩm", errorMsg);
                    // Optional: Reload data if unsure about state
                    // await LoadProductsAsync(); 
                }
            }
        }
    }

    private async Task EditVariantAsync(ProductVariant? variant)
    {
        if (variant == null) return;
        // TODO: Open a dialog or navigate to edit the specific variant
        await ShowNotImplementedDialogAsync($"Chỉnh Sửa Phiên Bản ID: {variant.VariantId}");
        // On success, find the variant in _allProducts and update its properties, INPC should update UI.
    }

    private async Task DeleteVariantAsync(ProductVariant? variant)
    {
        if (variant == null) return;

        // Find the parent product in the UI's current list for context
        var parentProduct = FilteredProducts?.FirstOrDefault(p => p.ProductId == variant.ProductId);
        if (parentProduct == null) return; // Should not happen if UI is consistent

        string variantDesc = $"{variant.Color?.ColorName ?? "N/A"} - {variant.Size?.SizeName ?? "N/A"} (ID: {variant.VariantId})";

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Phiên Bản",
            $"Bạn có chắc muốn xóa phiên bản {variantDesc} của sản phẩm '{parentProduct.ProductName}' không?",
            "Xóa",
            "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            try
            {
                await _unitOfWork.ProductVariants.DeleteAsync(variant); // Use variant repository
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    // Remove variant from the parent product's FullObservableCollection
                    // This will automatically update the UI's ItemsRepeater for that product
                    parentProduct.ProductVariants.Remove(variant);

                    // Also remove from the master list's corresponding product
                    var productInAllList = _allProducts?.FirstOrDefault(p => p.ProductId == variant.ProductId);
                    productInAllList?.ProductVariants.Remove(variant); // Keep master list consistent

                    // Trigger property changed on parent product if variant count display needs update
                    // (Already handled by Expander header binding to ProductVariants.Count)
                }
                else
                {
                    errorMsg = "Không thể lưu thay đổi vào cơ sở dữ liệu.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                Debug.WriteLine($"ERROR Deleting Variant (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Deleting Variant: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Phiên Bản", errorMsg);
                }
            }
        }
    }

    private async Task ImportProductsAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportProductsAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");

    // --- Helper Methods for Dialogs ---
    // (Dialog helper methods remain the same)
    private XamlRoot? GetXamlRootForDialogs()
    {
        if (Application.Current is App app && app.MainWindow?.Content is FrameworkElement rootElement)
        {
            return rootElement.XamlRoot; // This should now be the XamlRoot from DashboardWindow's content
        }
        Debug.WriteLine("ERROR: Could not obtain XamlRoot for ContentDialog. App, MainWindow, or MainWindow.Content might be null/invalid.");
        return null;
    }

    private async Task<ContentDialogResult> ShowConfirmationDialogAsync(string title, string content, string primaryText = "OK", string closeText = "Cancel")
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return ContentDialogResult.None; // Cannot show dialog

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryText,
            CloseButtonText = closeText,
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };
        return await dialog.ShowAsync();
    }

    private async Task ShowErrorDialogAsync(string title, string message)
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "Đóng",
            XamlRoot = xamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task ShowNotImplementedDialogAsync(string feature)
    {
        var xamlRoot = GetXamlRootForDialogs();
        if (xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = "Chức Năng Chưa Sẵn Sàng",
            Content = $"Chức năng '{feature}' đang được phát triển.",
            CloseButtonText = "Đóng",
            XamlRoot = xamlRoot
        };
        await dialog.ShowAsync();
    }
}