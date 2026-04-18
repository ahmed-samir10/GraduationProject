using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    public class GetHelpOrderWithItemsDto
    {
        // ✅ بيانات الـ HelpOrder الأساسية - بترجع دايماً
        public int HelpOrderId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public RequestedStatus HelpOrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalItems { get; set; }

        // ✅ بيانات الـ Items - بترجع بس لو في Items
        // لو فاضية = الـ HelpOrder ملوش Items
        public List<GetHelpOrderItemDto> Items { get; set; } = new();
    }
}
