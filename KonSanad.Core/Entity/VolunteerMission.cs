using KonSanad.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("VolunteerMission")]
    public class VolunteerMission : BaseEntity<int>
    {
        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? ResultMission { get; set; }
    }
}
