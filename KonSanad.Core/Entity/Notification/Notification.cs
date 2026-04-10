using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity.Notification
{
    public class Notification:BaseEntity<int>
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // Info, Success, Warning

        public string UserId { get; set; }   // المستلم
        public bool IsSeen { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
