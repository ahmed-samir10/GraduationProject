namespace KonSanad.APIS.DTOs
{
    public class VolunteerCampaignDTO
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public string? CampaignImageUrl { get; set; }
        public string Status { get; set; } = null!; // "Active", "Completed"
        public DateTime JoinedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // دور المتطوع في الحملة
        public string Role { get; set; } = null!; // "مساعد ميداني", "منسق", "سائق"
        public int TasksAssigned { get; set; }
        public int TasksCompleted { get; set; }
        public int HoursContributed { get; set; }
    }
}
