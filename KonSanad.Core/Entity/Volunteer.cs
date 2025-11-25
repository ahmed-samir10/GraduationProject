// KonSanad.Repository\Entities\Volunteer.cs
using KonSanad.Core.Entities;
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
        public bool? Availability { get; set; }


        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }

        // Navigation
        public ICollection<CampaignVolunteer> CampaignVolunteers { get; set; } = new List<CampaignVolunteer>();
        public ICollection<Mission> Missions { get; set; } = new List<Mission>();
    }
}