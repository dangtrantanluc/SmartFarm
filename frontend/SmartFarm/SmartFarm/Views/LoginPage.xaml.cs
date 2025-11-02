using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using SmartFarm.Services;

namespace SmartFarm.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly FirebaseAuthService _authService = new FirebaseAuthService();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = UsernameEntry.Text?.Trim();
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin.", "OK");
                return;
            }
            try
            {
                var (success, message, uid) = await _authService.LoginAsync(email, password);

                if (success)
                {
                    //await DisplayAlert("Thành công", "Đăng nhập thành công!", "OK");
                    // Lưu uid để dùng sau
                    Preferences.Set("user_uid", uid);
                    // Điều hướng sang trang chính
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await DisplayAlert("Thất bại", message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể đăng nhập: {ex.Message}", "OK");
            }
        }

        private async void OnRegisterRedirect(object sender, EventArgs e)
        {
           // await Shell.Current.GoToAsync(nameof(RegisterPage));
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}