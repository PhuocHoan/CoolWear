using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CoolWear.Views;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; set; }

    public LoginWindow()
    {
        this.InitializeComponent();
        ViewModel = new LoginViewModel();

        var localStorage = Windows.Storage.ApplicationData.Current.LocalSettings;
        var username = (string)localStorage.Values["Username"];
        var encryptedInBase64 = (string)localStorage.Values["Password"];
        var entropyInBase64 = (string)localStorage.Values["Entropy"];

        if (username == null) return;

        var encryptedInBytes = Convert.FromBase64String(encryptedInBase64);
        var entropyInBytes = Convert.FromBase64String(entropyInBase64);

        var passwordInBytes = ProtectedData.Unprotect(
            encryptedInBytes,
            entropyInBytes,
            DataProtectionScope.CurrentUser
        );
        var password = Encoding.UTF8.GetString(passwordInBytes);
        ViewModel.Password = password;
        ViewModel.Username = username;
    }

    private void loginButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine(ViewModel.Username + " " + ViewModel.Password);

        if (ViewModel.CanLogin())
        { // CanExecute - Look before you leap
            bool success = ViewModel.Login(); // Execute

            if (ViewModel.RememberMe == true)
            {
                var passwordInBytes = Encoding.UTF8.GetBytes(ViewModel.Password);
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
                localStorage.Values["Username"] = ViewModel.Username;
                localStorage.Values["Password"] = encryptedInBase64;
                localStorage.Values["Entropy"] = entropyInBase64;

                Debug.WriteLine($"Encrypted password in base 64 is: {encryptedInBase64}");
            }

            if (success)
            {
                var screen = new DashboardWindow();
                screen.Activate();

                this.Close();
            }
        }
    }
}
