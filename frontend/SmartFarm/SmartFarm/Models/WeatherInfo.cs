using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarm.Models
{
    internal class WeatherInfo
    {
        public string City { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
        public int Humidity { get; set; }
        public double Wind_Speed { get; set; }
        public string Icon { get; set; }
        public string Local_Time { get; set; }

    }
}
