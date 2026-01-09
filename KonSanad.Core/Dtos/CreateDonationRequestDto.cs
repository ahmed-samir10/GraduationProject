using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KonSanad.Core.Dtos
{
    


   public class CreateDonationItemRequestDto
    {
        public int? CampaignId { get; set; }
        public int? HelpOrderId { get; set; }
        

        [Required(ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        [MinLength(1, ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<DonationItemDto> Items { get; set; } = new List<DonationItemDto>();
    }
    public class CreateDonationCashRequestDto
    {
   public int? CampaignId { get; set; }
    public int? HelpOrderId { get; set; }
    public decimal? CashAmount { get; set; }

}
}