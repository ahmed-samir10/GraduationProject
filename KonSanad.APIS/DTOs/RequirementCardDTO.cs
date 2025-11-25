namespace KonSanad.APIS.DTOs
{
    public class RequirementCardDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string Priority { get; set; } = null!; // "Urgent", "High", "Medium", "Low"
        public string Status { get; set; } = null!; // "Pending", "InProgress", "Fulfilled"
        public int? BeneficiaryId { get; set; }
        public string? BeneficiaryName { get; set; }
    }
}
