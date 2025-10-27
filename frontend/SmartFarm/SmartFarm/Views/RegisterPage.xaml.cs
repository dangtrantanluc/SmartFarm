using System.Net.Http.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SmartFarm.Services;
using Newtonsoft.Json;  

namespace SmartFarm.Views
{
    public partial class RegisterPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyBi5JE3OPeMB9sftxE98_NWZ3TlExrcTas";
        private const string FirebaseDatabaseUrl = "https://smartfarm-d4d95-default-rtdb.firebaseio.com/";

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string name = UsernameEntry.Text?.Trim();

            string password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin.", "OK");
                return;
            }

            // Ở đây name cũng là email đăng ký
            string email = name; 

            try
            {
                // Gọi Firebase Auth API để tạo user
                var signupResult = await SignUpWithEmailPassword(email, password);


                if (signupResult == null)
                {
                    await DisplayAlert("Lỗi", "Không thể tạo tài khoản.", "OK");
                    return;
                }

                string uid = signupResult.localId;
                string idToken = signupResult.idToken;

                // Lưu thông tin user vào Firebase Database
                await SaveUserToDatabase(uid, email.Split('@')[0], email, idToken);

                await DisplayAlert("Thành công", "Đăng ký tài khoản thành công!", "OK");

                // Chuyển sang trang Login
                await Navigation.PushAsync(new LoginPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        private async Task<dynamic> SignUpWithEmailPassword(string email, string password)
        {
            using (var client = new HttpClient())
            {
                var signupUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}";

                var payload = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

                var response = await client.PostAsync(signupUrl, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Firebase Auth lỗi: " + responseJson);
                }

                return JsonConvert.DeserializeObject(responseJson);
            }
        }

        private async Task SaveUserToDatabase(string uid, string name, string email, string idToken)
        {
            using (var client = new HttpClient())
            {
                var userData = new
                {
                    uId = uid,
                    name = name,
                    mail = email,
                    work_details = new { } // chưa có task nào
                };

                var json = JsonConvert.SerializeObject(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var dbUrl = $"{FirebaseDatabaseUrl}/users/{uid}.json?auth={idToken}";
                var response = await client.PutAsync(dbUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Không thể lưu user vào Database: " + await response.Content.ReadAsStringAsync());
                }
            }
        }

        private async void OnLoginRedirect(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
