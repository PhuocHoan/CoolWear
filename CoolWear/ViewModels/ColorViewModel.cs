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
public partial class ColorViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ExcelService _excelService = new ExcelService();

    private bool _isResettingFilters = false;
    private const int DefaultPageSize = 4; // Số màu sắc trên mỗi trang

    // Backing fields
    private ObservableCollection<ProductColor>? _filteredColors;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // --- Pagination Fields ---
    private int _currentPage = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalItems;
    private int _totalPages = 1; // Khởi tạo là 1

    // --- Dialog Data Properties ---
    private string _dialogColorName = "";
    private string _dialogTitle = "Thêm Màu";
    private bool _isDialogSaving = false; // Flag cho biết dialog đang lưu

    /// <summary>
    /// Màu sắc đang được chỉnh sửa (null nếu đang thêm mới).
    /// </summary>
    private ProductColor? _editingColor = null;

    // Public properties for UI binding
    public ObservableCollection<ProductColor>? FilteredColors
    {
        get => _filteredColors;
        private set => SetProperty(ref _filteredColors, value);
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        // Gọi UpdateCommandStates khi IsLoading thay đổi
        set => SetProperty(ref _isLoading, value, onChanged: UpdateCommandStates);
    }

    public bool ShowEmptyMessage
    {
        get => _showEmptyMessage;
        // Gọi UpdateCommandStates khi ShowEmptyMessage thay đổi (ảnh hưởng visibility phân trang)
        set => SetProperty(ref _showEmptyMessage, value, onChanged: UpdateCommandStates);
    }

    // --- Pagination Properties ---
    public int CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value, onChanged: UpdateCommandStates);
    }
    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }
    public int TotalItems
    {
        get => _totalItems;
        private set => SetProperty(ref _totalItems, value); // Chỉ cần cập nhật khi tải dữ liệu
    }
    public int TotalPages
    {
        get => _totalPages;
        // Gọi UpdateCommandStates khi TotalPages thay đổi (ảnh hưởng nút Next/Prev)
        private set => SetProperty(ref _totalPages, value, onChanged: UpdateCommandStates);
    }

    // --- Dialog Properties (for Binding) ---
    public string DialogColorName
    {
        get => _dialogColorName;
        set => SetProperty(ref _dialogColorName, value);
    }
    public string DialogTitle // Title cho dialog
    {
        get => _dialogTitle;
        private set => SetProperty(ref _dialogTitle, value);
    }
    public bool IsDialogSaving // Để disable nút Lưu trong dialog khi đang xử lý
    {
        get => _isDialogSaving;
        private set => SetProperty(ref _isDialogSaving, value);
    }

    // --- Commands ---
    public ICommand LoadColorsCommand { get; }
    public ICommand AddColorCommand { get; }
    public ICommand EditColorCommand { get; }
    public ICommand DeleteColorCommand { get; }
    public ICommand ImportColorsCommand { get; }
    public ICommand ExportColorsCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    // --- Event to Signal View ---
    /// <summary>
    /// Event yêu cầu View hiển thị dialog Add/Edit.
    /// </summary>
    public event Func<Task<ContentDialogResult>>? RequestShowDialog; // Dùng Func để có thể await kết quả từ View

    // --- Constructor ---
    public ColorViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        FilteredColors = [];

        LoadColorsCommand = new AsyncRelayCommand(InitializeDataAsync, CanLoadData);
        AddColorCommand = new AsyncRelayCommand(PrepareAddDialogAsync, CanPrepareAddDialog);
        EditColorCommand = new AsyncRelayCommand<ProductColor>(PrepareEditDialogAsync, CanPrepareEditDialog);
        DeleteColorCommand = new AsyncRelayCommand<ProductColor>(DeleteColorAsync, CanDeleteColor);
        ImportColorsCommand = new AsyncRelayCommand(ImportAsync, () => !IsLoading); // Stubbed
        ExportColorsCommand = new AsyncRelayCommand(ExportAsync, () => !IsLoading); // Stubbed
        PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, CanGoToPreviousPage);
        NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, CanGoToNextPage);

        PropertyChanged += ViewModel_PropertyChanged; // Đăng ký sự kiện PropertyChanged
    }

    // --- Event Handler for Filter Changes ---
    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsLoading || _isResettingFilters)
        {
            Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Ignored because IsLoading is true.");
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(SearchTerm):
            case nameof(PageSize):
                Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Triggering ResetPageAndLoadAsync.");
                await ResetPageAndLoadAsync();
                break;
        }
    }

    // --- Data Loading & Filtering ---
    private bool CanLoadData() => !IsLoading;
    public async Task InitializeDataAsync()
    {
        if (!CanLoadData()) return;
        _isResettingFilters = true; // Đặt lại trạng thái sau khi đã reset

        SearchTerm = null;
        CurrentPage = 1;

        _isResettingFilters = false; // Đặt lại trạng thái sau khi đã reset
        await LoadColorsAsync();
    }

    public async Task LoadColorsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var countSpec = new ColorSpecification(SearchTerm);
            TotalItems = await _unitOfWork.ProductColors.CountAsync(countSpec);

            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;

            var dataSpec = new ColorSpecification(SearchTerm, skip, PageSize);
            var colors = await _unitOfWork.ProductColors.GetAsync(dataSpec);

            // Cập nhật UI trên luồng chính
            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredColors?.Clear();
                if (colors != null)
                {
                    foreach (var color in colors) FilteredColors?.Add(color);
                }
                ShowEmptyMessage = !(FilteredColors?.Any() ?? false);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Colors: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách màu sắc: {ex.Message}");
            ShowEmptyMessage = true;
            FilteredColors?.Clear();
            TotalItems = 0;
            TotalPages = 1;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ResetPageAndLoadAsync()
    {
        if (IsLoading) return; // Ngăn chặn nếu đang tải dở
        CurrentPage = 1; // Đặt lại trang về 1
        await LoadColorsAsync(); // Tải lại dữ liệu cho trang 1 với bộ lọc mới
    }

    // --- Pagination Command Implementations ---
    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private async Task GoToPreviousPageAsync()
    {
        if (CanGoToPreviousPage())
        {
            CurrentPage--;
            await LoadColorsAsync(); // Tải dữ liệu cho trang trước đó
        }
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private async Task GoToNextPageAsync()
    {
        if (CanGoToNextPage())
        {
            CurrentPage++;
            await LoadColorsAsync(); // Tải dữ liệu cho trang tiếp theo
        }
    }

    // --- Dialog Logic ---

    /// <summary>
    /// Chuẩn bị dữ liệu và yêu cầu View hiển thị dialog để Thêm mới.
    /// </summary>
    private bool CanPrepareAddDialog() => !IsLoading; // Chỉ cho phép khi không đang tải
    public async Task PrepareAddDialogAsync()
    {
        if (!CanPrepareAddDialog()) return; // Ngăn chặn nếu đang tải
        _editingColor = null; // Đảm bảo đang ở chế độ Add
        DialogTitle = "Thêm Màu Sắc Mới";
        DialogColorName = "";   // Xóa trắng các ô nhập liệu
        IsDialogSaving = false;    // Đảm bảo nút Lưu của dialog được bật

        // Yêu cầu View hiển thị dialog
        if (RequestShowDialog != null)
        {
            // Gọi event và chờ kết quả từ View (Primary hoặc Cancel)
            ContentDialogResult result = await RequestShowDialog.Invoke();
            Debug.WriteLine($"Add dialog closed with result: {result}");
        }
    }

    /// <summary>
    /// Chuẩn bị dữ liệu và yêu cầu View hiển thị dialog để Chỉnh sửa.
    /// </summary>
    private bool CanPrepareEditDialog(ProductColor? colorToEdit) => !IsLoading && colorToEdit != null;
    private async Task PrepareEditDialogAsync(ProductColor? colorToEdit)
    {
        if (!CanPrepareEditDialog(colorToEdit)) return;

        _editingColor = colorToEdit; // Lưu lại color đang sửa
        DialogTitle = "Chỉnh Sửa Màu Sắc";
        DialogColorName = colorToEdit.ColorName; // Điền thông tin cũ vào ô nhập liệu
        IsDialogSaving = false;

        // Yêu cầu View hiển thị dialog
        if (RequestShowDialog != null)
        {
            ContentDialogResult result = await RequestShowDialog.Invoke();
            Debug.WriteLine($"Edit dialog closed with result: {result}");
        }
    }

    /// <summary>
    /// Phương thức này được gọi từ View (ví dụ: trong PrimaryButtonClick handler) để thực hiện lưu.
    /// </summary>
    /// <returns>True nếu lưu thành công, False nếu có lỗi.</returns>
    public async Task<bool> SaveColorAsync()
    {
        // --- Validation ---
        if (string.IsNullOrWhiteSpace(DialogColorName))
        {
            await ShowErrorDialogAsync("Thiếu thông tin", "Tên màu sắc không được để trống.");
            return false;
        }

        IsDialogSaving = true; // Bật trạng thái đang lưu (để disable nút Lưu)
        string? errorMsg = null;
        bool saveSuccess = false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (_editingColor == null) // --- Chế độ Add ---
            {
                // Kiểm tra trùng tên trước khi thêm
                bool nameExists = await _unitOfWork.ProductColors.AnyAsync(c => c.ColorName.ToLower() == DialogColorName.Trim().ToLower());
                if (nameExists)
                {
                    throw new InvalidOperationException($"Tên màu sắc '{DialogColorName}' đã tồn tại.");
                }

                var newColor = new ProductColor
                {
                    ColorName = DialogColorName.Trim(),
                };
                await _unitOfWork.ProductColors.AddAsync(newColor);
                Debug.WriteLine("Yêu cầu thêm màu sắc mới.");
            }
            else // --- Chế độ Edit ---
            {
                // Kiểm tra trùng tên (loại trừ chính nó)
                bool nameExists = await _unitOfWork.ProductColors.AnyAsync(c =>
                   c.ColorName.ToLower() == DialogColorName.Trim().ToLower() &&
                   c.ColorId != _editingColor.ColorId); // Loại trừ chính nó
                if (nameExists)
                {
                    throw new InvalidOperationException($"Tên màu sắc '{DialogColorName}' đã tồn tại.");
                }

                // Cập nhật thông tin trên đối tượng đang sửa
                _editingColor.ColorName = DialogColorName.Trim();
                await _unitOfWork.ProductColors.UpdateAsync(_editingColor);
                Debug.WriteLine($"Yêu cầu cập nhật màu sắc ID: {_editingColor.ColorId}.");
            }

            bool saved = await _unitOfWork.SaveChangesAsync();
            if (saved)
            {
                await _unitOfWork.CommitTransactionAsync();
                saveSuccess = true;
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                errorMsg = "Không thể lưu thay đổi vào cơ sở dữ liệu.";
            }
        }
        catch (InvalidOperationException opEx) // Bắt lỗi trùng tên.
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = opEx.Message;
        }
        catch (DbUpdateException dbEx)
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}";
            Debug.WriteLine($"LỖI Lưu Màu sắc (DB): {dbEx}");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = $"Lỗi không mong muốn: {ex.Message}";
            Debug.WriteLine($"LỖI Lưu Màu sắc: {ex}");
        }
        finally
        {
            IsDialogSaving = false; // Tắt trạng thái đang lưu
        }

        if (errorMsg != null)
        {
            await ShowErrorDialogAsync("Lỗi Lưu Màu Sắc", errorMsg);
        }

        if (saveSuccess)
        {
            await ShowSuccessDialogAsync("Thành Công", "Màu sắc đã được lưu.");
            await LoadColorsAsync(); // Tải lại danh sách sau khi lưu
        }

        return saveSuccess; // Trả về kết quả lưu
    }

    /// <summary>
    /// Cập nhật trạng thái CanExecute cho các command phân trang.
    /// </summary>
    private void UpdateCommandStates()
    {
        (LoadColorsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AddColorCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditColorCommand as AsyncRelayCommand<ProductColor>)?.RaiseCanExecuteChanged();
        (DeleteColorCommand as AsyncRelayCommand<ProductColor>)?.RaiseCanExecuteChanged();
        (ImportColorsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportColorsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    // --- Command Implementations (Delete, Import, Export) ---
    private bool CanDeleteColor(ProductColor? color) => !IsLoading && color != null;
    private async Task DeleteColorAsync(ProductColor? color)
    {
        if (!CanDeleteColor(color)) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Màu Sắc",
            $"Bạn có chắc muốn xóa màu sắc '{color.ColorName}' không?",
            "Xóa", "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            bool deletedSuccess = false;
            await _unitOfWork.BeginTransactionAsync(); // Bắt đầu transaction
            try
            {
                // --- KIỂM TRA RÀNG BUỘC ORDER ITEM ---
                bool isInUseInOrder = await _unitOfWork.OrderItems.AnyAsync(oi => oi.Variant.ColorId == color.ColorId);
                if (isInUseInOrder)
                {
                    // --- XÓA MỀM ---
                    color.IsDeleted = true;
                    await _unitOfWork.ProductColors.UpdateAsync(color);
                    Debug.WriteLine($"Xóa mềm Màu ID: {color.ColorId} do đang dùng trong đơn hàng.");
                }
                else
                {
                    // --- XÓA CỨNG ---
                    await _unitOfWork.ProductColors.DeleteAsync(color);
                    Debug.WriteLine($"Xóa cứng Màu ID: {color.ColorId}.");
                }
                bool saved = await _unitOfWork.SaveChangesAsync();

                if (saved)
                {
                    await _unitOfWork.CommitTransactionAsync(); // Commit nếu lưu thành công
                    deletedSuccess = true;
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync(); // Rollback nếu không lưu được
                    errorMsg = "Không thể lưu thay đổi xóa vào cơ sở dữ liệu.";
                }
            }
            catch (DbUpdateException dbEx) // Bắt lỗi từ DB (ví dụ: khóa ngoại khác)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"LỖI Xóa Màu sắc (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex) // Bắt lỗi chung
            {
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"LỖI Xóa Màu sắc: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Màu Sắc", errorMsg);
                }

                // Nếu xóa thành công, tải lại trang hiện tại
                if (deletedSuccess)
                {
                    await ShowSuccessDialogAsync("Xóa Thành Công", $"Màu sắc '{color.ColorName}' đã được xóa thành công.");
                    await LoadColorsAsync();
                }
            }
        }
    }

    private async Task ImportAsync()
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
                var colors = _excelService.ImportColorsFromExcel(file.Path);
                foreach (var color in colors)
                {
                    // Add each color to the database
                    await _unitOfWork.ProductColors.AddAsync(color);
                }
                await _unitOfWork.SaveChangesAsync();
                await ShowSuccessDialogAsync("Import Successful", "Nhập file thành công.");
                await LoadColorsAsync(); // Reload the colors
            }
            catch (Exception ex)
            {
                string errorMessage = null;
                await ShowErrorDialogAsync("Import Failed", $"An error occurred while importing: {ex.Message}");
                if (ex.InnerException != null)
                {
                    
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                }
                await ShowErrorDialogAsync("Import Failed", errorMessage);

            }
        }
    }
    private async Task ExportAsync()
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        picker.FileTypeChoices.Add("Excel File", new List<string> { ".xlsx" });

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Microsoft.UI.Xaml.Application.Current).MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            try
            {
                var colors = await _unitOfWork.ProductColors.GetAllAsync();
                _excelService.ExportColorsToExcel(file.Path, colors.ToList());
                await ShowSuccessDialogAsync("Export Successful", "Xuất file thành công.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Export Failed", ex.Message);
            }
        }

    }

}


