using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoolWear.Views;

public sealed partial class CategoriesPage : Page
{
    private static readonly Converters.InverseBoolConverter InverseBoolConverter = new();
    public CategoryViewModel ViewModel { get; }

    public CategoriesPage()
    {
        InitializeComponent();

        try
        {
            ViewModel = ServiceManager.GetKeyedSingleton<CategoryViewModel>();
            DataContext = ViewModel;
            ViewModel.RequestShowDialog += ShowAddEditCategoryDialogAsync;
        }
        catch (Exception) { }
    }

    // Hủy đăng ký khi rời khỏi trang để tránh memory leak
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel != null)
        {
            ViewModel.RequestShowDialog -= ShowAddEditCategoryDialogAsync;
        }
    }

    // Tải dữ liệu ban đầu khi điều hướng đến trang
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine("CategoriesPage: OnNavigatedTo");

        if (ViewModel != null && !ViewModel.IsLoading)
        {
            await ViewModel.InitializeDataAsync();
        }
        else if (ViewModel == null)
        {
            Debug.WriteLine("LỖI: ViewModel bị null trong OnNavigatedTo.");
        }
    }

    /// <summary>
    /// Phương thức được gọi bởi ViewModel để hiển thị ContentDialog Add/Edit.
    /// </summary>
    /// <returns>Kết quả người dùng nhấn nút (Primary, Secondary, None).</returns>
    private async Task<ContentDialogResult> ShowAddEditCategoryDialogAsync()
    {
        // --- TẠO NỘI DUNG CHO DIALOG BẰNG CODE ---
        var contentPanel = new StackPanel { Spacing = 10 };

        // TextBox Tên Danh Mục
        var nameTextBox = new TextBox
        {
            Header = "Tên Danh Mục (*)",
            MaxLength = 50
        };
        // Binding Text vào ViewModel.DialogCategoryName
        nameTextBox.SetBinding(TextBox.TextProperty, new Binding
        {
            Source = ViewModel, // Nguồn binding là ViewModel
            Path = new PropertyPath(nameof(ViewModel.DialogCategoryName)), // Thuộc tính cần binding
            Mode = BindingMode.TwoWay, // Cho phép nhập liệu cập nhật ViewModel
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged // Cập nhật ngay khi gõ
        });
        contentPanel.Children.Add(nameTextBox);

        // TextBox Loại Sản Phẩm
        var typeTextBox = new TextBox
        {
            Header = "Loại Sản Phẩm (*)",
            MaxLength = 20,
            PlaceholderText = "Ví dụ: Áo, Quần"
        };
        // Binding Text vào ViewModel.DialogProductType
        typeTextBox.SetBinding(TextBox.TextProperty, new Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.DialogProductType)),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        contentPanel.Children.Add(typeTextBox);

        // ProgressRing (hiển thị khi đang lưu)
        var savingProgressRing = new ProgressRing { HorizontalAlignment = HorizontalAlignment.Center };
        savingProgressRing.SetBinding(ProgressRing.IsActiveProperty, new Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.IsDialogSaving)),
            Mode = BindingMode.OneWay // Chỉ đọc trạng thái từ ViewModel
        });
        contentPanel.Children.Add(savingProgressRing);
        // --- KẾT THÚC TẠO NỘI DUNG ---

        // --- TẠO VÀ CẤU HÌNH CONTENT DIALOG ---
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot, // Gán XamlRoot
            Title = ViewModel.DialogTitle, // Lấy Title từ ViewModel
            PrimaryButtonText = "Lưu",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            Content = contentPanel // Gán StackPanel làm nội dung
        };

        // Binding IsPrimaryButtonEnabled vào !IsDialogSaving
        dialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.IsDialogSaving)),
            Mode = BindingMode.OneWay,
            Converter = InverseBoolConverter // Sử dụng converter đã tạo
        });

        // --- HIỂN THỊ DIALOG VÀ XỬ LÝ KẾT QUẢ ---
        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Gọi hàm lưu trong ViewModel
            _ = await ViewModel.SaveCategoryAsync();
        }

        return result; // Trả về kết quả (Primary/Secondary/None)
    }
}