namespace KonSanad.APIS.DTOs
{
    public class PendingBeneficiaryRequestDTO
    {
        public int Id { get; set; }
        public string BeneficiaryName { get; set; } = null!;
        public int BeneficiaryId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal? EstimatedCost { get; set; }
        public string Priority { get; set; } = null!; // "Urgent", "High", "Medium", "Low"
        public string Status { get; set; } = null!; // "Pending", "UnderReview", "Approved", "Rejected"
        public DateTime RequestDate { get; set; }
        public string? Category { get; set; }
        public List<RequiredItemDTO>? RequiredItems { get; set; }
    }
}
