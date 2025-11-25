namespace KonSanad.APIS.DTOs
{
    public class VolunteerProfileDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Age { get; set; }
        public string? Occupation { get; set; }
        public string? Skills { get; set; }
        public string? AvailabilityTime { get; set; }
        public string? Bio { get; set; }
        public bool HasTransportation { get; set; }

        // إحصائيات الملف الشخصي
        public int TotalCampaignsJoined { get; set; }
        public int ActiveCampaigns { get; set; }
        public int CompletedCampaigns { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalVolunteerHours { get; set; }
        public DateTime JoinedDate { get; set; }

        // الحملات المنضم إليها
        public List<VolunteerCampaignDTO>? JoinedCampaigns { get; set; }

        // المهام الحالية
        public List<VolunteerTaskDTO>? CurrentTasks { get; set; }
    }
}
