namespace KonSanad.APIS.DTOs
{
    public class VolunteerReadDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Age { get; set; }
        public string? Occupation { get; set; }
        public string? Skills { get; set; }
        public bool HasTransportation { get; set; }

        // إحصائيات المتطوع
        public int TotalCampaignsJoined { get; set; }
        public int ActiveCampaigns { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalVolunteerHours { get; set; }
    }
}
