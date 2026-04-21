using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    // ✅ DTO جديد علشان يمثل كل Item جوا الـ HelpOrder لما المدير يعمل فيلتر
    public class HelpOrderItemDetailsDto
    {
        public int ItemId { get; set; }
        public string Tilte { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }         
        public RequestedStatus HelpOrderItemStatus { get; set; } 
    }
}
