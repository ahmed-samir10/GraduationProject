using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{
    public class BeneficiaryCampaign : BaseEntity<int>
    {
        
        public int BeneficiaryId { get; set; }
        public Beneficiary Beneficiary { get; set; }

        public int CampaignId { get; set; }
        public Campaign Campaign { get; set; }

        public CampaignRequestStatus campaignRequestStatus { get; set; }  = CampaignRequestStatus.Pending; // 👈 قيمة افتراضية
        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }

}
