// CustomerViewModel.cs
using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class CustomerViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _isResettingFilters = false;
    private const int DefaultPageSize = 1;

    // --- Backing fields ---
    private ObservableCollection<Customer>? _filteredCustomers;
    private string? _searchTerm;
    private int? _minPoints;
    private int? _maxPoints;
    private DateTimeOffset? _startDate; // Dùng DateTimeOffset cho DatePicker
    private DateTimeOffset? _endDate;
    private bool _isLoading;
    private bool _showEmptyMessage;

    // --- Pagination Fields ---
    private int _currentPage = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalItems;
    private int _totalPages = 1;

    // --- Dialog Data Properties ---
    private string _dialogCustomerName = "";
    private string _dialogEmail = "";
    private string _dialogPhone = "";
    private string _dialogAddress = "";
    private string _dialogTitle = "Thêm Khách Hàng";
    private bool _isDialogSaving = false;
    private Customer? _editingCustomer = null;

    // --- Public properties ---
    public ObservableCollection<Customer>? FilteredCustomers { get => _filteredCustomers; private set => SetProperty(ref _filteredCustomers, value); }
    public string? SearchTerm { get => _searchTerm; set => SetProperty(ref _searchTerm, value); }
    public int? MinPoints { get => _minPoints; set => SetProperty(ref _minPoints, value); }
    public int? MaxPoints { get => _maxPoints; set => SetProperty(ref _maxPoints, value); }
    public DateTimeOffset? StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }
    public DateTimeOffset? EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value, onChanged: UpdateCommandStates); }
    public bool ShowEmptyMessage { get => _showEmptyMessage; private set => SetProperty(ref _showEmptyMessage, value, onChanged: UpdateCommandStates); }

    // --- Pagination Properties ---
    public int CurrentPage { get => _currentPage; private set => SetProperty(ref _currentPage, value, onChanged: UpdateCommandStates); }
    public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }
    public int TotalItems { get => _totalItems; private set => SetProperty(ref _totalItems, value); } // Chỉ cần cập nhật khi tải dữ liệu
    public int TotalPages { get => _totalPages; private set => SetProperty(ref _totalPages, value, onChanged: UpdateCommandStates); } // Gọi UpdateCommandStates khi TotalPages thay đổi (ảnh hưởng nút Next/Prev)

    // --- Dialog Properties ---
    public string DialogCustomerName { get => _dialogCustomerName; set => SetProperty(ref _dialogCustomerName, value); }
    public string DialogEmail { get => _dialogEmail; set => SetProperty(ref _dialogEmail, value); }
    public string DialogPhone { get => _dialogPhone; set => SetProperty(ref _dialogPhone, value); }
    public string DialogAddress { get => _dialogAddress; set => SetProperty(ref _dialogAddress, value); }
    public string DialogTitle { get => _dialogTitle; private set => SetProperty(ref _dialogTitle, value); }
    public bool IsDialogSaving { get => _isDialogSaving; private set => SetProperty(ref _isDialogSaving, value); }

    // --- Commands ---
    public ICommand LoadCustomersCommand { get; }
    public ICommand AddCustomerCommand { get; }
    public ICommand EditCustomerCommand { get; }
    public ICommand DeleteCustomerCommand { get; }
    public ICommand ApplyFiltersCommand { get; } // Command để áp dụng filter
    public ICommand ClearFiltersCommand { get; } // Command để xóa filter
    public ICommand ImportCustomersCommand { get; }
    public ICommand ExportCustomersCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    // --- Event ---
    public event Func<Task<ContentDialogResult>>? RequestShowDialog;

    // --- Constructor ---
    public CustomerViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        FilteredCustomers = [];

        LoadCustomersCommand = new AsyncRelayCommand(InitializeDataAsync, CanLoadData);
        AddCustomerCommand = new AsyncRelayCommand(PrepareAddDialogAsync, CanShowAddDialog);
        EditCustomerCommand = new AsyncRelayCommand<Customer>(PrepareEditDialogAsync, CanShowEditDialog);
        DeleteCustomerCommand = new AsyncRelayCommand<Customer>(DeleteCustomerAsync, CanDeleteCustomer);
        ApplyFiltersCommand = new AsyncRelayCommand(ApplyFiltersAndLoadAsync, () => !IsLoading);
        ClearFiltersCommand = new AsyncRelayCommand(ResetFiltersAndLoadAsync, () => !IsLoading);
        PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, CanGoToPreviousPage);
        NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, CanGoToNextPage);
        ImportCustomersCommand = new AsyncRelayCommand(ImportCustomersAsync, () => !IsLoading);
        ExportCustomersCommand = new AsyncRelayCommand(ExportCustomersAsync, () => !IsLoading);

        PropertyChanged += ViewModel_PropertyChanged;
    }

    // --- Event Handler ---
    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Chỉ reload khi SearchTerm thay đổi tức thì (các filter khác chờ nút Apply)
        if (IsLoading || _isResettingFilters) return;
        switch (e.PropertyName)
        {
            case nameof(SearchTerm):
            case nameof(PageSize): // Thay đổi PageSize cũng nên load lại trang 1
                await ApplyFiltersAndLoadAsync();
                break;
        }
    }

    // --- Data Loading & Filtering ---
    private bool CanLoadData() => !IsLoading;
    public async Task InitializeDataAsync()
    {
        if (!CanLoadData()) return;
        await ResetFiltersAndLoadAsync();
    }

    /// <summary>
    /// Reset TẤT CẢ bộ lọc về mặc định và tải lại trang đầu tiên.
    /// Được gọi bởi nút Clear Filter (hoặc nút Reload).
    /// </summary>
    public async Task ResetFiltersAndLoadAsync()
    {
        if (IsLoading) return;
        _isResettingFilters = true;

        // Đặt lại các property filter trên luồng UI
        SearchTerm = null;
        MinPoints = null;
        MaxPoints = null;
        StartDate = null;
        EndDate = null;
        CurrentPage = 1;

        _isResettingFilters = false;
        // Gọi LoadCustomersAsync với filter đã reset
        await LoadCustomersAsync();
    }

    /// <summary>
    /// Áp dụng các bộ lọc hiện tại và tải lại trang đầu tiên.
    /// Được gọi bởi nút Apply Filters.
    /// </summary>
    private async Task ApplyFiltersAndLoadAsync()
    {
        if (IsLoading) return;
        CurrentPage = 1; // Luôn về trang 1 khi áp dụng bộ lọc mới
        await LoadCustomersAsync();
    }


    /// <summary>
    /// Tải danh sách khách hàng dựa trên các tham số filter và phân trang.
    /// </summary>
    public async Task LoadCustomersAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        DateTime? startDateUtc = StartDate?.Date.ToUniversalTime();
        DateTime? endDateUtc = EndDate?.Date.ToUniversalTime();

        try
        {
            var countSpec = new CustomerSpecification(SearchTerm, MinPoints, MaxPoints, startDateUtc, endDateUtc);
            TotalItems = await _unitOfWork.Customers.CountAsync(countSpec); // Sửa Repository

            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;

            var dataSpec = new CustomerSpecification(SearchTerm, MinPoints, MaxPoints, startDateUtc, endDateUtc, skip, PageSize);
            var customers = await _unitOfWork.Customers.GetAsync(dataSpec);
            // Cập nhật UI trên luồng chính
            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredCustomers?.Clear();
                if (customers != null) { foreach (var c in customers) FilteredCustomers?.Add(c); }
                ShowEmptyMessage = !(FilteredCustomers?.Any() ?? false);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Products: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách khách hàng: {ex.Message}");
            ShowEmptyMessage = true;
            FilteredCustomers?.Clear();
            TotalItems = 0;
            TotalPages = 1;
        }
        finally
        {
            IsLoading = false;
        }

    }

    // --- Pagination ---
    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoToPreviousPage()) return;
        CurrentPage--; // Giảm trang trước
        await LoadCustomersAsync(); // Load trang mới với filter hiện tại
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private async Task GoToNextPageAsync()
    {
        if (!CanGoToNextPage()) return;
        CurrentPage++;
        await LoadCustomersAsync();
    }

    // --- Dialog Logic ---
    private bool CanShowAddDialog() => !IsLoading;
    public async Task PrepareAddDialogAsync()
    {
        if (!CanShowAddDialog()) return;
        _editingCustomer = null;
        DialogTitle = "Thêm Khách Hàng Mới";
        DialogCustomerName = "";
        DialogEmail = "";
        DialogPhone = "";
        DialogAddress = "";
        IsDialogSaving = false;
        if (RequestShowDialog != null) { await RequestShowDialog.Invoke(); }
    }

    private bool CanShowEditDialog(Customer? customer) => customer != null && !IsLoading;
    private async Task PrepareEditDialogAsync(Customer? customerToEdit)
    {
        if (!CanShowEditDialog(customerToEdit)) return;
        _editingCustomer = customerToEdit;
        DialogTitle = "Chỉnh Sửa Khách Hàng";
        DialogCustomerName = customerToEdit!.CustomerName;
        DialogEmail = customerToEdit.Email ?? "N/A"; // Điền dữ liệu cũ
        DialogPhone = customerToEdit.Phone;
        DialogAddress = customerToEdit.Address;
        IsDialogSaving = false;
        if (RequestShowDialog != null) { await RequestShowDialog.Invoke(); }
    }

    public async Task<bool> SaveCustomerAsync()
    {
        // --- Validation ---
        if (string.IsNullOrWhiteSpace(DialogCustomerName)) { await ShowErrorDialogAsync("Thiếu thông tin", "Tên khách hàng không được để trống."); return false; }
        if (string.IsNullOrWhiteSpace(DialogPhone)) { await ShowErrorDialogAsync("Thiếu thông tin", "Số điện thoại không được để trống."); return false; }
        if (string.IsNullOrWhiteSpace(DialogAddress)) { await ShowErrorDialogAsync("Thiếu thông tin", "Địa chỉ không được để trống."); return false; }

        IsDialogSaving = true;
        string? errorMsg = null;
        bool saveSuccess = false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            string name = DialogCustomerName.Trim();
            string? email = string.IsNullOrWhiteSpace(DialogEmail) ? null : DialogEmail.Trim();
            string phone = DialogPhone.Trim();
            string address = DialogAddress.Trim();

            if (_editingCustomer == null) // Add
            {
                // Kiểm tra trùng lặp
                if (email != null)
                {
                    bool emailExists = await _unitOfWork.Customers.AnyAsync(c => c.Email != null && c.Email.ToLower() == email.ToLower() && !c.IsDeleted);
                    if (emailExists) throw new InvalidOperationException($"Email '{email}' đã được sử dụng.");
                }
                bool phoneExists = await _unitOfWork.Customers.AnyAsync(c => c.Phone == phone && !c.IsDeleted);
                if (phoneExists) throw new InvalidOperationException($"Số điện thoại '{phone}' đã được sử dụng.");

                var newCustomer = new Customer
                {
                    CustomerName = name,
                    Email = email,
                    Phone = phone,
                    Address = address,
                    CreateDate = DateTime.UtcNow,
                };
                await _unitOfWork.Customers.AddAsync(newCustomer);
            }
            else // Edit
            {
                // Kiểm tra trùng lặp
                if (email != null)
                {
                    bool emailExists = await _unitOfWork.Customers.AnyAsync(c => c.Email != null && c.Email.ToLower() == email.ToLower() && c.CustomerId != _editingCustomer.CustomerId && !c.IsDeleted);
                    if (emailExists) throw new InvalidOperationException($"Email '{email}' đã được sử dụng.");
                }
                bool phoneExists = await _unitOfWork.Customers.AnyAsync(c => c.Phone == phone && c.CustomerId != _editingCustomer.CustomerId && !c.IsDeleted);
                if (phoneExists) throw new InvalidOperationException($"Số điện thoại '{phone}' đã được sử dụng.");

                _editingCustomer.CustomerName = name;
                _editingCustomer.Email = email;
                _editingCustomer.Phone = phone;
                _editingCustomer.Address = address;
                await _unitOfWork.Customers.UpdateAsync(_editingCustomer);
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
        catch (InvalidOperationException opEx) // Bắt lỗi trùng lặp.
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = opEx.Message;
        }
        catch (DbUpdateException dbEx)
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}";
            Debug.WriteLine($"LỖI Lưu Khách hàng (DB): {dbEx}");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            errorMsg = $"Lỗi không mong muốn: {ex.Message}";
            Debug.WriteLine($"LỖI Lưu Khách hàng: {ex}");
        }
        finally
        {
            IsDialogSaving = false; // Tắt trạng thái đang lưu
        }

        if (errorMsg != null) { await ShowErrorDialogAsync("Lỗi Lưu Khách Hàng", errorMsg); }
        if (saveSuccess)
        {
            await ShowSuccessDialogAsync("Thành Công", "Khách Hàng đã được lưu.");
            await LoadCustomersAsync(); // Tải lại danh sách sau khi lưu
        }

        return saveSuccess; // Trả về kết quả lưu
    }

    // --- Other Commands ---
    /// <summary>
    /// Cập nhật trạng thái CanExecute cho các command phân trang.
    /// </summary>
    private void UpdateCommandStates()
    {
        (LoadCustomersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AddCustomerCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditCustomerCommand as AsyncRelayCommand<Customer>)?.RaiseCanExecuteChanged();
        (DeleteCustomerCommand as AsyncRelayCommand<Customer>)?.RaiseCanExecuteChanged();
        (ImportCustomersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportCustomersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ApplyFiltersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ClearFiltersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    private bool CanDeleteCustomer(Customer? customer) => customer != null && !IsLoading;
    private async Task DeleteCustomerAsync(Customer? customer)
    {
        if (!CanDeleteCustomer(customer)) return;
        var confirmation = await ShowConfirmationDialogAsync(
            "Xác Nhận Xóa Khách hàng",
            $"Bạn có chắc muốn xóa khách hàng '{customer.CustomerName}' không?",
            "Xóa", "Hủy");
        if (confirmation == ContentDialogResult.Primary)
        {
            IsLoading = true;
            string? errorMsg = null;
            bool deletedSuccess = false;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Kiểm tra xem có trong Order nào không trước khi xóa mềm/cứng
                bool hasOrders = await _unitOfWork.Orders.AnyAsync(o => o.CustomerId == customer.CustomerId);

                var customerToDelete = await _unitOfWork.Customers.GetByIdAsync(customer.CustomerId);
                if (customerToDelete != null)
                {
                    if (hasOrders) // Nếu có đơn hàng -> Xóa mềm
                    {
                        customerToDelete.IsDeleted = true;
                        await _unitOfWork.Customers.UpdateAsync(customerToDelete);
                        Debug.WriteLine($"Xóa mềm khách hàng ID: {customer.CustomerId}");
                    }
                    else // Không có đơn hàng -> Xóa cứng
                    {
                        await _unitOfWork.Customers.DeleteAsync(customerToDelete);
                        Debug.WriteLine($"Xóa cứng khách hàng ID: {customer.CustomerId}");
                    }
                    bool saved = await _unitOfWork.SaveChangesAsync();
                    if (saved) { await _unitOfWork.CommitTransactionAsync(); deletedSuccess = true; }
                    else { await _unitOfWork.RollbackTransactionAsync(); errorMsg = "Không thể lưu thay đổi xóa vào cơ sở dữ liệu."; }
                }
                else { await _unitOfWork.RollbackTransactionAsync(); errorMsg = "Không tìm thấy khách hàng để xóa"; }
            }
            catch (DbUpdateException dbEx) { await _unitOfWork.RollbackTransactionAsync(); errorMsg = $"Lỗi DB: {dbEx.InnerException?.Message ?? dbEx.Message}"; }
            catch (Exception ex) { await _unitOfWork.RollbackTransactionAsync(); errorMsg = $"Lỗi khác: {ex.Message}"; }
            finally
            {
                IsLoading = false;
                if (errorMsg != null)
                {
                    await ShowErrorDialogAsync("Lỗi Xóa Khách hàng", errorMsg);
                }

                // Nếu xóa thành công, tải lại trang hiện tại
                if (deletedSuccess)
                {
                    await ShowSuccessDialogAsync("Xóa Thành Công", $"Khách hàng '{customer.CustomerName}' đã được xóa thành công.");
                    await LoadCustomersAsync();
                }
            }
        }
    }
    private async Task ImportCustomersAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportCustomersAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}