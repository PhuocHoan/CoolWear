// In LoginWindow.xaml.cs
using CoolWear.Services;
using CoolWear.ViewModels;
using CoolWear.Views; // Add this if not present
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace CoolWear.Views;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; } = ServiceManager.GetKeyedSingleton<LoginViewModel>();

    public LoginWindow()
    {
        InitializeComponent();

        // Set the title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    public void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine(ViewModel.Username + " " + ViewModel.Password);

        if (ViewModel.CanLogin())
        { // CanExecute - Look before you leap
            bool success = ViewModel.Login(); // Execute

            if (ViewModel.RememberMe == true)
            {
                ViewModel.ManagePassword!.ProtectPassword(ViewModel.Password); // Use for change password, first, type the wanted password, click remember me, then click the button. Rerun the program and login successfully.

                Debug.WriteLine($"Encrypted password in base 64 is: {ViewModel.Password}");
            }

            if (success)
            {
                var dashboardWindow = new DashboardWindow();

                if (Application.Current is App app)
                {
                    app.SetMainWindow(dashboardWindow); // Tell the App about the new main window
                }
                else
                {
                    Debug.WriteLine("ERROR: Could not cast Application.Current to App to set main window.");
                }

                dashboardWindow.Activate(); // Activate the new window

                Close(); // Close the current login window
            }
            else
            {
                // Optional: Show login failed message
                ShowLoginFailedDialog(); // Implement this method if needed
            }
        }
        else
        {
            // Optional: Indicate why login can't proceed (e.g., empty fields)
        }
    }
    // Optional helper for failed login
    private async void ShowLoginFailedDialog()
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = "Đăng Nhập Thất Bại",
            Content = "Tài khoản hoặc mật khẩu không đúng. Vui lòng thử lại.",
            CloseButtonText = "Đóng",
            XamlRoot = this.Content.XamlRoot // Use LoginWindow's XamlRoot here
        };
        await dialog.ShowAsync();
    }
}
