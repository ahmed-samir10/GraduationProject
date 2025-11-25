namespace KonSanad.APIS.DTOs
{
    public class VolunteerOverviewDTO
    {
        public int ActiveCampaigns { get; set; }
        public int PendingTasks { get; set; }
        public int CompletedTasksThisMonth { get; set; }
        public int TotalVolunteerHours { get; set; }
        public int ReceivedItemsCount { get; set; }
        public DateTime? NextTaskDueDate { get; set; }
        public string? NextTaskTitle { get; set; }
    }
}
