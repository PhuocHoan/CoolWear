using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;
public partial class ColorViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private const int DefaultPageSize = 2; // Số màu sắc trên mỗi trang

    // Backing fields
    private FullObservableCollection<ProductColor>? _filteredColors;
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
    public FullObservableCollection<ProductColor>? FilteredColors
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

        FilteredColors = [];

        LoadColorsCommand = new AsyncRelayCommand(InitializeDataAsync, () => !IsLoading);
        AddColorCommand = new AsyncRelayCommand(PrepareAddDialogAsync, () => !IsLoading);
        EditColorCommand = new AsyncRelayCommand<ProductColor>(PrepareEditDialogAsync, (col) => col != null && !IsLoading);
        DeleteColorCommand = new AsyncRelayCommand<ProductColor>(DeleteColorAsync, (col) => col != null && !IsLoading);
        ImportColorsCommand = new AsyncRelayCommand(ImportAsync, () => !IsLoading); // Stubbed
        ExportColorsCommand = new AsyncRelayCommand(ExportAsync, () => !IsLoading); // Stubbed
        PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, CanGoToPreviousPage);
        NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, CanGoToNextPage);
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
            case nameof(SearchTerm):
            case nameof(PageSize):
                Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Triggering ResetPageAndLoadAsync.");
                await ResetPageAndLoadAsync();
                break;
        }
    }

    // --- Data Loading & Filtering ---
    public async Task InitializeDataAsync()
    {
        if (IsLoading) return;

        SearchTerm = null;
        CurrentPage = 1;

        await LoadColorsAsync();
    }

    public async Task LoadColorsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;

        try
        {
            // 1. Tạo Specification để ĐẾM tổng số item khớp bộ lọc
            var countSpec = new ColorSpecification(
                searchTerm: SearchTerm
            );
            TotalItems = await _unitOfWork.ProductColors.CountAsync(countSpec);

            // 2. Tính toán phân trang
            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            // Đảm bảo trang hiện tại hợp lệ
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            // 3. Tạo Specification để LẤY dữ liệu cho trang hiện tại
            int skip = (CurrentPage - 1) * PageSize;
            var dataSpec = new ColorSpecification(
                searchTerm: SearchTerm,
                skip: skip,
                take: PageSize
            );

            // 4. Lấy dữ liệu từ UnitOfWork
            var colors = await _unitOfWork.ProductColors.GetAsync(dataSpec);

            // 5. Cập nhật FullObservableCollection để UI hiển thị
            FilteredColors?.Clear(); // Xóa dữ liệu trang cũ
            if (colors != null)
            {
                foreach (var color in colors)
                {
                    FilteredColors?.Add(color);
                }
            }

            // 6. Cập nhật trạng thái hiển thị thông báo rỗng
            ShowEmptyMessage = TotalItems == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LỖI Tải Màu sắc: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách màu sắc: {ex.Message}");
            // Đặt lại trạng thái khi có lỗi
            FilteredColors?.Clear();
            TotalItems = 0;
            TotalPages = 1;
            CurrentPage = 1;
            ShowEmptyMessage = true;
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
    public async Task PrepareAddDialogAsync()
    {
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
    private async Task PrepareEditDialogAsync(ProductColor? colorToEdit)
    {
        if (colorToEdit == null) return;

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
    private async Task DeleteColorAsync(ProductColor? color)
    {
        if (color == null) return;

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
                // Nếu xóa màu sắc mà đang có biến thể sản phẩm sử dụng thì màu sắc của biến thể sản phẩm đó sẽ được đặt lại thành null
                // Có thể đổi màu sắc của biến thể sản phẩm từ null thành màu sắc khác
                await _unitOfWork.ProductColors.DeleteAsync(color);
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

    private async Task ImportAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}
