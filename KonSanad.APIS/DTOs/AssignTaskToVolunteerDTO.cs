using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class AssignTaskToVolunteerDTO
    {
        [Required]
        public int VolunteerId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TaskTitle { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string TaskDescription { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = null!; // "Urgent", "High", "Medium", "Low"

        public DateTime? DueDate { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public int? EstimatedHours { get; set; }

        [MaxLength(200)]
        public string? RequiredSkills { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
