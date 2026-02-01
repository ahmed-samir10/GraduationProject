// KonSanad.Repository\Entities\Volunteer.cs
using KonSanad.Core.Entities;
using KonSanad.Core.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("Volunteers")]
    public class Volunteer : BaseEntity<int>
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Skills { get; set; } = null!;
        public string? AppUserId { get; set; }
        public bool? Availability { get; set; }
        public string? NationalId { get; set; }


        // Relationships Between Manager and Volunteer 
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }


        // Relationships Between Volunteer and CampaignVolunteer
        public ICollection<CampaignVolunteer> CampaignVolunteers { get; set; } = new List<CampaignVolunteer>();


        // Volunteer ↔ Mission (via VolunteerMission)
        public ICollection<MissionVolunteers> VolunteerMissions { get; set; }
            = new List<MissionVolunteers>();
    }
}