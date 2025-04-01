using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
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
    // --- Dependencies ---
    private readonly IUnitOfWork _unitOfWork;

    // --- Backing Fields ---
    private List<Product>? _allProducts = [];
    private FullObservableCollection<Product>? _filteredProducts;
    private FullObservableCollection<ProductCategory>? _categories;
    private FullObservableCollection<ProductSize>? _sizes;
    private FullObservableCollection<ProductColor>? _colors;

    private ProductCategory? _selectedCategory;
    private ProductSize? _selectedSize;
    private ProductColor? _selectedColor;
    private bool _filterInStockOnly = true; // Keep default
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

        try
        {
            var spec = new ProductSpecification(true);
            var products = await _unitOfWork.Products.GetAsync(spec);
            _allProducts = [.. products];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Products: {ex}");
            errorMessage = $"Không thể tải danh sách sản phẩm: {ex.Message}";
        }
        finally
        {
            LoadFilterOptions(_allProducts);

            IsLoading = false;

            ApplyFilters();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", errorMessage);
            }
        }
    }

    private void LoadFilterOptions(List<Product>? products)
    {
        Categories?.Clear();
        Sizes?.Clear();
        Colors?.Clear();

        if (products != null && products.Any())
        {
            var distinctCategories = products.Where(p => p.Category != null)
                                            .Select(p => p.Category)
                                            .DistinctBy(c => c.CategoryId)
                                            .OrderBy(c => c.CategoryName)
                                            .ToList();
            foreach (var cat in distinctCategories) Categories?.Add(cat);

            var distinctSizes = products.SelectMany(p => p.ProductVariants?.Select(v => v.Size) ?? [])
                                        .Where(s => s != null)
                                        .DistinctBy(s => s.SizeId)
                                        .OrderBy(s => s.SizeName)
                                        .ToList();
            foreach (var size in distinctSizes) Sizes?.Add(size);

            var distinctColors = products.SelectMany(p => p.ProductVariants?.Select(v => v.Color) ?? [])
                                         .Where(c => c != null)
                                         .DistinctBy(c => c.ColorId)
                                         .OrderBy(c => c.ColorName)
                                         .ToList();
            foreach (var color in distinctColors) Colors?.Add(color);
        }
    }

    private void ApplyFilters()
    {
        FilteredProducts?.Clear();
        if (_allProducts == null)
        {
            ShowEmptyMessage = !_allProducts?.Any() ?? true;
            return;
        }

        IEnumerable<Product> filtered = _allProducts;

        // Apply filters sequentially
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            string lowerSearch = SearchTerm.Trim();
            filtered = filtered.Where(p => p.ProductName.Contains(lowerSearch, StringComparison.InvariantCultureIgnoreCase) ||
                                            p.ProductId.ToString().Contains(lowerSearch));
        }

        // --- Check for NON-NULL selections before applying filter ---
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

        filtered = FilterInStockOnly
            ? filtered.Where(p => p.ProductVariants.Any(v => v.StockQuantity > 0))
            : filtered.Where(p => p.ProductVariants.Any(v => v.StockQuantity == 0));

        var results = filtered.ToList();
        FilteredProducts?.Clear();
        foreach (var product in results)
        {
            FilteredProducts?.Add(product);
        }

        ShowEmptyMessage = !(FilteredProducts?.Any() ?? false);
    }

    // --- Command Implementations ---
    private async Task AddProductAsync() =>
        // TODO: Navigate to AddEditProductPage/ViewModel
        await ShowNotImplementedDialogAsync("Thêm Sản Phẩm Mới");// On successful add, call await LoadProductsAsync();

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
                await _unitOfWork.Products.DeleteAsync(product);
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
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

        // Avoid delete ProductVariant if it's in an OrderItem
        var deleteVariant = await _unitOfWork.ProductVariants.GetByIdAsync(variant.VariantId);
        var canDelete = !deleteVariant!.OrderItems.Any(item => item.VariantId == variant.VariantId);
        if (!canDelete)
        {
            await ShowErrorDialogAsync("Cảnh báo: Xóa Phiên Bản", "Không được xóa");
            return;
        }

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
}