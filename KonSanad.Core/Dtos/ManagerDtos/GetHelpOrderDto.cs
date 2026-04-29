using KonSanad.Core.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    public class GetHelpOrderDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Tilte { get; set; } = string.Empty;
        public RequestedStatus HelpOrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
