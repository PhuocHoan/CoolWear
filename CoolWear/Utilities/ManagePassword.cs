using CoolWear.Models;
using CoolWear.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CoolWear.Utilities;
public class ManagePassword(StoreOwner? storeOwner, IUnitOfWork unitOfWork)
{
    public async void ProtectPassword(string password)
    {
        if (storeOwner == null) return;

        var passwordInBytes = Encoding.UTF8.GetBytes(password);
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
        storeOwner.Entropy = entropyInBase64;
        storeOwner.Password = encryptedInBase64;
        await unitOfWork.StoreOwners.UpdateAsync(storeOwner);
        await unitOfWork.SaveChangesAsync();
    }

    public Tuple<string, string> UnprotectPassword()
    {
        if (storeOwner == null) return Tuple.Create<string, string>(null!, null!);

        var username = storeOwner.Username;
        var encryptedInBase64 = storeOwner.Password;
        var entropyInBase64 = storeOwner.Entropy;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(encryptedInBase64) ||
            string.IsNullOrEmpty(entropyInBase64)) return Tuple.Create<string, string>(null!, null!);

        try
        {
            var encryptedInBytes = Convert.FromBase64String(encryptedInBase64);
            var entropyInBytes = Convert.FromBase64String(entropyInBase64);
            var passwordInBytes = ProtectedData.Unprotect(
                encryptedInBytes,
                entropyInBytes,
                DataProtectionScope.CurrentUser
            );

            var password = Encoding.UTF8.GetString(passwordInBytes);
            return Tuple.Create(username, password);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error unprotecting password: {ex.Message}");
            return Tuple.Create<string, string>(null!, null!);
        }
    }
}
