using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CoolWear.ViewModels;

//============================================================
// ViewModel cho một dòng nhập/hiển thị biến thể trong danh sách
//============================================================
public partial class VariantEntryViewModel : ViewModelBase
{
    /// <summary>
    /// ID của biến thể trong database (bằng 0 nếu là biến thể mới chưa được lưu).
    /// </summary>
    public int VariantId { get; set; }

    private ProductColor? _color;
    private ProductSize? _size;
    private int _stockQuantity;

    /// <summary>
    /// Màu sắc của biến thể.
    /// </summary>
    public ProductColor? Color { get => _color; set => SetProperty(ref _color, value); }
    /// <summary>
    /// Kích thước của biến thể.
    /// </summary>
    public ProductSize? Size { get => _size; set => SetProperty(ref _size, value); }
    /// <summary>
    /// Số lượng tồn kho của biến thể.
    /// </summary>
    public int StockQuantity { get => _stockQuantity; set => SetProperty(ref _stockQuantity, value); }

    /// <summary>
    /// Tên màu sắc để hiển thị (hoặc "N/A").
    /// </summary>
    public string ColorName => Color?.ColorName ?? "N/A";
    /// <summary>
    /// Tên kích thước để hiển thị (hoặc "N/A").
    /// </summary>
    public string SizeName => Size?.SizeName ?? "N/A";

    /// <summary>
    /// Tham chiếu đến ViewModel cha (AddEditProductViewModel), được gán khi tạo instance này.
    /// </summary>
    public AddEditProductViewModel? ParentViewModel { get; set; }

    /// <summary>
    /// Command được kích hoạt bởi nút xóa trên dòng của biến thể này.
    /// </summary>
    public ICommand RequestRemoveCommand { get; }

    public VariantEntryViewModel() =>
        // Khởi tạo command, nó sẽ gọi ExecuteRequestRemove khi được nhấn
        RequestRemoveCommand = new RelayCommand(ExecuteRequestRemove);

    /// <summary>
    /// Thực thi yêu cầu xóa bằng cách gọi phương thức xử lý trên ViewModel cha.
    /// </summary>
    private void ExecuteRequestRemove() => ParentViewModel?.ProcessRemoveRequest(this);

    /// <summary>
    /// Ghi đè phương thức Equals để kiểm tra xem hai VariantEntryViewModel có cùng Màu và Size không.
    /// Được sử dụng để phát hiện trùng lặp trong danh sách VariantsToAdd.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not VariantEntryViewModel other) return false;
        // Coi là bằng nhau nếu ColorId và SizeId giống nhau
        return Equals(this.Color?.ColorId, other.Color?.ColorId) &&
               Equals(this.Size?.SizeId, other.Size?.SizeId);
    }

    /// <summary>
    /// Bắt buộc phải ghi đè khi ghi đè Equals. Dùng ColorId và SizeId để tạo mã hash.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Color?.ColorId, Size?.SizeId);
}


//============================================================
// ViewModel chính cho trang Thêm/Sửa Sản phẩm
//============================================================
public partial class AddEditProductViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;

    // --- Thuộc tính của Sản phẩm đang được chỉnh sửa ---
    /// <summary>
    /// ID của sản phẩm đang được chỉnh sửa. Bằng 0 nếu đang ở chế độ Thêm mới.
    /// </summary>
    private int _productIdToEdit = 0;
    /// <summary>
    /// Tiêu đề của trang ("Thêm Sản Phẩm Mới" hoặc "Chỉnh Sửa Sản Phẩm").
    /// </summary>
    private string _pageTitle = "Thêm Sản Phẩm Mới";
    /// <summary>
    /// Tên sản phẩm.
    /// </summary>
    private string _productName = "";
    /// <summary>
    /// Giá nhập sản phẩm.
    /// </summary>
    private int _importPrice;
    /// <summary>
    /// Giá bán sản phẩm.
    /// </summary>
    private int _price;
    /// <summary>
    /// Danh mục sản phẩm được chọn.
    /// </summary>
    private ProductCategory? _selectedCategory;
    /// <summary>
    /// Đường dẫn file cục bộ của ảnh vừa được người dùng chọn (nếu có).
    /// </summary>
    private string? _selectedImagePath;
    /// <summary>
    /// Đường dẫn (URL hoặc local path) được sử dụng bởi control Image để hiển thị ảnh preview.
    /// </summary>
    private string? _imageUrlPreview;
    /// <summary>
    /// Xác định xem có đang ở chế độ Edit không.
    /// </summary>
    private bool IsEditMode => _productIdToEdit > 0;

    public string PageTitle { get => _pageTitle; private set => SetProperty(ref _pageTitle, value); }
    public string ProductName { get => _productName; set { SetProperty(ref _productName, value); UpdateAllCommandStates(); } }
    public int ImportPrice { get => _importPrice; set { SetProperty(ref _importPrice, value); UpdateAllCommandStates(); } }
    public int Price { get => _price; set { SetProperty(ref _price, value); UpdateAllCommandStates(); } }
    public ProductCategory? SelectedCategory { get => _selectedCategory; set { SetProperty(ref _selectedCategory, value); UpdateAllCommandStates(); } }
    public string? SelectedImagePath { get => _selectedImagePath; private set => SetProperty(ref _selectedImagePath, value); }
    public string ImageUrlPreview
    {
        // Trả về ảnh placeholder nếu _imageUrlPreview là null/trống, ngược lại trả về giá trị hiện tại.
        get => !string.IsNullOrEmpty(_imageUrlPreview) ? _imageUrlPreview : "ms-appx:///Assets/StoreLogo.png";
        private set => SetProperty(ref _imageUrlPreview, value);
    }

    // --- Collections và Dữ liệu cho việc quản lý Biến thể ---
    /// <summary>
    /// Danh sách các Danh mục để hiển thị trong ComboBox.
    /// </summary>
    public FullObservableCollection<ProductCategory> AvailableCategories { get; } = [];
    /// <summary>
    /// Danh sách các Màu sắc để hiển thị trong ComboBox.
    /// </summary>
    public FullObservableCollection<ProductColor> AvailableColors { get; } = [];
    /// <summary>
    /// Danh sách các Kích thước để hiển thị trong ComboBox.
    /// </summary>
    public FullObservableCollection<ProductSize> AvailableSizes { get; } = [];

    /// <summary>
    /// Danh sách các biến thể đang được hiển thị trên giao diện người dùng (bao gồm cả biến thể mới thêm và biến thể đã có khi Edit).
    /// </summary>
    public FullObservableCollection<VariantEntryViewModel> VariantsToAdd { get; } = [];

    /// <summary>
    /// Danh sách lưu trữ các biến thể ĐÃ TỒN TẠI trong DB mà người dùng đã nhấn nút xóa trên UI.
    /// Chúng sẽ được xử lý (xóa mềm/cứng) khi người dùng nhấn nút Lưu.
    /// </summary>
    private readonly List<VariantEntryViewModel> _variantsPendingDeletion = [];

    // Thuộc tính liên kết với các control nhập liệu cho biến thể mới
    private ProductColor? _currentVariantColor;
    private ProductSize? _currentVariantSize;
    private int _currentVariantStock;

    public ProductColor? CurrentVariantColor { get => _currentVariantColor; set { SetProperty(ref _currentVariantColor, value); UpdateAllCommandStates(); } }
    public ProductSize? CurrentVariantSize { get => _currentVariantSize; set { SetProperty(ref _currentVariantSize, value); UpdateAllCommandStates(); } }
    public int CurrentVariantStock { get => _currentVariantStock; set { SetProperty(ref _currentVariantStock, value); UpdateAllCommandStates(); } }

    // --- Commands ---
    /// <summary>
    /// Command để tải dữ liệu cho các ComboBox (lookup data).
    /// </summary>
    public ICommand LoadLookupsCommand { get; }
    /// <summary>
    /// Command để thêm biến thể mới từ các ô nhập liệu vào danh sách VariantsToAdd.
    /// </summary>
    public ICommand AddVariantCommand { get; }
    /// <summary>
    /// Command để lưu sản phẩm (Thêm mới hoặc Cập nhật).
    /// </summary>
    public ICommand SaveProductCommand { get; }
    /// <summary>
    /// Command để hủy bỏ thao tác và quay lại trang trước.
    /// </summary>
    public ICommand CancelCommand { get; }
    /// <summary>
    /// Command để mở hộp thoại chọn file ảnh.
    /// </summary>
    public ICommand SelectImageCommand { get; }

    // --- Trạng thái ---
    /// <summary>
    /// Cờ báo hiệu đang thực hiện thao tác lưu, dùng để hiển thị loading và vô hiệu hóa control.
    /// </summary>
    private bool _isSaving;
    public bool IsSaving { get => _isSaving; private set { SetProperty(ref _isSaving, value); UpdateAllCommandStates(); } }

    // --- Events để điều hướng ---
    /// <summary>
    /// Event được phát ra khi lưu sản phẩm thành công. View sẽ bắt event này để điều hướng về.
    /// </summary>
    public event EventHandler? SaveCompleted;
    /// <summary>
    /// Event được phát ra khi nhấn nút Hủy. View sẽ bắt event này để điều hướng về.
    /// </summary>
    public event EventHandler? Cancelled;

    // --- Constructor ---
    public AddEditProductViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        // Khởi tạo các Command
        LoadLookupsCommand = new AsyncRelayCommand(LoadLookupsAsync);
        AddVariantCommand = new AsyncRelayCommand(AddVariant, CanAddVariant); // CanAddVariant là hàm kiểm tra điều kiện thực thi
        SaveProductCommand = new AsyncRelayCommand(SaveProductInternalAsync, CanSaveProduct); // CanSaveProduct là hàm kiểm tra điều kiện thực thi
        CancelCommand = new RelayCommand(CancelOperation);
        SelectImageCommand = new AsyncRelayCommand(SelectImageAsync);

        // Đăng ký để cập nhật trạng thái CanExecute của Command khi các thuộc tính liên quan thay đổi
        PropertyChanged += (s, e) => UpdateAllCommandStates();
        // Cũng cập nhật khi danh sách biến thể thay đổi (ảnh hưởng CanSaveProduct)
        VariantsToAdd.CollectionChanged += (s, e) => UpdateAllCommandStates();
    }

    // --- Logic kiểm tra điều kiện thực thi Command (CanExecute) ---
    /// <summary>
    /// Kiểm tra xem có thể thêm biến thể mới không (phải chọn màu, size và tồn kho không âm).
    /// </summary>
    private bool CanAddVariant() =>
        CurrentVariantColor != null &&
        CurrentVariantSize != null &&
        CurrentVariantStock >= 0 &&
        !IsSaving; // Không cho thêm khi đang lưu

    /// <summary>
    /// Kiểm tra xem có thể lưu sản phẩm không.
    /// </summary>
    private bool CanSaveProduct() =>
        !string.IsNullOrWhiteSpace(ProductName) && // Tên SP là bắt buộc
        SelectedCategory != null &&               // Danh mục là bắt buộc
        Price > 0 &&                              // Giá bán phải > 0
        ImportPrice >= 0 &&                       // Giá nhập >= 0
        ImportPrice < Price &&                    // Giá nhập < Giá bán
        VariantsToAdd.Any() &&                    // Phải có ít nhất 1 biến thể đang hiển thị
        !IsSaving;                                // Không cho lưu khi đang lưu

    // --- Các phương thức ---

    /// <summary>
    /// Cập nhật trạng thái Enable/Disable của các nút dựa trên hàm CanExecute tương ứng.
    /// </summary>
    private void UpdateAllCommandStates()
    {
        (AddVariantCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (SaveProductCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SelectImageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Reset trạng thái ViewModel về ban đầu để chuẩn bị cho việc thêm sản phẩm mới.
    /// </summary>
    public void SetAddMode()
    {
        _productIdToEdit = 0; // Đặt ID về 0
        PageTitle = "Thêm Sản Phẩm Mới";
        ProductName = "";
        ImportPrice = 0;
        Price = 0;
        SelectedCategory = null;
        SelectedImagePath = null;
        ImageUrlPreview = "ms-appx:///Assets/StoreLogo.png"; // Ảnh mặc định
        VariantsToAdd.Clear();          // Xóa danh sách biến thể hiển thị
        _variantsPendingDeletion.Clear(); // Xóa danh sách biến thể chờ xóa
        CurrentVariantColor = null;
        CurrentVariantSize = null;
        CurrentVariantStock = 0;
        IsSaving = false;             // Đảm bảo tắt trạng thái loading
        UpdateAllCommandStates();     // Cập nhật trạng thái nút
    }

    /// <summary>
    /// Tải dữ liệu cho các ComboBox (Danh mục, Màu sắc, Kích thước) từ database.
    /// </summary>
    public async Task LoadLookupsAsync()
    {
        try
        {
            // Lấy và sắp xếp dữ liệu
            var categories = (await _unitOfWork.ProductCategories.GetAllAsync()).OrderBy(x => x.CategoryName).ToList();
            var colors = (await _unitOfWork.ProductColors.GetAllAsync()).OrderBy(x => x.ColorName).ToList();
            var sizes = (await _unitOfWork.ProductSizes.GetAllAsync()).OrderBy(x => x.SizeName).ToList();

            // Cập nhật các ObservableCollection để ComboBox tự động cập nhật
            AvailableCategories.Clear();
            foreach (var c in categories) AvailableCategories.Add(c);

            AvailableColors.Clear();
            foreach (var c in colors) AvailableColors.Add(c);

            AvailableSizes.Clear();
            foreach (var s in sizes) AvailableSizes.Add(s);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi tải dữ liệu lookup cho Thêm/Sửa Sản phẩm: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Dữ Liệu", "Không thể tải danh mục, màu sắc hoặc size.");
        }
    }

    /// <summary>
    /// Tải dữ liệu của một sản phẩm đã tồn tại để hiển thị lên form chỉnh sửa.
    /// </summary>
    /// <param name="productId">ID của sản phẩm cần tải.</param>
    public async Task LoadProductAsync(int productId)
    {
        if (productId <= 0) return;

        _productIdToEdit = productId; // Lưu lại ID đang sửa
        PageTitle = "Chỉnh Sửa Sản Phẩm";
        IsSaving = true;
        _variantsPendingDeletion.Clear(); // Xóa danh sách chờ xóa cũ
        VariantsToAdd.Clear();          // Xóa danh sách biến thể hiển thị cũ

        try
        {
            // Tạo Specification để lấy sản phẩm và các biến thể liên quan
            var spec = new ProductSpecification();
            spec.AddCriteria(p => p.ProductId == productId);
            spec.IncludeVariantsWithDetails(); // Đảm bảo đã include variants, color, size
            var product = (await _unitOfWork.Products.GetAsync(spec)).FirstOrDefault();

            if (product != null)
            {
                // Điền thông tin cơ bản của sản phẩm
                ProductName = product.ProductName;
                ImportPrice = product.ImportPrice;
                Price = product.Price;
                SelectedCategory = AvailableCategories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
                ImageUrlPreview = product.PublicId; // Hiển thị ảnh từ DB (đường dẫn local hoặc URL)
                SelectedImagePath = null; // Reset đường dẫn ảnh cục bộ mới chọn

                // Load các biến thể chưa bị xóa mềm vào danh sách hiển thị
                foreach (var variant in product.ProductVariants.Where(v => !v.IsDeleted))
                {
                    VariantsToAdd.Add(new VariantEntryViewModel
                    {
                        VariantId = variant.VariantId, // Gán ID từ DB
                        Color = AvailableColors.FirstOrDefault(c => c.ColorId == variant.ColorId),
                        Size = AvailableSizes.FirstOrDefault(s => s.SizeId == variant.SizeId),
                        StockQuantity = variant.StockQuantity,
                        ParentViewModel = this // Gán ViewModel cha
                    });
                }
            }
            else
            {
                // Xử lý trường hợp không tìm thấy sản phẩm
                await ShowErrorDialogAsync("Lỗi Tải Sản Phẩm", $"Không tìm thấy sản phẩm với ID: {productId}");
                CancelOperation(); // Quay lại trang trước
            }
        }
        catch (Exception ex)
        {
            // Xử lý lỗi khi tải dữ liệu
            Debug.WriteLine($"Lỗi tải sản phẩm {productId}: {ex}");
            await ShowErrorDialogAsync("Lỗi Tải Sản Phẩm", $"Đã xảy ra lỗi khi tải sản phẩm: {ex.Message}");
            CancelOperation(); // Quay lại trang trước
        }
        finally
        {
            IsSaving = false; // Tắt loading
            UpdateAllCommandStates(); // Cập nhật trạng thái nút
        }
    }

    /// <summary>
    /// Thêm một biến thể mới dựa trên các giá trị đang chọn trong các ComboBox và NumberBox vào danh sách hiển thị VariantsToAdd.
    /// </summary>
    private async Task AddVariant()
    {
        if (!CanAddVariant()) return;

        // Lấy ID màu và size đang chọn
        var currentColorId = CurrentVariantColor!.ColorId;
        var currentSizeId = CurrentVariantSize!.SizeId;

        // Kiểm tra xem biến thể này có đang nằm trong danh sách chờ xóa không
        var variantPendingDeletion = _variantsPendingDeletion
            .FirstOrDefault(v => v.Color?.ColorId == currentColorId && v.Size?.SizeId == currentSizeId);

        if (variantPendingDeletion != null)
        {
            // Nếu tìm thấy -> "Hủy xóa": Xóa khỏi danh sách chờ xóa, cập nhật tồn kho và thêm lại vào danh sách hiển thị
            Debug.WriteLine($"Thêm lại biến thể (ID: {variantPendingDeletion.VariantId}) đang chờ xóa.");
            _variantsPendingDeletion.Remove(variantPendingDeletion);
            variantPendingDeletion.StockQuantity = CurrentVariantStock; // Cập nhật tồn kho nếu người dùng đã sửa
            VariantsToAdd.Add(variantPendingDeletion); // Thêm lại vào danh sách hiển thị

            // Reset các ô nhập liệu
            CurrentVariantColor = null;
            CurrentVariantSize = null;
            CurrentVariantStock = 0;
            return; // Hoàn thành, không cần tạo mới
        }

        // Nếu không chờ xóa, kiểm tra xem đã tồn tại trong danh sách đang hiển thị chưa
        var newVariantEntryCheck = new VariantEntryViewModel { Color = CurrentVariantColor, Size = CurrentVariantSize };
        // Sử dụng phương thức Equals đã override trong VariantEntryViewModel để kiểm tra trùng màu/size
        if (VariantsToAdd.Contains(newVariantEntryCheck))
        {
            Debug.WriteLine("Biến thể với Màu/Size này đã có trong danh sách hiển thị.");
            await ShowErrorDialogAsync("Lỗi Thêm Biến Thể", "Biến thể với màu sắc và kích thước này đã tồn tại trong danh sách.");
            return;
        }

        // Nếu không chờ xóa và không trùng -> Tạo ViewModel mới và thêm vào danh sách hiển thị
        var newVariantEntry = new VariantEntryViewModel
        {
            VariantId = 0, // ID = 0 vì là biến thể mới, chưa có trong DB
            Color = CurrentVariantColor,
            Size = CurrentVariantSize,
            StockQuantity = CurrentVariantStock,
            ParentViewModel = this // Gán tham chiếu ViewModel cha
        };
        VariantsToAdd.Add(newVariantEntry);

        // Reset các ô nhập liệu
        CurrentVariantColor = null;
        CurrentVariantSize = null;
        CurrentVariantStock = 0;
    }

    /// <summary>
    /// Được gọi bởi VariantEntryViewModel (thông qua RequestRemoveCommand) khi người dùng nhấn nút xóa trên một dòng biến thể.
    /// Xử lý việc xóa biến thể khỏi danh sách hiển thị và đánh dấu chờ xóa nếu cần.
    /// </summary>
    /// <param name="variantToRemove">ViewModel của biến thể cần xử lý xóa.</param>
    public void ProcessRemoveRequest(VariantEntryViewModel variantToRemove)
    {
        if (variantToRemove == null) return;

        if (variantToRemove.VariantId > 0) // Trường hợp 1: Đây là biến thể đã tồn tại trong DB (đang ở chế độ Edit)
        {
            Debug.WriteLine($"Đánh dấu biến thể đã tồn tại (ID: {variantToRemove.VariantId}) chờ xóa.");
            // Thêm vào danh sách chờ xóa (nếu chưa có) để xử lý khi Lưu
            if (!_variantsPendingDeletion.Contains(variantToRemove))
            {
                _variantsPendingDeletion.Add(variantToRemove);
            }
            // Xóa khỏi danh sách đang hiển thị trên UI
            VariantsToAdd.Remove(variantToRemove);
        }
        else // Trường hợp 2: Đây là biến thể mới (chỉ tồn tại trong ViewModel, chưa lưu vào DB)
        {
            Debug.WriteLine("Xóa biến thể mới khỏi danh sách ViewModel.");
            // Chỉ cần xóa khỏi danh sách đang hiển thị là đủ
            VariantsToAdd.Remove(variantToRemove);
        }
    }

    /// <summary>
    /// Mở hộp thoại cho phép người dùng chọn một file ảnh.
    /// </summary>
    private async Task SelectImageAsync()
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        // Chỉ cho phép chọn các định dạng ảnh phổ biến
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".gif");


        // Lấy handle (HWND) của cửa sổ hiện tại để hiển thị đúng FileOpenPicker
        Window? parentWindow = (Application.Current as App)?.MainWindow;
        if (parentWindow == null)
        {
            Debug.WriteLine("LỖI: Không thể lấy MainWindow để mở FileOpenPicker.");
            await ShowErrorDialogAsync("Lỗi", "Không thể xác định cửa sổ hiện tại.");
            return;
        }
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(parentWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd); // Khởi tạo picker với HWND

        try
        {
            // Mở hộp thoại chọn file
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Nếu người dùng chọn file thành công
                SelectedImagePath = file.Path; // Lưu đường dẫn file cục bộ đã chọn
                ImageUrlPreview = file.Path;   // Cập nhật ảnh preview bằng đường dẫn cục bộ
                Debug.WriteLine($"Đã chọn ảnh: {SelectedImagePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi chọn file: {ex.Message}");
            await ShowErrorDialogAsync("Lỗi Chọn Ảnh", "Không thể mở hộp thoại chọn ảnh. Vui lòng thử lại.");
        }
    }

    /// <summary>
    /// Thực hiện lưu sản phẩm (Thêm mới hoặc Cập nhật) vào database.
    /// </summary>
    private async Task SaveProductInternalAsync()
    {
        if (!CanSaveProduct())
        {
            await ShowErrorDialogAsync("Chưa Hợp Lệ", "Vui lòng kiểm tra lại thông tin sản phẩm. Tên, danh mục, giá bán (>0) là bắt buộc. Phải có ít nhất một phiên bản và giá nhập phải nhỏ hơn giá bán.");
            return;
        }

        IsSaving = true; // Bật trạng thái đang lưu
        bool success = false;
        string? imagePathToSave = null; // Đường dẫn ảnh cuối cùng sẽ được lưu vào DB

        await _unitOfWork.BeginTransactionAsync(); // Bắt đầu một transaction DB

        try
        {
            // --- Xác định đường dẫn ảnh cần lưu ---
            if (!string.IsNullOrWhiteSpace(SelectedImagePath))
            {
                // Người dùng vừa chọn ảnh mới -> sử dụng đường dẫn này
                imagePathToSave = SelectedImagePath;
                Debug.WriteLine($"Sử dụng đường dẫn ảnh cục bộ mới chọn: {imagePathToSave}");
            }
            else if (IsEditMode)
            {
                imagePathToSave = null; // Giữ nguyên đường dẫn ảnh cũ trong DB
            }
            else // không chọn ảnh
            {
                // Sử dụng ảnh placeholder
                imagePathToSave = "ms-appx:///Assets/StoreLogo.png";
                Debug.WriteLine($"Không chọn ảnh cho sản phẩm mới, dùng ảnh mặc định: {imagePathToSave}");
            }


            if (IsEditMode)
            {
                // --- Chế độ cập nhật sản phẩm ---
                // Tải sản phẩm hiện có cùng các biến thể của nó
                var spec = new ProductSpecification();
                spec.AddCriteria(p => p.ProductId == _productIdToEdit);
                spec.IncludeVariants(); // phải Include variants để xử lý
                var existingProduct = (await _unitOfWork.Products.GetAsync(spec)).FirstOrDefault()
                    ?? throw new InvalidOperationException($"Sản phẩm ID {_productIdToEdit} không tồn tại.");

                // --- 1. Xử lý các biến thể được đánh dấu chờ xóa ---
                if (_variantsPendingDeletion.Any())
                {
                    // Lấy ID của các biến thể cần xử lý xóa
                    var idsToDelete = _variantsPendingDeletion.Select(vm => vm.VariantId).ToList();
                    var variantsToRemoveFromCollection = existingProduct.ProductVariants
                                                                    .Where(v => idsToDelete.Contains(v.VariantId))
                                                                    .ToList();
                    foreach (var variantToDelete in variantsToRemoveFromCollection)
                    {
                        bool isInOrder = await _unitOfWork.OrderItems.AnyAsync(oi => oi.VariantId == variantToDelete.VariantId);
                        if (isInOrder)
                        {
                            // Soft delete: Chỉ sửa trạng thái
                            variantToDelete.IsDeleted = true;
                            Debug.WriteLine($"Soft deleting variant ID: {variantToDelete.VariantId}");
                        }
                        else
                        {
                            // Hard delete: XÓA KHỎI COLLECTION MẸ
                            existingProduct.ProductVariants.Remove(variantToDelete);
                            Debug.WriteLine($"Hard deleting variant ID: {variantToDelete.VariantId}");
                        }
                    }
                }
                _variantsPendingDeletion.Clear();

                // --- 2. Xử lý Thêm mới / Cập nhật các biến thể còn lại (hiển thị trên UI) ---
                // Tạo map các biến thể đang hoạt động trong DB để so sánh (key là Tuple ColorId, SizeId)
                var existingActiveVariantsMap = existingProduct.ProductVariants
                                           .Where(v => !v.IsDeleted) // Chỉ lấy cái chưa xóa mềm
                                           .ToDictionary(v => Tuple.Create(v.ColorId, v.SizeId));

                // Tạo map các biến thể đang hiển thị trên UI
                var viewModelVariantsMap = VariantsToAdd
                                            .ToDictionary(vm => Tuple.Create((int?)vm.Color!.ColorId, (int?)vm.Size!.SizeId));

                // Duyệt qua các biến thể trên UI để tìm cái cần Cập nhật hoặc Thêm mới
                foreach (var vmVariantKeyPair in viewModelVariantsMap)
                {
                    var vmKey = vmVariantKeyPair.Key;
                    var variantVM = vmVariantKeyPair.Value;

                    if (existingActiveVariantsMap.TryGetValue(vmKey, out var existingVariant))
                    {
                        // Tìm thấy trong DB -> Cần CẬP NHẬT (chỉ cập nhật giá trị tồn kho)
                        if (existingVariant.StockQuantity != variantVM.StockQuantity)
                        {
                            existingVariant.StockQuantity = variantVM.StockQuantity;
                            Debug.WriteLine($"Cập nhật tồn kho cho biến thể ID: {existingVariant.VariantId} thành {variantVM.StockQuantity}");
                        }
                        // Đánh dấu đã xử lý (xóa khỏi map)
                        existingActiveVariantsMap.Remove(vmKey);
                    }
                    else // Không tìm thấy trong DB -> Biến thể MỚI cần THÊM
                    {
                        var newVariant = new ProductVariant
                        {
                            ColorId = variantVM.Color!.ColorId,
                            SizeId = variantVM.Size!.SizeId,
                            StockQuantity = variantVM.StockQuantity,
                        };
                        existingProduct.ProductVariants.Add(newVariant);
                        Debug.WriteLine($"Xác định biến thể mới cần thêm: Màu={variantVM.Color.ColorName}, Size={variantVM.Size.SizeName}");
                    }
                }

                // --- 3. Cập nhật các thuộc tính của Sản phẩm ---
                existingProduct.ProductName = this.ProductName;
                existingProduct.ImportPrice = this.ImportPrice;
                existingProduct.Price = this.Price;
                existingProduct.CategoryId = this.SelectedCategory!.CategoryId;
                // Cập nhật đường dẫn ảnh nếu nó thay đổi so với DB
                if (imagePathToSave != null)
                {
                    existingProduct.PublicId = imagePathToSave ?? "ms-appx:///Assets/StoreLogo.png"; // Gán đường dẫn mới hoặc placeholder
                    Debug.WriteLine($"Cập nhật PublicId của sản phẩm thành: {existingProduct.PublicId}");
                }
            }
            else // Chế độ THÊM MỚI
            {
                // --- THỰC HIỆN THÊM SẢN PHẨM MỚI ---
                Debug.WriteLine("Đang thêm sản phẩm mới.");
                var newProduct = new Product
                {
                    ProductName = ProductName,
                    ImportPrice = ImportPrice,
                    Price = Price,
                    CategoryId = SelectedCategory!.CategoryId,
                    PublicId = imagePathToSave ?? "ms-appx:///Assets/StoreLogo.png", // Gán đường dẫn ảnh hoặc placeholder
                    IsDeleted = false,
                    ProductVariants = [] // Khởi tạo danh sách biến thể
                };

                // Thêm các biến thể từ danh sách trên UI vào sản phẩm mới
                foreach (var variantVM in VariantsToAdd)
                {
                    newProduct.ProductVariants.Add(new ProductVariant
                    {
                        ColorId = variantVM.Color!.ColorId,
                        SizeId = variantVM.Size!.SizeId,
                        StockQuantity = variantVM.StockQuantity,
                    });
                }
                await _unitOfWork.Products.AddAsync(newProduct);
            }

            // --- Bước 4: Lưu tất cả thay đổi vào Database ---
            success = await _unitOfWork.SaveChangesAsync();

            if (success)
            {
                await _unitOfWork.CommitTransactionAsync(); // Hoàn tất transaction thành công
                Debug.WriteLine("Lưu thành công và commit transaction.");
                await ShowSuccessDialogAsync("Lưu Thành Công", "Sản phẩm đã được lưu thành công.");
                SaveCompleted?.Invoke(this, EventArgs.Empty); // Báo hiệu cho View biết đã lưu xong để điều hướng
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync(); // Hoàn tác transaction
                await ShowErrorDialogAsync("Lỗi Lưu", "Không thể lưu thay đổi vào cơ sở dữ liệu (không có thay đổi nào được ghi nhận). Thay đổi đã được hoàn tác.");
            }
        }
        catch (DbUpdateException dbEx) // Bắt lỗi cụ thể khi cập nhật DB
        {
            await _unitOfWork.RollbackTransactionAsync(); // Hoàn tác transaction khi có lỗi DB
            Debug.WriteLine($"Lỗi lưu sản phẩm (DbUpdateException): {dbEx}\nInner: {dbEx.InnerException}");
            string errorDetails = dbEx.InnerException?.Message ?? dbEx.Message;
            // Hiển thị thông báo lỗi cụ thể hơn cho người dùng
            if (errorDetails.Contains("duplicate key value violates unique constraint"))
                await ShowErrorDialogAsync("Lỗi Lưu", $"Trùng lặp dữ liệu. Tên sản phẩm hoặc một phiên bản (màu/size) có thể đã tồn tại. Chi tiết: {errorDetails}. Thay đổi đã được hoàn tác.");
            else if (errorDetails.Contains("violates foreign key constraint"))
                await ShowErrorDialogAsync("Lỗi Lưu", $"Lỗi dữ liệu liên kết. Danh mục hoặc thông tin liên quan khác có thể không hợp lệ. Chi tiết: {errorDetails}. Thay đổi đã được hoàn tác.");
            else
                await ShowErrorDialogAsync("Lỗi Lưu", $"Lỗi cơ sở dữ liệu khi lưu: {errorDetails}. Thay đổi đã được hoàn tác.");
        }
        catch (Exception ex) // Bắt các lỗi không mong muốn khác
        {
            await _unitOfWork.RollbackTransactionAsync(); // Hoàn tác transaction khi có lỗi khác
            Debug.WriteLine($"Lỗi lưu sản phẩm: {ex}");
            await ShowErrorDialogAsync("Lỗi Lưu", $"Đã xảy ra lỗi không mong muốn: {ex.Message}. Thay đổi đã được hoàn tác.");
        }
        finally
        {
            IsSaving = false; // Luôn tắt trạng thái đang lưu
            UpdateAllCommandStates(); // Cập nhật trạng thái các nút
        }
    }

    /// <summary>
    /// Phát ra event Cancelled để báo hiệu cho View hủy bỏ và điều hướng về trang trước.
    /// </summary>
    private void CancelOperation() => Cancelled?.Invoke(this, EventArgs.Empty);
}