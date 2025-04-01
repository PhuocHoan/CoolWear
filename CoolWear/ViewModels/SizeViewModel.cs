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
public partial class SizeViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;

    // Backing fields
    private List<ProductSize>? _allSizes = [];
    private FullObservableCollection<ProductSize>? _filteredSizes;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // Public properties for UI binding
    public FullObservableCollection<ProductSize>? FilteredSizes
    {
        get => _filteredSizes;
        private set => SetProperty(ref _filteredSizes, value);
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
    public ICommand LoadSizesCommand { get; }
    public ICommand AddSizeCommand { get; }
    public ICommand EditSizeCommand { get; }
    public ICommand DeleteSizeCommand { get; }
    public ICommand ImportSizesCommand { get; }
    public ICommand ExportSizesCommand { get; }

    // --- Constructor ---
    public SizeViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        FilteredSizes = [];

        LoadSizesCommand = new AsyncRelayCommand(LoadSizesAsync);
        AddSizeCommand = new AsyncRelayCommand(AddSizeAsync); // Stubbed
        EditSizeCommand = new AsyncRelayCommand<ProductSize>(EditSizeAsync); // Stubbed
        DeleteSizeCommand = new AsyncRelayCommand<ProductSize>(DeleteSizeAsync);
        ImportSizesCommand = new AsyncRelayCommand(ImportAsync); // Stubbed
        ExportSizesCommand = new AsyncRelayCommand(ExportAsync); // Stubbed
    }

    // --- Data Loading & Filtering ---
    public async Task LoadSizesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;
        FilteredSizes?.Clear(); // Clear before loading
        string errorMessage = string.Empty;

        try
        {
            var colors = await _unitOfWork.ProductSizes.GetAllAsync();
            _allSizes = [.. colors];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Sizes: {ex}");
            errorMessage = $"Không thể tải size: {ex.Message}";
        }
        finally
        {
            IsLoading = false;

            ApplyFilters();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", errorMessage);
            }
        }
    }

    private void ApplyFilters()
    {
        FilteredSizes?.Clear();
        if (_allSizes == null)
        {
            ShowEmptyMessage = true;
            return;
        }

        IEnumerable<ProductSize> filtered = _allSizes;

        // Filter by Search Term (ID or Name)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            string lowerSearch = SearchTerm.Trim().ToLowerInvariant();
            filtered = filtered.Where(c => c.SizeName.Contains(lowerSearch, StringComparison.InvariantCultureIgnoreCase) ||
                                            c.SizeId.ToString().Contains(lowerSearch));
        }

        // Update the observable collection
        var results = filtered.ToList();
        if (results.Any())
        {
            foreach (var color in results)
            {
                FilteredSizes?.Add(color);
            }
        }

        ShowEmptyMessage = !(FilteredSizes?.Any() ?? false);
    }

    // --- Command Implementations ---

    private async Task AddSizeAsync() =>
        // TODO: Implement logic to show an Add/Edit dialog or navigate to a page
        await ShowNotImplementedDialogAsync("Thêm Size");// On success: await LoadSizesAsync();

    private async Task EditSizeAsync(ProductSize? color)
    {
        if (color == null) return;
        // TODO: Implement logic to show an Add/Edit dialog or navigate, passing color details
        await ShowNotImplementedDialogAsync($"Chỉnh Sửa: {color.SizeName}");
        // On success: Find color in _allSizes and update its properties, or reload: await LoadSizesAsync();
    }

    private async Task DeleteSizeAsync(ProductSize? color)
    {
        if (color == null) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Size",
            $"Bạn có chắc muốn xóa size '{color.SizeName}' không? \n(Lưu ý: Hành động này có thể thất bại nếu có sản phẩm đang thuộc size này).",
            "Xóa",
            "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            try
            {
                await _unitOfWork.ProductSizes.DeleteAsync(color);
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    // Remove from local lists
                    _allSizes?.Remove(color);
                    FilteredSizes?.Remove(color);
                    ShowEmptyMessage = !(FilteredSizes?.Any() ?? false);
                }
                else
                {
                    errorMsg = "Không thể lưu thay đổi vào cơ sở dữ liệu.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                Debug.WriteLine($"ERROR Deleting Size (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: Không thể xóa size vì có sản phẩm đang sử dụng. ({dbEx.InnerException?.Message ?? dbEx.Message})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Deleting Size: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Size", errorMsg);
                }
            }
        }
    }

    private async Task ImportAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}
