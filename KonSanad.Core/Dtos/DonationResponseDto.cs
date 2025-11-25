namespace KonSanad.Core.Dtos
{
    public class DonationResponseDto
    {
        public int DonationOrderId { get; set; }
        public string Message => "تم استلام تبرعك بنجاح، شكرًا لكرمك";
        public decimal? CashAmount { get; set; }
        public int TotalItems => Items.Sum(i => i.Quantity);
        public List<DonationItemDto> Items { get; set; } = new();
    }
}