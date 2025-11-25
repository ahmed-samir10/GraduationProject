namespace KonSanad.APIS.DTOs
{
    public class CampaignSpendingDTO
    {
        public string Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public int Percentage { get; set; }
    }
}
