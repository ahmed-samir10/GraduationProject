using System;
using System.Collections.Generic;

namespace KonSanad.Core.Dtos
{
    public class DonationOrderDto
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? CashAmount { get; set; }
        public string? Status { get; set; }
        public string? DonationCategoryStatus { get; set; }
        public int? CampaignId { get; set; }
        public int? ManagerId { get; set; }

        // Item donations (can be empty)
        public List<DonationItemDto> Items { get; set; } = new List<DonationItemDto>();

        // Any related receipt ids
        public List<int> ReceiptIds { get; set; } = new List<int>();
    }
}