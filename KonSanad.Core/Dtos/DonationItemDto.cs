using Microsoft.AspNetCore.Http;

namespace KonSanad.Core.Dtos
{
    public class DonationItemDto
    {

            public int Id { get; set; }
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
             public DateTime? OrderDate { get; set; }
             public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
              public string? CustomName { get; set; }
        public string ImageFile { get; set; }
              public string Unit { get; set; } = string.Empty;
              public decimal? CashAmount { get; set; }
        // Item donations (can be empty)
        public List<DonationItemDto> Items { get; set; } = new List<DonationItemDto>();

        // Any related receipt ids
        public List<int> ReceiptIds { get; set; } = new List<int>();
    }

}