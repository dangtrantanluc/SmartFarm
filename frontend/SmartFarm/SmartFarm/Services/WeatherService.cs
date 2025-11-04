using SmartFarm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarm.Services
{   
    class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.88.51:8000")// địa chỉ API FastAPI
            };
        }

        public async Task<WeatherInfo?> GetWeatherAsync(string city) //tự động đọc JSON và ánh xạ vào class WeatherInfo
        {
            try
            {
                string url = $"/weather/weather?city={Uri.EscapeDataString(city)}";
                return await _httpClient.GetFromJsonAsync<WeatherInfo>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gọi API: {ex.Message}");
                return null;
            }
        }
    }
}
