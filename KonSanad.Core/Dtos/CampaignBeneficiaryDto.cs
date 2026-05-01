using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos
{
    public class CampaignBeneficiaryDto : CampaignDto
    { 
        public CampaignStatus CampaignStatus { get; set; }
        
    }
    public class CampaignMangerDto : CampaignDto
    {
        public CampaignStatus CampaignStatus { get; set; }
        public int CountBen { get; set; }
        public int CountVolunteer { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
