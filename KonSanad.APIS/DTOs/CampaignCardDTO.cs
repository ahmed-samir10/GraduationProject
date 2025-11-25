namespace KonSanad.APIS.DTOs
{
    public class CampaignCardDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public decimal? GoalCash { get; set; }
        public decimal CurrentCash { get; set; }
        public int PercentageComplete { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? CategoryName { get; set; }
        public bool IsUrgent { get; set; }
        public string Status { get; set; } = null!; // "Active", "Completed", "Pending"
    }
}
