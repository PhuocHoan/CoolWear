using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CoolWear.ViewModels;

public partial class AccountViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private ManagePassword _managePassword;
    private StoreOwner? _storeOwner;

    private string _ownerName = "";
    private string _email = "";
    private string _phone = "";
    private string _address = "";

    private string _oldPassword = "";
    private string _newPassword = "";
    private string _repeatPassword = "";


    public string OwnerName
    {
        get => _ownerName;
        set => SetProperty(ref _ownerName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string OldPassword
    {
        get => _oldPassword;
        set => SetProperty(ref _oldPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string RepeatPassword
    {
        get => _repeatPassword;
        set => SetProperty(ref _repeatPassword, value);
    }

    public AccountViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

    }

    public async Task LoadAccountInfoAsync()
    {
        var owners = await _unitOfWork.StoreOwners.GetAllAsync();
        if (owners.Any())
        {
            _storeOwner = owners.First();
            _managePassword = new ManagePassword(_storeOwner, _unitOfWork);


            Debug.WriteLine($"Store owner: {_storeOwner.OwnerName}");

            OwnerName = _storeOwner.OwnerName ?? string.Empty;
            Email = _storeOwner.Email ?? string.Empty;
            Phone = _storeOwner.Phone ?? string.Empty;
            Address = _storeOwner.Address ?? string.Empty;
        }
        else
        {
            Debug.WriteLine("No store owners found");
        }
    }

    public async Task SaveAccountInfoAsync()
    {
        if (_storeOwner == null)
        {
            Debug.WriteLine("No store owner loaded to save changes.");
            return;
        }

        _storeOwner.OwnerName = OwnerName;
        _storeOwner.Email = Email;
        _storeOwner.Phone = Phone;
        _storeOwner.Address = Address;

        try
        {
            await _unitOfWork.StoreOwners.UpdateAsync(_storeOwner);
            var success = await _unitOfWork.SaveChangesAsync();

            if (success)
            {
                Debug.WriteLine("Account information saved successfully.");
                ShowSuccessDialogAsync("Success", "Đổi thông tin thành công.");
            }
            else
            {
                Debug.WriteLine("Failed to save account information.");
                await ShowErrorDialogAsync("Error", "Đổi thông tin thất bại.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving account information: {ex.Message}");
        }
    }

    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword, string repeatPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(repeatPassword))
        {
            await ShowErrorDialogAsync("Error", "Vui lòng điền đủ thông tin.");
            return false;
        }

        if (newPassword != repeatPassword)
        {
            await ShowErrorDialogAsync("Error", "Mật khẩu mới và nhập lại mật khẩu không trùng.");
            return false;
        }

        var storedPassword = _managePassword.UnprotectPassword().Item2;
        if (string.IsNullOrEmpty(storedPassword))
        {
            throw new InvalidOperationException("Stored password is invalid.");
        }

        //Debug.WriteLine($"Stored password: {storedPassword}");
        //Debug.WriteLine($"Old password: {oldPassword}");
        //Debug.WriteLine($"New password: {newPassword}");
        //Debug.WriteLine($"Repeat password: {repeatPassword}");

        if (storedPassword != oldPassword) 
        {
            await ShowErrorDialogAsync("Error", "Mật khẩu cũ không đúng.");
            return false;
        }

        _managePassword.ProtectPassword(newPassword);


         _unitOfWork.SaveChangesAsync();
         ShowSuccessDialogAsync("Success", "Đổi mật khẩu thành công.");
         return true;
    }
}
