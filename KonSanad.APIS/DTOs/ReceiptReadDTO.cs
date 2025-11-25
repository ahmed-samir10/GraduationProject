namespace KonSanad.APIS.DTOs
{
    public class ReceiptReadDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        // Donation Info
        public int? DonationOrderId { get; set; }
        public decimal? CashAmount { get; set; }
        public string DonationType { get; set; } = null!;

        // Campaign Info
        public string? CampaignTitle { get; set; }

        // Donor Info
        public string DonorName { get; set; } = null!;

        // Manager Info
        public string? ManagerName { get; set; }
    }
}
