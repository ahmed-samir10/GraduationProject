namespace KonSanad.Core.Dtos
{
    public class CreateDonationRequestDto
    {
        public int? CampaignId { get; set; }                    // اختياري: لو تبرع لحملة معينة
        public int? HelpOrderId { get; set; }                   // اختياري: لو تبرع لطلب مساعدة مباشر
        public decimal? CashAmount { get; set; }                // لو تبرع نقدي
        public List<DonationItemDto> Items { get; set; } = new(); // لو تبرع عيني
    }
}