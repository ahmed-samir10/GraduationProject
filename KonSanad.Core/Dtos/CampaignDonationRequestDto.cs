using System;
using System.Collections.Generic;
using KonSanad.Core.Entity.Enums;
namespace KonSanad.Core.Dtos
{
    public class CampaignDonationRequestDto
    {
        public int DonationOrderId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Core.Entity.Enums.RequestedStatus Status { get; set; }
        public List<DonationItemDetailDto> Items { get; set; } = new List<DonationItemDetailDto>();
    }
}
