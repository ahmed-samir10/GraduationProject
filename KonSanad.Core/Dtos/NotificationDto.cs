using KonSanad.Core.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class NotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public RequestedStatus Status { get; set; } 
        public string Type { get; set; } // Success / Info / Warning
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
