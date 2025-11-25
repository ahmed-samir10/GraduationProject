using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class CreateCashDonationDTO
    {
        [Required]
        public int CampaignId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? Notes { get; set; }
    }
}
