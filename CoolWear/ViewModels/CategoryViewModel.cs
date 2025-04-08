using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class CategoryViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private const int DefaultPageSize = 2; // Số danh mục trên mỗi trang

    // Backing fields
    private FullObservableCollection<ProductCategory>? _filteredCategories;
    private ObservableCollection<string>? _productTypes; // For filter product type
    private string? _selectedProductType;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // --- Pagination Fields ---
    private int _currentPage = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalItems;
    private int _totalPages = 1; // Khởi tạo là 1

    // --- Dialog Data Properties ---
    private string _dialogCategoryName = "";
    private string _dialogProductType = "";
    private string _dialogTitle = "Thêm Danh Mục";
    private bool _isDialogSaving = false; // Flag cho biết dialog đang lưu

    /// <summary>
    /// Danh mục đang được chỉnh sửa (null nếu đang thêm mới).
    /// </summary>
    private ProductCategory? _editingCategory = null;

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
        set => SetProperty(ref _selectedProductType, value);
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
    public string DialogCategoryName
    {
        get => _dialogCategoryName;
        set => SetProperty(ref _dialogCategoryName, value);
    }
    public string DialogProductType
    {
        get => _dialogProductType;
        set => SetProperty(ref _dialogProductType, value);
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
    public ICommand LoadCategoriesCommand { get; } // Sẽ gọi LoadCategoriesAsync
    public ICommand AddCategoryCommand { get; }
    public ICommand EditCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand ImportCategoriesCommand { get; }
    public ICommand ExportCategoriesCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    // --- Event to Signal View ---
    /// <summary>
    /// Event yêu cầu View hiển thị dialog Add/Edit.
    /// </summary>
    public event Func<Task<ContentDialogResult>>? RequestShowDialog; // Dùng Func để có thể await kết quả từ View

    // --- Constructor ---
    public CategoryViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        FilteredCategories = [];
        ProductTypes = [];

        // Gắn Command với phương thức thực thi và kiểm tra điều kiện (CanExecute)
        LoadCategoriesCommand = new AsyncRelayCommand(InitializeDataAsync, CanLoadData);
        // Add/Edit Command để chuẩn bị và yêu cầu hiển thị dialog
        AddCategoryCommand = new AsyncRelayCommand(PrepareAddDialogAsync, CanShowAddDialog);
        EditCategoryCommand = new AsyncRelayCommand<ProductCategory>(PrepareEditDialogAsync, CanShowEditDialog);
        DeleteCategoryCommand = new AsyncRelayCommand<ProductCategory>(DeleteCategoryAsync, CanDeleteCategory);
        ImportCategoriesCommand = new AsyncRelayCommand(ImportAsync, () => !IsLoading);
        ExportCategoriesCommand = new AsyncRelayCommand(ExportAsync, () => !IsLoading);
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
            case nameof(SearchTerm):
            case nameof(SelectedProductType):
            case nameof(PageSize):
                Debug.WriteLine($"ViewModel_PropertyChanged for {e.PropertyName}: Triggering ResetPageAndLoadAsync.");
                await ResetPageAndLoadAsync();
                break;
        }
    }

    private bool CanLoadData() => !IsLoading;
    // --- Data Loading & Filtering ---
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
        CurrentPage = 1;

        await LoadCategoriesAsync();
    }

    /// <summary>
    /// Tải danh sách danh mục cho trang hiện tại dựa trên bộ lọc và phân trang.
    /// </summary>
    public async Task LoadCategoriesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ShowEmptyMessage = false;

        try
        {
            // 1. Tạo Specification để ĐẾM tổng số item khớp bộ lọc
            var countSpec = new CategorySpecification(
                searchTerm: SearchTerm,
                productType: SelectedProductType
            // Không cần skip/take khi đếm
            );
            TotalItems = await _unitOfWork.ProductCategories.CountAsync(countSpec);

            // 2. Tính toán phân trang
            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            // Đảm bảo trang hiện tại hợp lệ
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            // 3. Tạo Specification để LẤY dữ liệu cho trang hiện tại
            int skip = (CurrentPage - 1) * PageSize;
            var dataSpec = new CategorySpecification(
                searchTerm: SearchTerm,
                productType: SelectedProductType,
                skip: skip,
                take: PageSize
            );

            // 4. Lấy dữ liệu từ UnitOfWork
            var categories = await _unitOfWork.ProductCategories.GetAsync(dataSpec);

            // 5. Cập nhật FullObservableCollection để UI hiển thị
            FilteredCategories?.Clear(); // Xóa dữ liệu trang cũ
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    FilteredCategories?.Add(category);
                }
            }

            // 6. Cập nhật trạng thái hiển thị thông báo rỗng
            ShowEmptyMessage = TotalItems == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LỖI Tải Danh mục: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách danh mục: {ex.Message}");
            // Đặt lại trạng thái khi có lỗi
            FilteredCategories?.Clear();
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

    /// <summary>
    /// Reset về trang đầu tiên và tải lại dữ liệu (thường dùng khi bộ lọc thay đổi).
    /// </summary>
    private async Task ResetPageAndLoadAsync()
    {
        if (IsLoading) return; // Ngăn chặn nếu đang tải dở
        CurrentPage = 1; // Đặt lại trang về 1
        await LoadCategoriesAsync(); // Tải lại dữ liệu cho trang 1 với bộ lọc mới
    }

    /// <summary>
    /// Tải các tùy chọn cho bộ lọc Loại sản phẩm (chỉ chạy 1 lần).
    /// </summary>
    private async Task LoadFilterOptionsAsync()
    {
        try
        {
            // Lấy tất cả danh mục chỉ để lấy ra các loại sản phẩm duy nhất
            var allCategories = await _unitOfWork.ProductCategories.GetAllAsync();

            ProductTypes?.Clear();
            ProductTypes?.Add("Tất cả loại sản phẩm"); // Thêm tùy chọn mặc định

            if (allCategories != null && allCategories.Any())
            {
                var distinctTypes = allCategories
                    .Select(c => c.ProductType)
                    .Where(pt => !string.IsNullOrEmpty(pt))
                    .Distinct()
                    .OrderBy(pt => pt);

                foreach (var type in distinctTypes)
                {
                    ProductTypes?.Add(type);
                }
            }
            // Đặt giá trị mặc định cho ComboBox
            SelectedProductType = ProductTypes?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi tải tùy chọn bộ lọc: {ex}");
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
            await LoadCategoriesAsync(); // Tải dữ liệu cho trang trước đó
        }
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private async Task GoToNextPageAsync()
    {
        if (CanGoToNextPage())
        {
            CurrentPage++;
            await LoadCategoriesAsync(); // Tải dữ liệu cho trang tiếp theo
        }
    }

    // --- Dialog Logic ---

    private bool CanShowAddDialog() => !IsLoading; // Kiểm tra trạng thái có thể hiển thị dialog
    /// <summary>
    /// Chuẩn bị dữ liệu và yêu cầu View hiển thị dialog để Thêm mới.
    /// </summary>
    public async Task PrepareAddDialogAsync()
    {
        if (!CanShowAddDialog()) return;
        _editingCategory = null; // Đảm bảo đang ở chế độ Add
        DialogTitle = "Thêm Danh Mục Mới";
        DialogCategoryName = "";   // Xóa trắng các ô nhập liệu
        DialogProductType = "";
        IsDialogSaving = false;    // Đảm bảo nút Lưu của dialog được bật

        // Yêu cầu View hiển thị dialog
        if (RequestShowDialog != null)
        {
            // Gọi event và chờ kết quả từ View (Primary hoặc Cancel)
            ContentDialogResult result = await RequestShowDialog.Invoke();
            Debug.WriteLine($"Add dialog closed with result: {result}");
        }
    }

    private bool CanShowEditDialog(ProductCategory? category) => category != null && !IsLoading; // Kiểm tra trạng thái có thể hiển thị dialog
    /// <summary>
    /// Chuẩn bị dữ liệu và yêu cầu View hiển thị dialog để Chỉnh sửa.
    /// </summary>
    private async Task PrepareEditDialogAsync(ProductCategory? categoryToEdit)
    {
        if (!CanShowEditDialog(categoryToEdit)) return;

        _editingCategory = categoryToEdit; // Lưu lại category đang sửa
        DialogTitle = "Chỉnh Sửa Danh Mục";
        DialogCategoryName = categoryToEdit.CategoryName; // Điền thông tin cũ vào ô nhập liệu
        DialogProductType = categoryToEdit.ProductType;
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
    public async Task<bool> SaveCategoryAsync()
    {
        // --- Validation ---
        if (string.IsNullOrWhiteSpace(DialogCategoryName))
        {
            await ShowErrorDialogAsync("Thiếu thông tin", "Tên danh mục không được để trống.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(DialogProductType))
        {
            await ShowErrorDialogAsync("Thiếu thông tin", "Loại sản phẩm không được để trống.");
            return false;
        }

        IsDialogSaving = true; // Bật trạng thái đang lưu (để disable nút Lưu)
        string? errorMsg = null;
        bool saveSuccess = false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (_editingCategory == null) // --- Chế độ Add ---
            {
                // Kiểm tra trùng tên trước khi thêm
                bool nameExists = await _unitOfWork.ProductCategories.AnyAsync(c => c.CategoryName.ToLower() == DialogCategoryName.Trim().ToLower());
                if (nameExists)
                {
                    throw new InvalidOperationException($"Tên danh mục '{DialogCategoryName}' đã tồn tại.");
                }

                var newCategory = new ProductCategory
                {
                    CategoryName = DialogCategoryName.Trim(),
                    ProductType = DialogProductType.Trim()
                };
                await _unitOfWork.ProductCategories.AddAsync(newCategory);
                Debug.WriteLine("Yêu cầu thêm danh mục mới.");
            }
            else // --- Chế độ Edit ---
            {
                // Kiểm tra trùng tên (loại trừ chính nó)
                bool nameExists = await _unitOfWork.ProductCategories.AnyAsync(c =>
                   c.CategoryName.ToLower() == DialogCategoryName.Trim().ToLower() &&
                   c.CategoryId != _editingCategory.CategoryId); // Loại trừ chính nó
                if (nameExists)
                {
                    throw new InvalidOperationException($"Tên danh mục '{DialogCategoryName}' đã tồn tại.");
                }

                // Cập nhật thông tin trên đối tượng đang sửa
                _editingCategory.CategoryName = DialogCategoryName.Trim();
                _editingCategory.ProductType = DialogProductType.Trim();
                await _unitOfWork.ProductCategories.UpdateAsync(_editingCategory);
                Debug.WriteLine($"Yêu cầu cập nhật danh mục ID: {_editingCategory.CategoryId}.");
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
            Debug.WriteLine($"LỖI Lưu Danh mục (DB): {dbEx}");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = $"Lỗi không mong muốn: {ex.Message}";
            Debug.WriteLine($"LỖI Lưu Danh mục: {ex}");
        }
        finally
        {
            IsDialogSaving = false; // Tắt trạng thái đang lưu
        }

        if (errorMsg != null)
        {
            await ShowErrorDialogAsync("Lỗi Lưu Danh Mục", errorMsg);
        }

        if (saveSuccess)
        {
            await ShowSuccessDialogAsync("Thành Công", "Danh mục đã được lưu.");
            await LoadCategoriesAsync(); // Tải lại danh sách sau khi lưu
        }

        return saveSuccess; // Trả về kết quả lưu
    }

    /// <summary>
    /// Cập nhật trạng thái CanExecute cho các command phân trang.
    /// </summary>
    private void UpdateCommandStates()
    {
        (LoadCategoriesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AddCategoryCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditCategoryCommand as AsyncRelayCommand<ProductCategory>)?.RaiseCanExecuteChanged();
        (DeleteCategoryCommand as AsyncRelayCommand<ProductCategory>)?.RaiseCanExecuteChanged();
        (ImportCategoriesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportCategoriesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    private bool CanDeleteCategory(ProductCategory? category) => category != null && !IsLoading;
    // --- Command Implementations (Delete, Import, Export) ---
    private async Task DeleteCategoryAsync(ProductCategory? category)
    {
        if (!CanDeleteCategory(category)) return;

        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Danh Mục",
            $"Bạn có chắc muốn xóa danh mục '{category.CategoryName}' không?",
            "Xóa", "Hủy");

        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            bool deletedSuccess = false;
            await _unitOfWork.BeginTransactionAsync(); // Bắt đầu transaction
            try
            {
                // Nếu xóa danh mục mà đang có sản phẩm sử dụng thì danh mục của sản phẩm đó sẽ được đặt lại thành null
                // Có thể đổi danh mục của sản phẩm từ null thành danh mục khác
                await _unitOfWork.ProductCategories.DeleteAsync(category);
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
                Debug.WriteLine($"LỖI Xóa Danh mục (DB): {dbEx}");
                errorMsg = $"Lỗi cơ sở dữ liệu khi xóa: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex) // Bắt lỗi chung
            {
                await _unitOfWork.RollbackTransactionAsync();
                Debug.WriteLine($"LỖI Xóa Danh mục: {ex}");
                errorMsg = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Danh Mục", errorMsg);
                }

                // Nếu xóa thành công, tải lại trang hiện tại
                if (deletedSuccess)
                {
                    await ShowSuccessDialogAsync("Xóa Thành Công", $"Danh mục '{category.CategoryName}' đã được xóa thành công.");
                    await LoadCategoriesAsync();
                }
            }
        }
    }

    private async Task ImportAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}