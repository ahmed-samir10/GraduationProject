namespace KonSanad.APIS.DTOs
{
    public class PendingVolunteerRequestDTO
    {
        public int Id { get; set; }
        public int VolunteerId { get; set; }
        public string VolunteerName { get; set; } = null!;
        public string VolunteerEmail { get; set; } = null!;
        public string VolunteerPhone { get; set; } = null!;
        public int Age { get; set; }
        public string? Skills { get; set; }
        public string? Occupation { get; set; }
        public bool HasTransportation { get; set; }

        // تفاصيل طلب الانضمام
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public string PreferredRole { get; set; } = null!;
        public string? AvailableDays { get; set; }
        public string? AvailableTime { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = null!; // "Pending", "Approved", "Rejected"
        public string? Notes { get; set; }
    }
}
