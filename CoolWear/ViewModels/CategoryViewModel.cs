using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class CategoryViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;

    // Backing fields
    private List<ProductCategory>? _allCategories = [];
    private FullObservableCollection<ProductCategory>? _filteredCategories;
    private ObservableCollection<string>? _productTypes; // For filter dropdown
    private string? _selectedProductType;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // Public properties for UI binding
    public FullObservableCollection<ProductCategory>? FilteredCategories
    {
        get => _filteredCategories;
        private set => SetProperty(ref _filteredCategories, value);
    }

    public ObservableCollection<string>? ProductTypes
    {
        get => _productTypes;
        private set => SetProperty(ref _productTypes, value);
    }

    public string? SelectedProductType
    {
        get => _selectedProductType;
        set
        {
            if (SetProperty(ref _selectedProductType, value))
            {
                ApplyFilters();
            }
        }
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                ApplyFilters();
            }
        }
    }

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
    public ICommand LoadCategoriesCommand { get; }
    public ICommand AddCategoryCommand { get; }
    public ICommand EditCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand ImportCategoriesCommand { get; }
    public ICommand ExportCategoriesCommand { get; }

    // --- Constructor ---
    public CategoryViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        FilteredCategories = [];
        ProductTypes = [];

        LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync); // Stubbed
        EditCategoryCommand = new AsyncRelayCommand<ProductCategory>(EditCategoryAsync); // Stubbed
        DeleteCategoryCommand = new AsyncRelayCommand<ProductCategory>(DeleteCategoryAsync);
        ImportCategoriesCommand = new AsyncRelayCommand(ImportAsync); // Stubbed
        ExportCategoriesCommand = new AsyncRelayCommand(ExportAsync); // Stubbed
    }

    // --- Data Loading & Filtering ---
    public async Task LoadCategoriesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;
        FilteredCategories?.Clear(); // Clear before loading
        string errorMessage = string.Empty;

        try
        {
            var categories = await _unitOfWork.ProductCategories.GetAllAsync();
            _allCategories = [.. categories];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Categories: {ex}");
            errorMessage = $"Không thể tải danh mục: {ex.Message}";
        }
        finally
        {
            LoadFilterOptions(_allCategories);

            IsLoading = false;

            ApplyFilters();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", errorMessage);
            }
        }
    }

    private void LoadFilterOptions(List<ProductCategory>? categories)
    {
        ProductTypes?.Clear();
        ProductTypes?.Add("Tất cả loại sản phẩm");

        if (categories != null && categories.Any())
        {
            var distinctTypes = categories
                .Select(c => c.ProductType)
                .Where(pt => !string.IsNullOrEmpty(pt))
                .Distinct()
                .OrderBy(pt => pt);

            foreach (var type in distinctTypes)
            {
                ProductTypes?.Add(type);
            }
        }
        // Set default selection to "All"
        SelectedProductType = ProductTypes?.FirstOrDefault();
    }

    private void ApplyFilters()
    {
        FilteredCategories?.Clear();
        if (_allCategories == null)
        {
            ShowEmptyMessage = true;
            return;
        }

        IEnumerable<ProductCategory> filtered = _allCategories;

        // Filter by Product Type
        if (!string.IsNullOrEmpty(SelectedProductType) && SelectedProductType != "Tất cả loại sản phẩm")
        {
            filtered = filtered.Where(c => c.ProductType == SelectedProductType);
        }

        // Filter by Search Term (ID or Name)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            string lowerSearch = SearchTerm.Trim().ToLowerInvariant();
            filtered = filtered.Where(c => c.CategoryName.Contains(lowerSearch, StringComparison.InvariantCultureIgnoreCase) ||
                                            c.CategoryId.ToString().Contains(lowerSearch));
        }

        // Update the observable collection
        var results = filtered.ToList();
        if (results.Any())
        {
            foreach (var category in results)
            {
                FilteredCategories?.Add(category);
            }
        }

        ShowEmptyMessage = !(FilteredCategories?.Any() ?? false);
    }

    // --- Command Implementations ---

    private async Task AddCategoryAsync() =>
        // TODO: Implement logic to show an Add/Edit dialog or navigate to a page
        await ShowNotImplementedDialogAsync("Thêm Danh Mục");// On success: await LoadCategoriesAsync();

    private async Task EditCategoryAsync(ProductCategory? category)
    {
        if (category == null) return;
        // TODO: Implement logic to show an Add/Edit dialog or navigate, passing category details
        await ShowNotImplementedDialogAsync($"Chỉnh Sửa: {category.CategoryName}");
        // On success: Find category in _allCategories and update its properties, or reload: await LoadCategoriesAsync();
    }

    private async Task DeleteCategoryAsync(ProductCategory? category)
    {
        if (category == null) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Danh Mục",
            $"Bạn có chắc muốn xóa danh mục '{category.CategoryName}' không? \n(Lưu ý: Hành động này có thể thất bại nếu có sản phẩm đang thuộc danh mục này).",
            "Xóa",
            "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            try
            {
                await _unitOfWork.ProductCategories.DeleteAsync(category);
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    // Remove from local lists
                    _allCategories?.Remove(category);
                    FilteredCategories?.Remove(category);
                    ShowEmptyMessage = !(FilteredCategories?.Any() ?? false);
                }
                else
                {
                    errorMsg = "Không thể lưu thay đổi vào cơ sở dữ liệu.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                Debug.WriteLine($"ERROR Deleting Category (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: Không thể xóa danh mục vì có sản phẩm đang sử dụng. ({dbEx.InnerException?.Message ?? dbEx.Message})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Deleting Category: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Danh Mục", errorMsg);
                }
            }
        }
    }

    private async Task ImportAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}