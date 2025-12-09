using Microsoft.AspNetCore.Http;

namespace KonSanad.Core.Dtos
{
    public class 
        
        CreateDonationItemRequestDto
    {
        public int? CampaignId { get; set; }
        public int? HelpOrderId { get; set; }
      
        public IFormFile? ImageFile { get; set; }
        
        public List<DonationItemDto> Items { get; set; } = new();
    }
    public class CreateDonationCashRequestDto
    {
   public int? CampaignId { get; set; }
    public int? HelpOrderId { get; set; }
    public decimal? CashAmount { get; set; }

}
}