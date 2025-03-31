using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

    public LoginViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        // Use proper async loading method
        GetStoreOwnersAsync(unitOfWork);
    }

    private async void GetStoreOwnersAsync(IUnitOfWork unitOfWork)
    {
        var owners = await unitOfWork.StoreOwners.GetAllAsync();
        if (owners.Any())
        {
            _storeOwner = owners.First(); // Assume there is one store owner
            Debug.WriteLine($"Store owner: {_storeOwner.Username}");
            ManagePassword = new(_storeOwner, unitOfWork);
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
        Password == _unProtectedPassword;
}
