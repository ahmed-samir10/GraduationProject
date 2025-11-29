using System;
using KonSanad.Core.Dtos;

namespace KonSanad.Core.Dtos
{
        public class CampaignDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string ShortDescription { get; set; } = string.Empty;

        public string PictureUrl { get; set; }
        // Category info for the card
        // public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

            public string Status { get; set; } = string.Empty; // نشطة - عاجلة - مكتملة
            public int ProgressPercentage { get; set; }
        public int DonorCount { get; set; }

        //public BeneficiaryDto Beneficiary { get; set; } = new BeneficiaryDto();
        //public List<SupplyDto> Supplies { get; set; } = new List<SupplyDto>();
    }
    
}