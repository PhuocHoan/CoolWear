using CoolWear.Models;
using CoolWear.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public class LoginViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private StoreOwner? _storeOwner;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; } = false;

    public LoginViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ = GetStoreOwnersAsync(unitOfWork);
    }

    private async Task GetStoreOwnersAsync(IUnitOfWork unitOfWork)
    {
        var owners = await unitOfWork.StoreOwners.GetAllAsync();
        _storeOwner = owners.First(); // Assume there is one store owner
    }

    public bool CanLogin() => Username != null && Password != null;

    public bool Login() => Username == _storeOwner.Username && Password == _storeOwner.Password;

    public void ProtectPassword()
    {
        var passwordInBytes = Encoding.UTF8.GetBytes(Password);
        var entropyInBytes = new byte[20];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(entropyInBytes);
        }
        var encryptedInBytes = ProtectedData.Protect(
            passwordInBytes,
            entropyInBytes,
            DataProtectionScope.CurrentUser
        );
        var encryptedInBase64 = Convert.ToBase64String(encryptedInBytes);
        var entropyInBase64 = Convert.ToBase64String(entropyInBytes);
        var localStorage = Windows.Storage.ApplicationData.Current.LocalSettings;
        localStorage.Values["Username"] = Username;
        localStorage.Values["Password"] = encryptedInBase64;
        localStorage.Values["Entropy"] = entropyInBase64;
    }

    public void UnprotectPassword()
    {
        var localStorage = Windows.Storage.ApplicationData.Current.LocalSettings;
        var username = (string)localStorage.Values["Username"];
        var encryptedInBase64 = (string)localStorage.Values["Password"];
        var entropyInBase64 = (string)localStorage.Values["Entropy"];
        if (username == null || encryptedInBase64 == null || entropyInBase64 == null) return;
        var encryptedInBytes = Convert.FromBase64String(encryptedInBase64);
        var entropyInBytes = Convert.FromBase64String(entropyInBase64);
        var passwordInBytes = ProtectedData.Unprotect(
            encryptedInBytes,
            entropyInBytes,
            DataProtectionScope.CurrentUser
        );
        Password = Encoding.UTF8.GetString(passwordInBytes);
        Username = username;
    }
}
