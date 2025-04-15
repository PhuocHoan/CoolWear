using CoolWear.Services;
using CoolWear.Utilities;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

// Enum để định nghĩa các khoảng thời gian báo cáo
public enum ReportPeriod
{
    Daily,
    Monthly,
    Yearly
}

// Model cho dữ liệu biểu đồ doanh thu theo thời gian
public class DateRevenueModel
{
    public DateTime Date { get; set; }
    public double Revenue { get; set; }
}

// Model cho dữ liệu sản phẩm bán chạy
public class TopProductModel
{
    public string ProductName { get; set; } = "";
    public int QuantitySold { get; set; }
}

public partial class ReportViewModel(IUnitOfWork unitOfWork) : ViewModelBase
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private bool _isLoading = false;

    // --- Properties cho lựa chọn thời gian ---
    private ReportPeriod _selectedPeriod = ReportPeriod.Daily; // Mặc định là Ngày
    private string _currentDateSelectionText = ""; // Text dùng cho báo cáo cơ bản, sẽ cập nhật dựa trên lựa chọn
    private string _currentDateRevenueSelectionText = ""; // Text dùng cho báo cáo đồ thị doanh thu, sẽ cập nhật dựa trên lựa chọn
    private string _currentDateTopProductSelectionText = ""; // Text dùng cho báo cáo đồ thị doanh thu, sẽ cập nhật dựa trên lựa chọn
    private DateTimeOffset _selectedDate = DateTimeOffset.Now; // Binding với DatePicker, mặc định là hôm nay

    // --- Properties cho kết quả tính toán ---
    private double _totalRevenue;
    private double _totalProfit;
    private int _completedOrdersCount;
    private int _processingOrdersCount;
    private int _cancelledOrdersCount;
    private int _returnedOrdersCount;

    // --- Properties cho Biểu đồ Doanh thu ---
    private ObservableCollection<ISeries> _revenueChartSeries = [];
    private IEnumerable<ICartesianAxis> _revenueXAxes = [];

    // --- Properties cho Biểu đồ Sản phẩm bán chạy ---
    private ObservableCollection<ISeries> _topProductChartSeries = [];
    // Đổi tên thành XAxes cho rõ ràng
    private IEnumerable<ICartesianAxis> _topProductXAxes = [];

    // === Public Properties ===
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
    public ReportPeriod SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            // Dùng SetProperty để tránh xử lý nếu giá trị không đổi
            if (SetProperty(ref _selectedPeriod, value))
            {
                Debug.WriteLine($"ViewModel: SelectedPeriod changed to {value}. Triggering LoadReportDataAsync.");
                _ = LoadReportDataAsync();
            }
        }
    }
    public List<ReportPeriod> AvailablePeriods { get; } = [ReportPeriod.Daily, ReportPeriod.Monthly, ReportPeriod.Yearly];
    public string CurrentDateSelectionText { get => _currentDateSelectionText; private set => SetProperty(ref _currentDateSelectionText, value); }
    public string CurrentDateRevenueSelectionText { get => _currentDateRevenueSelectionText; private set => SetProperty(ref _currentDateRevenueSelectionText, value); }
    public string CurrentDateTopProductSelectionText { get => _currentDateTopProductSelectionText; private set => SetProperty(ref _currentDateTopProductSelectionText, value); }

    public DateTimeOffset SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                Debug.WriteLine($"ViewModel: SelectedDate changed to {value}. Triggering LoadReportDataAsync.");
                _ = LoadReportDataAsync(); // Gọi Load Data SAU KHI property có sự thay đổi
            }
        }
    }
    public double TotalRevenue { get => _totalRevenue; private set => SetProperty(ref _totalRevenue, value); }
    public double TotalProfit { get => _totalProfit; private set => SetProperty(ref _totalProfit, value); }
    public int CompletedOrdersCount { get => _completedOrdersCount; private set => SetProperty(ref _completedOrdersCount, value); }
    public int ProcessingOrdersCount { get => _processingOrdersCount; private set => SetProperty(ref _processingOrdersCount, value); }
    public int CancelledOrdersCount { get => _cancelledOrdersCount; private set => SetProperty(ref _cancelledOrdersCount, value); }
    public int ReturnedOrdersCount { get => _returnedOrdersCount; private set => SetProperty(ref _returnedOrdersCount, value); }

    // LiveCharts Properties
    public ObservableCollection<ISeries> RevenueChartSeries { get => _revenueChartSeries; set => SetProperty(ref _revenueChartSeries, value); }
    public IEnumerable<ICartesianAxis> RevenueXAxes { get => _revenueXAxes; set => SetProperty(ref _revenueXAxes, value); }
    public ObservableCollection<ISeries> TopProductChartSeries { get => _topProductChartSeries; set => SetProperty(ref _topProductChartSeries, value); }
    public IEnumerable<ICartesianAxis> TopProductXAxes { get => _topProductXAxes; set => SetProperty(ref _topProductXAxes, value); }

    /// <summary>
    /// Phương thức chính để tải tất cả dữ liệu báo cáo dựa trên SelectedPeriod.
    /// </summary>
    public async Task LoadReportDataAsync()
    {
        if (IsLoading)
        {
            Debug.WriteLine($"LoadReportDataAsync skipped. IsLoading: {IsLoading}");
            return;
        }
        IsLoading = true;

        // === XÁC ĐỊNH KHOẢNG THỜI GIAN TRUY VẤN CHO BIỂU ĐỒ DỰA TRÊN LỰA CHỌN ===
        DateTime endDateChart;
        DateTime startDateChart;

        // === XÁC ĐỊNH THỜI GIAN BÁO CÁO THÔNG TIN ===
        DateTime startDateReport;
        DateTime endDateReport;

        // Lấy ngày đã chọn (chỉ phần Date)
        DateTime selectedDateOnly = SelectedDate.Date;

        CurrentDateRevenueSelectionText = "Biểu Đồ Doanh Thu trong giai đoạn";
        CurrentDateTopProductSelectionText = "Top 10 Sản Phẩm Bán Chạy trong giai đoạn";
        string period;

        switch (SelectedPeriod)
        {
            case ReportPeriod.Daily:
            default:
                endDateChart = endDateReport = selectedDateOnly.AddDays(1); // Đầu ngày mai
                startDateChart = selectedDateOnly.AddDays(-9);  // 10 ngày gần nhất (tính cả ngày chọn)
                startDateReport = selectedDateOnly;

                // Update binding text hiển thị trên giao diện
                CurrentDateSelectionText = $"Ngày: {startDateReport:dd/MM/yyyy}";

                period = $" {startDateChart:dd/MM/yyyy} - {selectedDateOnly:dd/MM/yyyy}";
                CurrentDateRevenueSelectionText += period;
                CurrentDateTopProductSelectionText += period;
                break;
            case ReportPeriod.Monthly:
                // Đầu tháng của tháng được chọn + 1 tháng = đầu tháng sau
                endDateChart = endDateReport = new DateTime(selectedDateOnly.Year, selectedDateOnly.Month, 1).AddMonths(1);
                // Lùi lại 11 tháng từ đầu tháng được chọn = đầu của 12 tháng trước
                startDateChart = new DateTime(selectedDateOnly.Year, selectedDateOnly.Month, 1).AddMonths(-11);
                startDateReport = new DateTime(selectedDateOnly.Year, selectedDateOnly.Month, 1);

                // Update binding text hiển thị trên giao diện
                CurrentDateSelectionText = $"Tháng: {startDateReport:MM/yyyy}";

                period = $" {startDateChart:MM/yyyy} - {selectedDateOnly:MM/yyyy}";
                CurrentDateRevenueSelectionText += period;
                CurrentDateTopProductSelectionText += period;
                break;
            case ReportPeriod.Yearly:
                // Đầu năm của năm được chọn + 1 năm = đầu năm sau
                endDateChart = endDateReport = new DateTime(selectedDateOnly.Year, 1, 1).AddYears(1);
                // Lùi lại 4 năm từ đầu năm được chọn = đầu của 5 năm trước
                startDateChart = new DateTime(selectedDateOnly.Year, 1, 1).AddYears(-4);
                startDateReport = new DateTime(selectedDateOnly.Year, 1, 1);

                // Update binding text hiển thị trên giao diện
                CurrentDateSelectionText = $"Năm: {startDateReport:yyyy}";

                period = $" {startDateChart:yyyy} - {selectedDateOnly:yyyy}";
                CurrentDateRevenueSelectionText += period;
                CurrentDateTopProductSelectionText += period;
                break;
        }

        // Chuyển sang UTC để truy vấn DB
        DateTime startDateChartUtc = startDateChart.ToUniversalTime();
        DateTime endDateChartUtc = endDateChart.ToUniversalTime();

        DateTime startDateReportUtc = startDateReport.ToUniversalTime();
        DateTime endDateReportUtc = endDateReport.ToUniversalTime();

        try
        {
            (double revenue, double profit) = await CalculateRevenueAndProfitAsync(startDateReportUtc, endDateReportUtc);
            (int completed, int processing, int cancelled, int returned) = await GetOrderStatusCountsAsync(startDateReportUtc, endDateReportUtc);
            (List<ISeries> revenueSeries, IEnumerable<ICartesianAxis> revenueXAxes) = await LoadRevenueChartDataAsync(startDateChartUtc, endDateChartUtc, SelectedPeriod);
            (List<ISeries> topProductSeries, IEnumerable<ICartesianAxis> topProductXAxesResult) = await LoadTopProductsChartDataAsync(startDateChartUtc, endDateChartUtc);

            _dispatcherQueue.TryEnqueue(() =>
            {
                TotalRevenue = revenue;
                TotalProfit = profit;
                CompletedOrdersCount = completed;
                ProcessingOrdersCount = processing;
                CancelledOrdersCount = cancelled;
                ReturnedOrdersCount = returned;

                // Cập nhật dữ liệu biểu đồ (ObservableCollection nên cập nhật trên UI thread)
                RevenueChartSeries = [.. revenueSeries];
                RevenueXAxes = revenueXAxes; // Gán trục X đã cấu hình
                TopProductChartSeries = [.. topProductSeries];
                TopProductXAxes = topProductXAxesResult; // Gán trục X đã cấu hình
            });

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LỖI Tải Báo cáo: {ex}");
            _dispatcherQueue.TryEnqueue(() =>
            {
                _ = ShowErrorDialogAsync("Lỗi Tải Báo Cáo", $"Không thể tải dữ liệu báo cáo: {ex.Message}");
                // Reset giá trị nếu lỗi
                TotalRevenue = 0; TotalProfit = 0; CompletedOrdersCount = 0; ProcessingOrdersCount = 0; CancelledOrdersCount = 0; ReturnedOrdersCount = 0;
                RevenueChartSeries = []; TopProductChartSeries = [];
                RevenueXAxes = []; TopProductXAxes = [];
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Tính tổng doanh thu và lợi nhuận cho các đơn hàng 'Hoàn thành' trong khoảng thời gian.
    /// </summary>
    private async Task<(double revenue, double profit)> CalculateRevenueAndProfitAsync(DateTime startDateUtc, DateTime endDateUtc)
    {
        double totalRevenue = 0;
        double totalCost = 0;

        var spec = new OrderSpecification().CompletedOrderRevenue(startDateUtc, endDateUtc); // Lọc đơn hàng hoàn thành trong khoảng thời gian, dùng cho việc tính tổng doanh thu.
        var completedOrders = await unitOfWork.Orders.GetAsync(spec);

        if (completedOrders != null)
        {
            foreach (var order in completedOrders)
            {
                totalRevenue += order.NetTotal; // Tính doanh thu từ NetTotal

                // Tính tổng giá nhập của các sản phẩm trong đơn hàng này
                foreach (var item in order.OrderItems)
                {
                    if (item.Variant?.Product != null)
                    {
                        totalCost += (double)item.Quantity * item.Variant.Product.ImportPrice;
                    }
                }
            }
        }

        var totalProfit = totalRevenue - totalCost; // Lợi nhuận là doanh thu - tổng giá nhập

        return (totalRevenue, totalProfit);
    }

    /// <summary>
    /// Đếm số lượng đơn hàng theo từng trạng thái trong khoảng thời gian.
    /// </summary>
    private async Task<(int completed, int processing, int cancelled, int returned)> GetOrderStatusCountsAsync(DateTime startDateUtc, DateTime endDateUtc)
    {
        var spec = new OrderSpecification().OrderStatusCount(startDateUtc, endDateUtc); // Lọc đơn hàng trong khoảng thời gian
        var ordersInPeriod = await unitOfWork.Orders.GetAsync(spec);

        int completed = 0; int processing = 0; int cancelled = 0; int returned = 0;
        if (ordersInPeriod != null)
        {
            // Thực hiện Count trên kết quả trả về (trong bộ nhớ)
            completed = ordersInPeriod.Count(o => o.Status == "Hoàn thành");
            processing = ordersInPeriod.Count(o => o.Status == "Đang xử lý");
            cancelled = ordersInPeriod.Count(o => o.Status == "Đã hủy");
            returned = ordersInPeriod.Count(o => o.Status == "Đã hoàn trả");
        }
        return (completed, processing, cancelled, returned);
    }


    /// <summary>
    /// Chuẩn bị dữ liệu và cấu hình cho biểu đồ doanh thu theo thời gian.
    /// </summary>
    private async Task<(List<ISeries> series, Axis[] xAxes)> LoadRevenueChartDataAsync(DateTime startDateUtc, DateTime endDateUtc, ReportPeriod period)
    {
        List<DateRevenueModel> revenueData = [];
        Axis[] axes = [];
        string axisName;
        string axisLabelFormat = "dd/MM/yy";

        try
        {
            switch (period)
            {
                case ReportPeriod.Monthly:
                    axisName = "Tháng"; axisLabelFormat = "MM/yy";
                    break;
                case ReportPeriod.Yearly:
                    axisName = "Năm"; axisLabelFormat = "yyyy";
                    break;
                case ReportPeriod.Daily:
                default:
                    axisName = "Ngày"; axisLabelFormat = "dd/MM/yy";
                    break;
            }

            var spec = new OrderSpecification().CompletedOrderRevenue(startDateUtc, endDateUtc);
            var completedOrders = await unitOfWork.Orders.GetAsync(spec);

            if (completedOrders != null && completedOrders.Any())
            {
                revenueData = period switch
                {
                    ReportPeriod.Monthly => [.. completedOrders
                            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                            .Select(g => new DateRevenueModel { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Revenue = g.Sum(o => o.NetTotal) })
                            .OrderBy(d => d.Date)],
                    ReportPeriod.Yearly => [.. completedOrders
                            .GroupBy(o => o.OrderDate.Year)
                            .Select(g => new DateRevenueModel { Date = new DateTime(g.Key, 1, 1), Revenue = g.Sum(o => o.NetTotal) })
                            .OrderBy(d => d.Date)],
                    _ => [.. completedOrders
                           .GroupBy(o => o.OrderDate.Date) // Nhóm theo ngày
                           .Select(g => new DateRevenueModel { Date = g.Key, Revenue = g.Sum(o => o.NetTotal) })
                           .OrderBy(d => d.Date)],
                };
            }

            // Cấu hình trục X cho biểu đồ dạng cột
            if (revenueData.Any())
            {
                // Tạo mảng nhãn từ dữ liệu
                string[] dateLabels = [.. revenueData.Select(d => d.Date.ToString(axisLabelFormat))];

                axes =
                [
                    new Axis
                    {
                        Name = axisName,
                        Labels = dateLabels,
                        UnitWidth = 1,        // Đơn vị chiều rộng
                        MinStep = 1,          // Bước tối thiểu
                        SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)), // Màu sắc của các đường phân cách
                        SeparatorsAtCenter = false, // Không phân cách ở giữa
                    }
                ];
            }
            else
            {
                // Nếu không có dữ liệu, vẫn tạo một trục mặc định
                axes =
                [
                    new Axis
                    {
                        Name = axisName,
                        Labels = ["", "Không có dữ liệu", ""], // Thêm khoảng trống trước và sau để đẩy nhãn vào giữa
                        MinLimit = 0,
                        MaxLimit = 2, // Giới hạn số điểm trên trục để đảm bảo nhãn ở giữa
                        // Ẩn các đường kẻ lưới và phân cách
                        SeparatorsPaint = null,
                        ShowSeparatorLines = false,
                        TextSize = 20,
                        LabelsPaint = new SolidColorPaint(SKColors.Red),
                    }
                ];
            }
        }
        catch (Exception ex) { Debug.WriteLine($"Lỗi tải dữ liệu biểu đồ doanh thu: {ex}"); }

        // Tạo Series cho LiveCharts - dạng cột
        var series = new List<ISeries>
            {
                new ColumnSeries<DateRevenueModel>  // Thay thế LineSeries bằng ColumnSeries
                {
                    Name = "Doanh thu",
                    Values = revenueData,
                    // Trong ColumnSeries, tham số đầu tiên là vị trí (index) của cột, thứ hai là giá trị
                    Mapping = (model, index) => new(index, model.Revenue),  // Map theo index để phù hợp với labels
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Stroke = null,
                    MaxBarWidth = double.MaxValue,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                }
            };

        return (series, axes);
    }


    /// <summary>
    /// Chuẩn bị dữ liệu và cấu hình cho biểu đồ Top 10 sản phẩm bán chạy DẠNG CỘT DỌCG (Column Chart).
    /// </summary>
    private async Task<(List<ISeries> series, Axis[] xAxes)> LoadTopProductsChartDataAsync(DateTime startDateUtc, DateTime endDateUtc)
    {
        List<TopProductModel> topProductsData = [];
        Axis[] xAxes = []; // trục X chứa nhãn sản phẩm
        string[] labels = [];

        try
        {
            var spec = new OrderItemSpecification()
                .TopSellingItems(startDateUtc, endDateUtc); // Lọc OrderItem của các đơn hàng Hoàn thành trong khoảng thời gian
            var itemsSold = await unitOfWork.OrderItems.GetAsync(spec);

            if (itemsSold != null && itemsSold.Any())
            {
                topProductsData = [.. itemsSold
                    .Where(oi => oi.Variant?.Product != null)
                    .GroupBy(oi => oi.Variant!.ProductId)
                    .Select(g => new TopProductModel
                    {
                        ProductName = g.First().Variant!.Product!.ProductName,
                        QuantitySold = g.Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(10)];

                labels = [.. topProductsData.Select(p => p.ProductName)];
            }

            // Cấu hình trục X cho biểu đồ dạng cột
            if (topProductsData.Any())
            {
                xAxes =
                [
                    new Axis
                    {
                        Name = "Sản phẩm",
                        Labels = labels,
                        UnitWidth = 1,        // Đơn vị chiều rộng
                        MinStep = 1,          // Bước tối thiểu
                        SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)), // Màu sắc của các đường phân cách
                        SeparatorsAtCenter = false, // Không phân cách ở giữa
                    }
                ];
            }
            else
            {
                // Nếu không có dữ liệu, vẫn tạo một trục mặc định
                xAxes =
                [
                    new Axis
                    {
                        Name = "Sản phẩm",
                        Labels = ["", "Không có dữ liệu", ""], // Thêm khoảng trống trước và sau để đẩy nhãn vào giữa
                        MinLimit = 0,
                        MaxLimit = 2, // Giới hạn số điểm trên trục để đảm bảo nhãn ở giữa
                        // Ẩn các đường kẻ lưới và phân cách
                        SeparatorsPaint = null,
                        ShowSeparatorLines = false,
                        TextSize = 20,
                        LabelsPaint = new SolidColorPaint(SKColors.Red),
                    }
                ];
            }
        }
        catch (Exception ex) { Debug.WriteLine($"Lỗi tải dữ liệu top sản phẩm: {ex}"); xAxes = []; } // Gán mảng rỗng nếu lỗi

        var series = new List<ISeries>
        {
            new ColumnSeries<TopProductModel>
            {
                Name = "Số lượng bán",
                Values = topProductsData,
                Mapping = (model, index) => new(index, model.QuantitySold),
                Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                Stroke = null,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsSize = 14,
                MaxBarWidth = int.MaxValue,
            }
        };

        return (series, xAxes);
    }
}