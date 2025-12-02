using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class JoinCampaignRequestDto
    {
       // public int BeneficiaryId { get; set; }
        public int CampaignId { get; set; }
        public string Reason { get; set; }
    }
}
