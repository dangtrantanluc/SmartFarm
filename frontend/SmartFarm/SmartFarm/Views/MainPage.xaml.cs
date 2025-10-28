using SmartFarm.ViewModels;

namespace SmartFarm.Views;

public partial class MainPage : ContentPage
{
    private readonly WeatherViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new WeatherViewModel();
        BindingContext = _viewModel;

        // Goi api khi khoi dong trang
        _ = _viewModel.LoadWeatherAsync();
    }
}