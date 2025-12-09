namespace KonSanad.Core.Dtos
{
   
    public class DonationResponseCashDto
    {
       
        public decimal? CashAmount { get; set; }
     
        public string Message { get; set; } = string.Empty;

        // returned receipt ids (nullable depending on which operations occurred)
        public int? CashReceiptId { get; set; }
      
    }
    public class DonationResponseItemDto
    {
     
        public int TotalItems { get; set; }
        public List<DonationItemDto> Items { get; set; } = new();
        public string Message { get; set; } = string.Empty;

        // returned receipt ids (nullable depending on which operations occurred)


    }

}