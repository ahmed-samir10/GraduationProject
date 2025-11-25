namespace KonSanad.APIS.DTOs
{
    public class RequirementSummaryDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal? EstimatedCost { get; set; }
        public string Priority { get; set; } = null!; // "Urgent", "High", "Medium", "Low"
        public string Status { get; set; } = null!; // "Pending", "InProgress", "Fulfilled", "Cancelled"
        public DateTime CreatedAt { get; set; }
        public DateTime? FulfilledAt { get; set; }
        public int? LinkedCampaignId { get; set; }
        public string? LinkedCampaignTitle { get; set; }
    }
}
