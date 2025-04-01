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
public partial class ColorViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;

    // Backing fields
    private List<ProductColor>? _allColors = [];
    private FullObservableCollection<ProductColor>? _filteredColors;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // Public properties for UI binding
    public FullObservableCollection<ProductColor>? FilteredColors
    {
        get => _filteredColors;
        private set => SetProperty(ref _filteredColors, value);
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
    public ICommand LoadColorsCommand { get; }
    public ICommand AddColorCommand { get; }
    public ICommand EditColorCommand { get; }
    public ICommand DeleteColorCommand { get; }
    public ICommand ImportColorsCommand { get; }
    public ICommand ExportColorsCommand { get; }

    // --- Constructor ---
    public ColorViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        FilteredColors = [];

        LoadColorsCommand = new AsyncRelayCommand(LoadColorsAsync);
        AddColorCommand = new AsyncRelayCommand(AddColorAsync); // Stubbed
        EditColorCommand = new AsyncRelayCommand<ProductColor>(EditColorAsync); // Stubbed
        DeleteColorCommand = new AsyncRelayCommand<ProductColor>(DeleteColorAsync);
        ImportColorsCommand = new AsyncRelayCommand(ImportAsync); // Stubbed
        ExportColorsCommand = new AsyncRelayCommand(ExportAsync); // Stubbed
    }

    // --- Data Loading & Filtering ---
    public async Task LoadColorsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;
        FilteredColors?.Clear(); // Clear before loading
        string errorMessage = string.Empty;

        try
        {
            var colors = await _unitOfWork.ProductColors.GetAllAsync();
            _allColors = [.. colors];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Colors: {ex}");
            errorMessage = $"Không thể tải màu sắc: {ex.Message}";
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
        FilteredColors?.Clear();
        if (_allColors == null)
        {
            ShowEmptyMessage = true;
            return;
        }

        IEnumerable<ProductColor> filtered = _allColors;

        // Filter by Search Term (ID or Name)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            string lowerSearch = SearchTerm.Trim().ToLowerInvariant();
            filtered = filtered.Where(c => c.ColorName.Contains(lowerSearch, StringComparison.InvariantCultureIgnoreCase) ||
                                            c.ColorId.ToString().Contains(lowerSearch));
        }

        // Update the observable collection
        var results = filtered.ToList();
        if (results.Any())
        {
            foreach (var color in results)
            {
                FilteredColors?.Add(color);
            }
        }

        ShowEmptyMessage = !(FilteredColors?.Any() ?? false);
    }

    // --- Command Implementations ---

    private async Task AddColorAsync() =>
        // TODO: Implement logic to show an Add/Edit dialog or navigate to a page
        await ShowNotImplementedDialogAsync("Thêm Màu");// On success: await LoadColorsAsync();

    private async Task EditColorAsync(ProductColor? color)
    {
        if (color == null) return;
        // TODO: Implement logic to show an Add/Edit dialog or navigate, passing color details
        await ShowNotImplementedDialogAsync($"Chỉnh Sửa: {color.ColorName}");
        // On success: Find color in _allColors and update its properties, or reload: await LoadColorsAsync();
    }

    private async Task DeleteColorAsync(ProductColor? color)
    {
        if (color == null) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Màu Sắc",
            $"Bạn có chắc muốn xóa màu sắc '{color.ColorName}' không? \n(Lưu ý: Hành động này có thể thất bại nếu có sản phẩm đang thuộc màu sắc này).",
            "Xóa",
            "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            try
            {
                await _unitOfWork.ProductColors.DeleteAsync(color);
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    // Remove from local lists
                    _allColors?.Remove(color);
                    FilteredColors?.Remove(color);
                    ShowEmptyMessage = !(FilteredColors?.Any() ?? false);
                }
                else
                {
                    errorMsg = "Không thể lưu thay đổi vào cơ sở dữ liệu.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                Debug.WriteLine($"ERROR Deleting Color (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: Không thể xóa màu sắc vì có sản phẩm đang sử dụng. ({dbEx.InnerException?.Message ?? dbEx.Message})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Deleting Color: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Màu Sắc", errorMsg);
                }
            }
        }
    }

    private async Task ImportAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}
