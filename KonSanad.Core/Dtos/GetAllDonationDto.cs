using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos
{
    public class GetAllDonationDto
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public int? SupplyId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public RequestedStatus DonationStatus { get; set; }
        public DonationCategory DonationCategory { get; set; }
        public decimal AmountCashDonation { get; set; }
        public string? CampaignTitle { get; set; }
        public string SupplyName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }

      


    }
}
