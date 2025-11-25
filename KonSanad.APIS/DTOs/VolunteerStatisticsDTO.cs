namespace KonSanad.APIS.DTOs
{
    public class VolunteerStatisticsDTO
    {
        public int TotalVolunteers { get; set; }
        public int ActiveVolunteers { get; set; }
        public int TotalVolunteerHours { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int TotalCampaignsSupported { get; set; }

        // توزيع حسب المهارات
        public Dictionary<string, int> VolunteersBySkills { get; set; } = new();

        // توزيع حسب الأعمار
        public Dictionary<string, int> VolunteersByAgeGroup { get; set; } = new();

        // المتطوعون الأكثر نشاطاً
        public List<TopVolunteerDTO> TopVolunteers { get; set; } = new();
    }
}
