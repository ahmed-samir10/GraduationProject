namespace KonSanad.Core.Dtos
{
   
    public class DonationResponseDto
    {
        public int DonationOrderId { get; set; }
        public decimal? CashAmount { get; set; }
        public int TotalItems { get; set; }
        public List<DonationItemDto> Items { get; set; } = new();
        public string Message { get; set; } = string.Empty;

        // returned receipt ids (nullable depending on which operations occurred)
        public int? CashReceiptId { get; set; }
        public int? ItemsReceiptId { get; set; }
    }
}