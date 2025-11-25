namespace KonSanad.APIS.DTOs
{
    public class DonationSummaryDTO
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public string DonationType { get; set; } = null!; // "Cash" or "InKind"
        public decimal? CashAmount { get; set; }
        public int? ItemsCount { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
