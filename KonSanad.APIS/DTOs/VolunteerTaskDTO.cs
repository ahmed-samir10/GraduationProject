namespace KonSanad.APIS.DTOs
{
    public class VolunteerTaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!; // "Pending", "InProgress", "Completed", "Cancelled"
        public string Priority { get; set; } = null!; // "Urgent", "High", "Medium", "Low"

        // معلومات الحملة المرتبطة
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;

        // التواريخ
        public DateTime AssignedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        // تفاصيل إضافية
        public string? Location { get; set; }
        public int? EstimatedHours { get; set; }
        public string? RequiredSkills { get; set; }
    }
}
