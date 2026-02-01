// KonSanad.Repository\Entities\Mission.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonSanad.Core.Entity;

namespace KonSanad.Repository.Entities
{
    [Table("Missions")]
    public class Mission : BaseEntity<int>
    {
        // Allow nulls because DB contains nulls for some rows
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        // Manager

        public MissionStatus MissionStatus { get; set; }
        public int? ManagerId { get; set; }
        public Manager? Manager { get; set; }

        // Campaign
        public int? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }

        // HelpOrder
        public int? HelpOrderId { get; set; }
        public HelpOrder? HelpOrder { get; set; }

        // Mission ↔ Volunteer (via VolunteerMission)
        public ICollection<MissionVolunteers> VolunteerMissions { get; set; }
            = new List<MissionVolunteers>();
    }
}