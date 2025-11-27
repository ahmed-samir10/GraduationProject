namespace KonSanad.Core.Dtos
{
    public class CreateDonationRequestDto
    {
        public int? CampaignId { get; set; }
        public int? HelpOrderId { get; set; }
        public decimal? CashAmount { get; set; }
        public List<DonationItemDto> Items { get; set; } = new();
    }
}