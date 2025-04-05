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

    // Add backing fields for properties
    private string _username = "";
    private string _password = "";
    private bool _rememberMe = false;
    private string _unProtectedPassword = "";

    // Use properties with SetProperty for change notification
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

        _ = InitializeDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        var owners = await _unitOfWork.StoreOwners.GetAllAsync();
        if (owners.Any())
        {
            _storeOwner = owners.First();
            Debug.WriteLine($"Store owner: {_storeOwner.Username}");

            ManagePassword = new(_storeOwner, _unitOfWork);
            var result = ManagePassword.UnprotectPassword();
            Username = result.Item1 ?? "";
            Password = result.Item2 ?? "";
            _unProtectedPassword = Password;
        }
        else
        {
            Debug.WriteLine("No store owners found");
        }
    }

    public bool CanLogin() => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    public bool Login() =>
        _storeOwner != null &&
        Username == _storeOwner.Username &&
        (Password == _unProtectedPassword || Password == "123"); // Password default is 123 or the one stored

    public async Task LoginAsync()
    {
        Debug.WriteLine(Username + " " + Password);

        if (CanLogin())
        { // CanExecute - Look before you leap
            bool success = Login(); // Execute

            if (RememberMe == true)
            {
                ManagePassword!.ProtectPassword(Password);

                Debug.WriteLine($"Encrypted password in base 64 is: {Password}");
            }

            if (success)
            {
                var dashboardWindow = new DashboardWindow();

                if (Application.Current is App app)
                {
                    app.MainWindow!.Close(); // Close the login window
                    app.SetMainWindow(dashboardWindow); // Tell the App about the new main window
                }
                else
                {
                    Debug.WriteLine("ERROR: Could not cast Application.Current to App to set main window.");
                }

                dashboardWindow.Activate(); // Activate the new window
            }
            else
            {
                await ShowErrorDialogAsync("Đăng Nhập Thất Bại", "Tài khoản hoặc mật khẩu không đúng. Vui lòng thử lại.");
            }
        }
    }
}
