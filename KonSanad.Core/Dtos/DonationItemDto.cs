using Microsoft.AspNetCore.Http;

namespace KonSanad.Core.Dtos
{
    public class DonationItemDto
    {
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
        public string? CustomName { get; set; }
        public string ImageFile { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

}