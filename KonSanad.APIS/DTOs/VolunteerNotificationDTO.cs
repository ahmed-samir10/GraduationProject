namespace KonSanad.APIS.DTOs
{
    public class VolunteerNotificationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!; // "TaskAssigned", "CampaignUpdate", "ItemReceived"
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
