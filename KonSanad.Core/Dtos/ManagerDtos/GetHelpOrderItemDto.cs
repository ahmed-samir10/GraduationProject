using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    // يظهر كل HelpOrderItem بتاع HelpOrder معين
    public class GetHelpOrderItemDto
    {
        public int ItemId { get; set; }
        public int HelpOrderId { get; set; }
        public int SupplyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }
        public string? ImageUrl { get; set; }
        public RequestedStatus HelpOrderItemStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
