namespace KonSanad.APIS.DTOs
{
    public class VolunteerListItemDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public int Age { get; set; }
        public string? Skills { get; set; }
        public bool IsAvailable { get; set; }
        public int ActiveCampaigns { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalVolunteerHours { get; set; }
        public string Status { get; set; } = null!; // "Active", "Inactive"
    }
}
