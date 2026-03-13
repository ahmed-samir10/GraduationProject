using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos
{
    public class BeneficiaryJoinedCampaignDto : CampaignDto
    {
        public RequestedStatus campaignRequestStatus {  get; set; }
    }
}
