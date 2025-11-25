namespace KonSanad.APIS.DTOs
{
    public class DonationDetailsDTO
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public string DonationType { get; set; } = null!;
        public string Status { get; set; } = null!;

        // Campaign Info
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;

        // Cash Donation
        public decimal? CashAmount { get; set; }

        // Item Donations
        public List<OrderItemReadDTO>? Items { get; set; }

        // Receipt Info
        public int? ReceiptId { get; set; }
        public DateTime? ReceiptDate { get; set; }
    }
}
