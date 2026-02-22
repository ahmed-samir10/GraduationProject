using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity;

namespace KonSanad.Core.Dtos
{
    public class CampaignBeneficiaryDto : CampaignDto
    { 
        public CampaignStatus CampaignStatus { get; set; }
    }
}
