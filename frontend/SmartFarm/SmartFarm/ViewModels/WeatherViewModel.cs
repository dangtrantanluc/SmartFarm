using SmartFarm.Models;
using SmartFarm.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SmartFarm.ViewModels
{
    public class WeatherViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService;

        private string _city = "Ho Chi Minh";
        private string _temperature;
        private string _humidity;
        private string _windSpeed;
        private string _description;
        private string _icon;

        public event PropertyChangedEventHandler PropertyChanged;

        public WeatherViewModel()
        {
            _weatherService = new WeatherService();
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged();
            }
        }

        public string Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemperatureAndDescription));
            }
        }

        public string Humidity
        {
            get => _humidity;
            set
            {
                _humidity = value;
                OnPropertyChanged();
            }
        }

        public string WindSpeed
        {
            get => _windSpeed;
            set
            {
                _windSpeed = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemperatureAndDescription));
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDate => DateTime.Now.ToString("dddd, dd MMMM yyyy");

        public string TemperatureAndDescription => $"{Temperature} {Description}";

        //Ham load api  
        public async Task LoadWeatherAsync()
        {
            var weather = await _weatherService.GetWeatherAsync(City);

            if (weather != null)
            {
                Temperature = $"{weather.Temperature}°C";
                Humidity = $"{weather.Humidity}%";
                WindSpeed = $"{weather.Wind_Speed} km/h";
                Description = weather.Description;
                Icon = weather.Icon switch
                {
                    "01d" => "sunny.png",
                    "02d" => "cloudy.png",
                    "09d" => "rain.png",
                    _ => "default_icon.jpg"
                };
            }
            else
            {
                Description = "Không lấy được dữ liệu!";
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
