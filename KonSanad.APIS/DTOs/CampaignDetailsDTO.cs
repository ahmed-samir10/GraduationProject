namespace KonSanad.APIS.DTOs
{
    public class CampaignDetailsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Financial Info
        public decimal? GoalCash { get; set; }
        public decimal CurrentCashCollected { get; set; }
        public int PercentageComplete { get; set; }

        // Supplies Info
        public int? GoalSupplies { get; set; }
        public int CurrentSuppliesCollected { get; set; }

        // Category
        public string? CategoryName { get; set; }

        // Manager
        public string? ManagerName { get; set; }

        // Statistics
        public int TotalDonors { get; set; }
        public int TotalDonations { get; set; }

        // Investment/Spending Info
        public List<CampaignSpendingDTO>? Spendings { get; set; }
    }
}
