// OrderViewModel.cs
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

public partial class OrderViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _isResettingFilters = false;
    private const int DefaultPageSize = 1; // Số đơn hàng trên mỗi trang

    // --- Backing fields ---
    private ObservableCollection<Order>? _filteredOrders;
    private string? _searchTerm;
    private bool _isLoading;
    private bool _showEmptyMessage;
    private string? _selectedStatus;
    private DateTimeOffset? _startDate;
    private DateTimeOffset? _endDate;
    private PaymentMethod? _selectedPaymentMethod;
    private int? _minNetTotal;
    private int? _maxNetTotal;

    // --- Pagination Fields ---
    private int _currentPage = 1;
    private int _pageSize = DefaultPageSize;
    private int _totalItems;
    private int _totalPages = 1;

    // --- Dialog Data Properties ---
    private string _dialogTitle = "Cập Nhật Trạng Thái Đơn Hàng";
    private bool _isDialogSaving = false;
    private string? _selectedDialogStatus; // Trạng thái được chọn trong dialog
    private ObservableCollection<string> _availableDialogStatuses = []; // Danh sách trạng thái có thể chọn trong dialog

    // --- Public properties ---
    public ObservableCollection<Order>? FilteredOrders { get => _filteredOrders; private set => SetProperty(ref _filteredOrders, value); }
    public string? SearchTerm { get => _searchTerm; set => SetProperty(ref _searchTerm, value); }
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value, onChanged: UpdateCommandStates); }
    public bool ShowEmptyMessage { get => _showEmptyMessage; private set => SetProperty(ref _showEmptyMessage, value, onChanged: UpdateCommandStates); }

    // --- Filter Properties ---
    public List<string> AvailableStatuses { get; } = ["Tất cả trạng thái", "Đang xử lý", "Hoàn thành", "Đã hoàn trả", "Đã hủy"];
    public string? SelectedStatus { get => _selectedStatus; set => SetProperty(ref _selectedStatus, value); }
    public DateTimeOffset? StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }
    public DateTimeOffset? EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }
    public ObservableCollection<PaymentMethod> AvailablePaymentMethods { get; } = [];
    public PaymentMethod? SelectedPaymentMethod { get => _selectedPaymentMethod; set => SetProperty(ref _selectedPaymentMethod, value); }
    public int? MinNetTotal { get => _minNetTotal; set => SetProperty(ref _minNetTotal, value); }
    public int? MaxNetTotal { get => _maxNetTotal; set => SetProperty(ref _maxNetTotal, value); }

    // --- Pagination Properties ---
    public int CurrentPage { get => _currentPage; private set => SetProperty(ref _currentPage, value, onChanged: UpdateCommandStates); }
    public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }
    public int TotalItems { get => _totalItems; private set => SetProperty(ref _totalItems, value); } // Chỉ cần cập nhật khi tải dữ liệu
    public int TotalPages { get => _totalPages; private set => SetProperty(ref _totalPages, value, onChanged: UpdateCommandStates); } // Gọi UpdateCommandStates khi TotalPages thay đổi (ảnh hưởng nút Next/Prev)

    // --- Dialog Properties ---
    public string DialogTitle { get => _dialogTitle; private set => SetProperty(ref _dialogTitle, value); }
    public bool IsDialogSaving { get => _isDialogSaving; private set => SetProperty(ref _isDialogSaving, value); }
    public Order? EditingOrder { get; private set; } = null; // Chỉ đọc, để hiển thị thông tin đơn hàng gốc
    public string? SelectedDialogStatus { get => _selectedDialogStatus; set => SetProperty(ref _selectedDialogStatus, value); }
    public ObservableCollection<string> AvailableDialogStatuses { get => _availableDialogStatuses; private set => SetProperty(ref _availableDialogStatuses, value); }

    // --- Commands ---
    public ICommand LoadOrdersCommand { get; }
    public ICommand EditOrderCommand { get; } // Chỉ có sửa
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand ImportOrdersCommand { get; }
    public ICommand ExportOrdersCommand { get; }

    // --- Event ---
    public event Func<Task<ContentDialogResult>>? RequestShowDialog;

    // --- Constructor ---
    public OrderViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        FilteredOrders = [];

        LoadOrdersCommand = new AsyncRelayCommand(ResetFiltersAndLoadAsync, CanLoadData);
        EditOrderCommand = new AsyncRelayCommand<Order>(PrepareEditDialogAsync, CanShowEditDialog);
        ApplyFiltersCommand = new AsyncRelayCommand(ApplyFiltersAndLoadAsync, () => !IsLoading);
        ClearFiltersCommand = new AsyncRelayCommand(ResetFiltersAndLoadAsync, () => !IsLoading);
        PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, CanGoToPreviousPage);
        NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, CanGoToNextPage);
        ImportOrdersCommand = new AsyncRelayCommand(ImportOrdersAsync, () => !IsLoading);
        ExportOrdersCommand = new AsyncRelayCommand(ExportOrdersAsync, () => !IsLoading);

        PropertyChanged += ViewModel_PropertyChanged;
    }

    // --- Event Handler ---
    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsLoading || _isResettingFilters) return;
        switch (e.PropertyName)
        {
            case nameof(SearchTerm): // Tìm kiếm ID thì load ngay
            case nameof(PageSize):
                await ApplyFiltersAndLoadAsync();
                break;
        }
    }

    // --- Data Loading ---
    private bool CanLoadData() => !IsLoading;
    public async Task InitializeDataAsync() // Gọi từ OnNavigatedTo
    {
        if (!CanLoadData()) return;
        await LoadFilterOptionsAsync();
        await ResetFiltersAndLoadAsync();
    }

    public async Task ResetFiltersAndLoadAsync()
    {
        if (IsLoading) return;
        _isResettingFilters = true;
        SearchTerm = null;
        SelectedStatus = AvailableStatuses.FirstOrDefault(); // Reset về "Tất cả"
        StartDate = null;
        EndDate = null;
        SelectedPaymentMethod = null;
        MinNetTotal = null;
        MaxNetTotal = null;
        CurrentPage = 1;
        _isResettingFilters = false;
        await LoadOrdersAsync(); // Load không filter
    }

    private async Task ApplyFiltersAndLoadAsync()
    {
        if (IsLoading) return;
        CurrentPage = 1;
        await LoadOrdersAsync();
    }

    public async Task LoadOrdersAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        DateTime? startDateUtc = StartDate?.Date.ToUniversalTime(); // Lấy đầu ngày giờ UTC
        DateTime? endDateUtc = EndDate?.Date.ToUniversalTime(); // Lấy đầu ngày giờ UTC

        try
        {
            var countSpec = new OrderSpecification(SearchTerm, SelectedStatus, startDateUtc,
                endDateUtc, SelectedPaymentMethod?.PaymentMethodId,
                MinNetTotal, MaxNetTotal, includeDetails: false);
            TotalItems = await _unitOfWork.Orders.CountAsync(countSpec);

            TotalPages = TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;

            // IncludeDetails = true để lấy thông tin cần thiết
            var dataSpec = new OrderSpecification(SearchTerm, SelectedStatus, startDateUtc,
                endDateUtc, SelectedPaymentMethod?.PaymentMethodId,
                MinNetTotal, MaxNetTotal, skip, PageSize, includeDetails: true);
            var orders = await _unitOfWork.Orders.GetAsync(dataSpec);
            // Cập nhật UI trên luồng chính
            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredOrders?.Clear();
                if (orders != null) { foreach (var o in orders) FilteredOrders?.Add(o); }
                ShowEmptyMessage = !(FilteredOrders?.Any() ?? false);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR Loading Products: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", $"Không thể tải danh sách đơn hàng: {ex.Message}");
            ShowEmptyMessage = true;
            FilteredOrders?.Clear();
            TotalItems = 0;
            TotalPages = 1;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadFilterOptionsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        List<PaymentMethod>? paymentMethodsData = null;
        string? errorMsg = null;

        try
        {
            paymentMethodsData = [.. (await _unitOfWork.PaymentMethods.GetAllAsync()).OrderBy(p => p.PaymentMethodName)];
        }
        catch (Exception ex) { Debug.WriteLine($"Lỗi tải PT Thanh toán: {ex}"); errorMsg = ex.Message; }
        finally { IsLoading = false; }

        // Cập nhật UI
        _dispatcherQueue.TryEnqueue(() =>
        {
            AvailablePaymentMethods.Clear();
            if (paymentMethodsData != null)
            {
                foreach (var pm in paymentMethodsData) AvailablePaymentMethods.Add(pm);
            }
            if (!string.IsNullOrEmpty(errorMsg)) { _ = ShowErrorDialogAsync("Lỗi Tải Bộ Lọc", "Không thể tải danh sách phương thức thanh toán."); }
        });
    }

    // --- Pagination ---
    private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;
    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoToPreviousPage()) return; CurrentPage--;
        await LoadOrdersAsync();
    }
    private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
    private async Task GoToNextPageAsync()
    {
        if (!CanGoToNextPage()) return; CurrentPage++;
        await LoadOrdersAsync();
    }
    private void UpdateCommandStates()
    {
        (LoadOrdersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditOrderCommand as AsyncRelayCommand<Order>)?.RaiseCanExecuteChanged();
        (ImportOrdersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportOrdersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ApplyFiltersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ClearFiltersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    // --- Dialog Logic ---
    private bool CanShowEditDialog(Order? order) => order != null && !IsLoading;
    private async Task PrepareEditDialogAsync(Order? orderToEdit)
    {
        if (!CanShowEditDialog(orderToEdit)) return;

        EditingOrder = orderToEdit; // Lưu lại đơn hàng gốc
        DialogTitle = $"Cập nhật Đơn hàng #{orderToEdit.OrderId}";
        SelectedDialogStatus = orderToEdit.Status; // Trạng thái hiện tại
        IsDialogSaving = false;

        // Xác định các trạng thái có thể chuyển đổi
        _availableDialogStatuses.Clear();
        _availableDialogStatuses.Add(orderToEdit.Status); // Luôn có thể giữ nguyên trạng thái
        switch (orderToEdit.Status)
        {
            case "Đang xử lý":
                _availableDialogStatuses.Add("Hoàn thành");
                _availableDialogStatuses.Add("Đã hủy");
                break;
            case "Hoàn thành":
                _availableDialogStatuses.Add("Đã hoàn trả");
                break;
        }

        if (RequestShowDialog != null) { await RequestShowDialog.Invoke(); }
    }

    /// <summary>
    /// Lưu thay đổi trạng thái đơn hàng và xử lý logic nghiệp vụ liên quan.
    /// </summary>
    public async Task<bool> SaveOrderStatusAsync()
    {
        if (EditingOrder == null || string.IsNullOrEmpty(SelectedDialogStatus) || IsDialogSaving)
        {
            return false;
        }

        string originalStatus = EditingOrder.Status;
        string newStatus = SelectedDialogStatus;

        // Nếu trạng thái không đổi thì không làm gì cả
        if (originalStatus == newStatus) return true;

        IsDialogSaving = true;
        string? errorMsg = null;
        bool saveSuccess = false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // --- Xử lý Logic ---
            Customer? customer = null;
            if (EditingOrder.CustomerId.HasValue)
            {
                customer = await _unitOfWork.Customers.GetByIdAsync(EditingOrder.CustomerId.Value);
            }
            int pointsChange = 0;

            // A. Chuyển sang Hoàn thành: Tính điểm nếu cần
            if (newStatus == "Hoàn thành" && originalStatus == "Đang xử lý")
            {
                pointsChange = CalculatePoints(EditingOrder.NetTotal);
                Debug.WriteLine($"Đơn hàng #{EditingOrder.OrderId} hoàn thành. Điểm thay đổi: {pointsChange}");
            }
            // B. Chuyển sang Đã hủy hoặc Đã hoàn trả: Hoàn kho, Hoàn điểm
            else if ((newStatus == "Đã hủy" && originalStatus == "Đang xử lý") ||
                     (newStatus == "Đã hoàn trả" && originalStatus == "Hoàn thành"))
            {
                // 1. Hoàn kho
                foreach (var item in EditingOrder.OrderItems)
                {
                    var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.VariantId);
                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                        await _unitOfWork.ProductVariants.UpdateAsync(variant);
                        Debug.WriteLine($"Hoàn kho cho Variant ID {variant.VariantId}: +{item.Quantity}");
                    }
                }

                // 2. Hoàn điểm đã sử dụng (nếu có)
                if (EditingOrder.PointUsed > 0 && customer != null)
                {
                    customer.Points += EditingOrder.PointUsed; // Cộng lại điểm đã dùng
                    Debug.WriteLine($"Hoàn lại {EditingOrder.PointUsed} điểm đã sử dụng cho KH {customer.CustomerId}");
                }

                // 3. Trừ điểm đã tích lũy khi hoàn thành (chỉ áp dụng khi chuyển từ Hoàn thành -> Đã hoàn trả)
                if (newStatus == "Đã hoàn trả" && originalStatus == "Hoàn thành" && customer != null)
                {
                    int pointsEarned = CalculatePoints(EditingOrder.NetTotal);
                    pointsChange = -pointsEarned; // Trừ đi điểm đã cộng
                    Debug.WriteLine($"Đơn hàng #{EditingOrder.OrderId} hoàn trả. Trừ điểm đã tích lũy: {pointsChange}");
                }
                else if (newStatus == "Đã hủy" && originalStatus == "Đang xử lý")
                {
                    // Hủy đơn thì không có điểm nào được cộng hoặc trừ (ngoài việc hoàn điểm đã dùng)
                    pointsChange = 0;
                    Debug.WriteLine($"Đơn hàng #{EditingOrder.OrderId} bị hủy. Không thay đổi điểm tích lũy.");
                }
            }

            // Cập nhật điểm cho khách hàng nếu có thay đổi và có khách hàng
            if (pointsChange != 0 && customer != null)
            {
                customer.Points += pointsChange;
                if (customer.Points < 0) customer.Points = 0; // Đảm bảo điểm không âm
                await _unitOfWork.Customers.UpdateAsync(customer);
                Debug.WriteLine($"Cập nhật điểm cho KH {customer.CustomerId}. Điểm mới: {customer.Points}");
            }

            // Cập nhật trạng thái đơn hàng
            EditingOrder.Status = newStatus;
            await _unitOfWork.Orders.UpdateAsync(EditingOrder);

            // Lưu tất cả thay đổi
            bool saved = await _unitOfWork.SaveChangesAsync();
            if (saved) { await _unitOfWork.CommitTransactionAsync(); saveSuccess = true; }
            else { await _unitOfWork.RollbackTransactionAsync(); errorMsg = "Không thể lưu thay đổi trạng thái."; }
        }
        catch (InvalidOperationException opEx) { await _unitOfWork.RollbackTransactionAsync(); errorMsg = opEx.Message; }
        catch (DbUpdateException dbEx) { await _unitOfWork.RollbackTransactionAsync(); errorMsg = $"Lỗi DB: {dbEx.InnerException?.Message ?? dbEx.Message}"; }
        catch (Exception ex) { await _unitOfWork.RollbackTransactionAsync(); errorMsg = $"Lỗi khác: {ex.Message}"; }
        finally
        {
            IsDialogSaving = false;
        }

        if (errorMsg != null) { await ShowErrorDialogAsync("Lỗi Cập Nhật Trạng Thái", errorMsg); }
        if (saveSuccess)
        {
            await ShowSuccessDialogAsync("Thành Công", $"Đã cập nhật trạng thái đơn hàng #{EditingOrder?.OrderId} thành '{newStatus}'.");
            // Load lại với filter hiện tại để thấy thay đổi
            await LoadOrdersAsync();
        }
        return saveSuccess;
    }

    /// <summary>
    /// Tính điểm dựa trên tổng tiền hóa đơn.
    /// </summary>
    private static int CalculatePoints(int netTotal) => (int)Math.Floor((double)netTotal / 100000); // 100.000đ = 1 điểm, 200.000đ = 2 điểm, ...

    private async Task ImportOrdersAsync() => await ShowNotImplementedDialogAsync("Nhập File Excel/CSV");
    private async Task ExportOrdersAsync() => await ShowNotImplementedDialogAsync("Xuất File Excel/CSV");
}