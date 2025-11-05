using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarm.Models
{
    internal class NotificationResponse
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public ObservableCollection<Note> Notifications { get; set; }
    }
}
