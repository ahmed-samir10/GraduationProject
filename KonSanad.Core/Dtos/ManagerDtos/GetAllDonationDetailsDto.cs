using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    public class GetAllDonationDetailsDto : GetAllDonationDto
    {
        //view More Details
       
        public string? CampaignDescription { get; set; }

        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 
        public string? ImageFile { get; set; }
        public string? Unit { get; set; }
        public string? SupplyDescription { get; set; }

        public string? SupplyStatus { get; set; }

        public string? RejectReason { get; set; }   // خاصه بالمدير 
    }
}
