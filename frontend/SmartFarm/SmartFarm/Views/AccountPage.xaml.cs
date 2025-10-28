namespace SmartFarm.Views;

public partial class AccountPage : ContentPage
{
	public AccountPage()
	{
		InitializeComponent();
	}
    private void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        // Xóa trạng thái đăng nhập đã lưu
        Preferences.Clear(); 

        Application.Current.MainPage = new LoginPage();
    }
}