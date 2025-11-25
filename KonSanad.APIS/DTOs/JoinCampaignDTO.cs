using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class JoinCampaignDTO
    {
        [Required]
        public int CampaignId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PreferredRole { get; set; } = null!; // "مساعد ميداني", "منسق", "سائق"

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? AvailableDays { get; set; } // "السبت، الأحد، الاثنين"

        [MaxLength(100)]
        public string? AvailableTime { get; set; } // "صباحي 8-12"
    }
}
