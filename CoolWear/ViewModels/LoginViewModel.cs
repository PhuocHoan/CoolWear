using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using CoolWear.Views;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private StoreOwner? _storeOwner;
    public ManagePassword? ManagePassword { get; private set; }

    // Thêm các trường hỗ trợ cho các thuộc tính
    private string _username = "";
    private string _password = "";
    private bool _rememberMe = false;
    private string _unProtectedPassword = "";

    // Sử dụng các thuộc tính với SetProperty để thông báo thay đổi
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    public async Task InitializeDataAsync()
    {
        var owners = await _unitOfWork.StoreOwners.GetAllAsync();
        if (owners.Any())
        {
            _storeOwner = owners.First();
            Debug.WriteLine($"Chủ cửa hàng: {_storeOwner.Username}");

            ManagePassword = new(_storeOwner, _unitOfWork);
            var result = ManagePassword.UnprotectPassword();
            Username = result.Item1 ?? "";
            Password = result.Item2 ?? "";
            _unProtectedPassword = Password;
        }
        else
        {
            Debug.WriteLine("Không tìm thấy chủ cửa hàng nào");
        }
    }

    public bool CanLogin() => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    public bool Login() =>
        _storeOwner != null &&
        Username == _storeOwner.Username &&
        (Password == _unProtectedPassword || Password == "123"); // Mật khẩu mặc định là 123 hoặc mật khẩu đã lưu

    public async Task LoginAsync()
    {
        Debug.WriteLine(Username + " " + Password);

        if (CanLogin())
        { // Có thể thực hiện - Kiểm tra trước khi thực hiện
            bool success = Login(); // Thực hiện

            if (RememberMe == true)
            {
                ManagePassword!.ProtectPassword(Password);

                Debug.WriteLine($"Mật khẩu đã mã hóa dưới dạng base 64 là: {Password}");
            }

            if (success)
            {
                var dashboardWindow = new DashboardWindow();

                if (Application.Current is App app)
                {
                    app.MainWindow!.Close(); // Đóng cửa sổ đăng nhập
                    app.SetMainWindow(dashboardWindow); // Thông báo cho App về cửa sổ chính mới
                }
                else
                {
                    Debug.WriteLine("LỖI: Không thể ép kiểu Application.Current thành App để đặt cửa sổ chính.");
                }

                dashboardWindow.Activate(); // Kích hoạt cửa sổ mới
            }
            else
            {
                await ShowErrorDialogAsync("Đăng Nhập Thất Bại", "Tài khoản hoặc mật khẩu không đúng. Vui lòng thử lại.");
            }
        }
    }
}
