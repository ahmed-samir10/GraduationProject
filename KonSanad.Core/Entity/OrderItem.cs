// KonSanad.Repository\Entities\OrderItem.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace KonSanad.Repository.Entities
{
    public class OrderItem : BaseEntity<int>
    {
        public int DonationOrderId { get; set; }
        [ForeignKey(nameof(DonationOrderId))]
        public DonationOrder? DonationOrder { get; set; }

        // Make SupplyId nullable to allow custom (non-catalog) items
        public int? SupplyId { get; set; }
        [ForeignKey(nameof(SupplyId))]
        public Supply? Supply { get; set; }

        public string? CustomSupplyName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; } = null!;
    }
}