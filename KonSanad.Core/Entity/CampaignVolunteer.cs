using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entities
{
    public class CampaignVolunteer : BaseEntity<int>
    {
        // Foreign Key to Campaign
        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public Campaign? Campaign { get; set; }

        // Foreign Key to Volunteer
        public int VolunteerId { get; set; }
        [ForeignKey(nameof(VolunteerId))]
        public Volunteer? Volunteer { get; set; }

        // Additional Properties (Optional)
        public DateTime? JoinedDate { get; set; }
        public string? Status { get; set; } // مثال: "Active", "Completed", "Pending"
        public string? Notes { get; set; }
    }
}
