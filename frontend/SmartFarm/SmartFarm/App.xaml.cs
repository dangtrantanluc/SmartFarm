
using SmartFarm.Views;
using Microsoft.Maui.Controls;
namespace SmartFarm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            string uid = Preferences.Get("user_uid", string.Empty);

            Page initialPage;
            if (string.IsNullOrEmpty(uid))
            {
                // Chưa đăng nhập: mở trang đăng nhập
                initialPage = new NavigationPage(new LoginPage());
            }
            else
            {
                // Đã đăng nhập: mở trang chính
                initialPage = new AppShell();
            }

            return new Window(initialPage);
        }
    }
}