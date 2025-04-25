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
public partial class SizeViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ExcelService _excelService = new ExcelService(); 

    private bool _isResettingFilters = false;
    private const int DefaultPageSize = 2; // Số size trên mỗi trang

    // Backing fields
    private ObservableCollection<ProductSize>? _filteredSizes;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // --- Pagination Fields ---
    private int _currentPage = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalItems;
    private int _totalPages = 1; // Khởi tạo là 1

    // --- Dialog Data Properties ---
    private string _dialogSizeName = "";
    private string _dialogTitle = "Thêm Size";
    private bool _isDialogSaving = false; // Flag cho biết dialog đang lưu

    /// <summary>
    /// Size đang được chỉnh sửa (null nếu đang thêm mới).
    /// </summary>
    private ProductSize? _editingSize = null;

    // Public properties for UI binding
    public ObservableCollection<ProductSize>? FilteredSizes
    {
        get => _filteredSizes;
        private set => SetProperty(ref _filteredSizes, value);
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
    public string DialogSizeName
    {
        get => _dialogSizeName;
        set => SetProperty(ref _dialogSizeName, value);
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
    public ICommand LoadSizesCommand { get; }
    public ICommand AddSizeCommand { get; }
    public ICommand EditSizeCommand { get; }
    public ICommand DeleteSizeCommand { get; }
    public ICommand ImportSizesCommand { get; }
    public ICommand ExportSizesCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    // --- Event to Signal View ---
    /// <summary>
    /// Event yêu cầu View hiển thị dialog Add/Edit.
    /// </summary>
    public event Func<Task<ContentDialogResult>>? RequestShowDialog; // Dùng Func để có thể await kết quả từ View

    // --- Constructor ---
    public SizeViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        FilteredSizes = [];

        LoadSizesCommand = new AsyncRelayCommand(InitializeDataAsync, CanLoadData);
        AddSizeCommand = new AsyncRelayCommand(PrepareAddDialogAsync, CanPrepareAddDialog);
        EditSizeCommand = new AsyncRelayCommand<ProductSize>(PrepareEditDialogAsync, CanPrepareEditDialog);
        DeleteSizeCommand = new AsyncRelayCommand<ProductSize>(DeleteSizeAsync, CanDeleteSize);
        ImportSizesCommand = new AsyncRelayCommand(ImportAsync, () => !IsLoading); // Stubbed
        ExportSizesCommand = new AsyncRelayCommand(ExportAsync, () => !IsLoading); // Stubbed
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
        await LoadSizesAsync();
    }

    public async Task LoadSizesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var countSpec = new SizeSpecification(SearchTerm);
            TotalItems = await _unitOfWork.ProductSizes.CountAsync(countSpec);

            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;

            var dataSpec = new SizeSpecification(SearchTerm, skip, PageSize);
            var sizes = await _unitOfWork.ProductSizes.GetAsync(dataSpec);

            // Cập nhật UI trên luồng chính
            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredSizes?.Clear();
                if (sizes != null)
                {
                    foreach (var size in sizes) FilteredSizes?.Add(size);
                }
                ShowEmptyMessage = !(FilteredSizes?.Any() ?? false);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Sizes: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách size: {ex.Message}");
            ShowEmptyMessage = true;
            FilteredSizes?.Clear();
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
        await LoadSizesAsync(); // Tải lại dữ liệu cho trang 1 với bộ lọc mới
    }

    // --- Pagination Command Implementations ---
    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private async Task GoToPreviousPageAsync()
    {
        if (CanGoToPreviousPage())
        {
            CurrentPage--;
            await LoadSizesAsync(); // Tải dữ liệu cho trang trước đó
        }
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private async Task GoToNextPageAsync()
    {
        if (CanGoToNextPage())
        {
            CurrentPage++;
            await LoadSizesAsync(); // Tải dữ liệu cho trang tiếp theo
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
        _editingSize = null; // Đảm bảo đang ở chế độ Add
        DialogTitle = "Thêm Size Mới";
        DialogSizeName = "";   // Xóa trắng các ô nhập liệu
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
    private bool CanPrepareEditDialog(ProductSize? sizeToEdit) => !IsLoading && sizeToEdit != null;
    private async Task PrepareEditDialogAsync(ProductSize? sizeToEdit)
    {
        if (!CanPrepareEditDialog(sizeToEdit)) return;

        _editingSize = sizeToEdit; // Lưu lại size đang sửa
        DialogTitle = "Chỉnh Sửa Size";
        DialogSizeName = sizeToEdit.SizeName; // Điền thông tin cũ vào ô nhập liệu
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
    public async Task<bool> SaveSizeAsync()
    {
        // --- Validation ---
        if (string.IsNullOrWhiteSpace(DialogSizeName))
        {
            await ShowErrorDialogAsync("Thiếu thông tin", "Tên size không được để trống.");
            return false;
        }

        IsDialogSaving = true; // Bật trạng thái đang lưu (để disable nút Lưu)
        string? errorMsg = null;
        bool saveSuccess = false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (_editingSize == null) // --- Chế độ Add ---
            {
                // Kiểm tra trùng tên trước khi thêm
                bool nameExists = await _unitOfWork.ProductSizes.AnyAsync(c => c.SizeName.ToLower() == DialogSizeName.Trim().ToLower());
                if (nameExists)
                {
                    throw new InvalidOperationException($"Tên size '{DialogSizeName}' đã tồn tại.");
                }

                var newSize = new ProductSize
                {
                    SizeName = DialogSizeName.Trim(),
                };
                await _unitOfWork.ProductSizes.AddAsync(newSize);
                Debug.WriteLine("Yêu cầu thêm size mới.");
            }
            else // --- Chế độ Edit ---
            {
                // Kiểm tra trùng tên (loại trừ chính nó)
                bool nameExists = await _unitOfWork.ProductSizes.AnyAsync(c =>
                   c.SizeName.ToLower() == DialogSizeName.Trim().ToLower() &&
                   c.SizeId != _editingSize.SizeId); // Loại trừ chính nó
                if (nameExists)
                {
                    throw new InvalidOperationException($"Tên size '{DialogSizeName}' đã tồn tại.");
                }

                // Cập nhật thông tin trên đối tượng đang sửa
                _editingSize.SizeName = DialogSizeName.Trim();
                await _unitOfWork.ProductSizes.UpdateAsync(_editingSize);
                Debug.WriteLine($"Yêu cầu cập nhật size ID: {_editingSize.SizeId}.");
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
            Debug.WriteLine($"LỖI Lưu Size (DB): {dbEx}");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = $"Lỗi không mong muốn: {ex.Message}";
            Debug.WriteLine($"LỖI Lưu Size: {ex}");
        }
        finally
        {
            IsDialogSaving = false; // Tắt trạng thái đang lưu
        }

        if (errorMsg != null)
        {
            await ShowErrorDialogAsync("Lỗi Lưu Size", errorMsg);
        }

        if (saveSuccess)
        {
            await ShowSuccessDialogAsync("Thành Công", "Size đã được lưu.");
            await LoadSizesAsync(); // Tải lại danh sách sau khi lưu
        }

        return saveSuccess; // Trả về kết quả lưu
    }

    /// <summary>
    /// Cập nhật trạng thái CanExecute cho các command phân trang.
    /// </summary>
    private void UpdateCommandStates()
    {
        (LoadSizesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AddSizeCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditSizeCommand as AsyncRelayCommand<ProductSize>)?.RaiseCanExecuteChanged();
        (DeleteSizeCommand as AsyncRelayCommand<ProductSize>)?.RaiseCanExecuteChanged();
        (ImportSizesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportSizesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    // --- Command Implementations (Delete, Import, Export) ---
    private bool CanDeleteSize(ProductSize? size) => !IsLoading && size != null;
    private async Task DeleteSizeAsync(ProductSize? size)
    {
        if (!CanDeleteSize(size)) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Size",
            $"Bạn có chắc muốn xóa size '{size.SizeName}' không?",
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
                bool isInUseInOrder = await _unitOfWork.OrderItems.AnyAsync(oi => oi.Variant.SizeId == size.SizeId);
                if (isInUseInOrder)
                {
                    // --- XÓA MỀM ---
                    size.IsDeleted = true;
                    await _unitOfWork.ProductSizes.UpdateAsync(size);
                    Debug.WriteLine($"Xóa mềm Size ID: {size.SizeId} do đang dùng trong đơn hàng.");
                }
                else
                {
                    // --- XÓA CỨNG ---
                    await _unitOfWork.ProductSizes.DeleteAsync(size);
                    Debug.WriteLine($"Xóa cứng Size ID: {size.SizeId}.");
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
                Debug.WriteLine($"LỖI Xóa Size (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex) // Bắt lỗi chung
            {
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"LỖI Xóa Size: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Size", errorMsg);
                }

                // Nếu xóa thành công, tải lại trang hiện tại
                if (deletedSuccess)
                {
                    await ShowSuccessDialogAsync("Xóa Thành Công", $"Size '{size.SizeName}' đã được xóa thành công.");
                    await LoadSizesAsync();
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
                var sizes = _excelService.ImportSizesFromExcel(file.Path);

                // Retrieve existing size names from the database
                var existingSizeNames = (await _unitOfWork.ProductSizes.GetAllAsync())
                    .Select(s => s.SizeName.ToLower())
                    .ToHashSet(); // Use HashSet for efficient lookups

                foreach (var size in sizes)
                {
                    // Check if the size already exists
                    if (!existingSizeNames.Contains(size.SizeName.ToLower()))
                    {
                        // Add the size if it doesn't already exist
                        await _unitOfWork.ProductSizes.AddAsync(size);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await ShowSuccessDialogAsync("Import Successful", "Nhập file thành công.");
                await LoadSizesAsync(); // Reload the sizes
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while importing: {ex.Message}";
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
                var sizes = await _unitOfWork.ProductSizes.GetAllAsync();
                _excelService.ExportSizesToExcel(file.Path, sizes.ToList());
                await ShowSuccessDialogAsync("Export Successful", "Xuất file thành công.");
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while exporting: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                }
                await ShowErrorDialogAsync("Export Failed", errorMessage);
            }
        }
    }
}
