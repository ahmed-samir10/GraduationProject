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
     
        public int QuantityItems { get; set; }
        public List<object> Items { get; set; } = new();
        public string Message { get; set; } = string.Empty;

        // returned receipt ids (nullable depending on which operations occurred)


    }

}