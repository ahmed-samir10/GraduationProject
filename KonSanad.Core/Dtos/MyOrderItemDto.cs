using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity;

namespace KonSanad.Core.Dtos
{
    public class MyOrderItemDto
    {
        public int Id { get; set; }
        public int DonationOrderId { get; set; }
        public int? SupplyId { get; set; }
        public string? CustomSupplyName { get; set; }
        public int Quantity { get; set; }
        public string? Category { get; set; } = string.Empty;
        public string? Unit { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        //OrderItemStatus
        public OrderItemStatus orderItemStatus { get; set; }  
    }
}
