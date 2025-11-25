namespace KonSanad.APIS.DTOs
{
    public class TopVolunteerDTO
    {
        public int VolunteerId { get; set; }
        public string VolunteerName { get; set; } = null!;
        public int TotalHours { get; set; }
        public int CampaignsJoined { get; set; }
        public int TasksCompleted { get; set; }
    }
}
