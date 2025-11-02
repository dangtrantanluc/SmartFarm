using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarm.Models
{
    public class Notes 
    {
        public string Key { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; } 
        public DateTime TimeStamp { get; set; }
        
        public string TimeDisplay => TimeStamp.ToString("HH:mm");

    }
}
