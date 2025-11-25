// KonSanad.Repository\Entities\Mission.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("Missions")]
    public class Mission : BaseEntity<int>
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }

        public int? VolunteerId { get; set; }
        [ForeignKey(nameof(VolunteerId))]
        public Volunteer? Volunteer { get; set; }

        public int? CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public Campaign? Campaign { get; set; }


    }
}