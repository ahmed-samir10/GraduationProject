using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public string CustomSupplyName { get; set; }
        public int Quantity { get; set; }

        public string Unit { get; set; }
        // public int DonationOrderId { get; set; }
        // public int SupplyId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}