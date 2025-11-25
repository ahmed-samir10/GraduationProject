using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class CreateItemDonationDTO
    {
        [Required]
        public int CampaignId { get; set; }

        [Required]
        public List<DonationItemDTO> Items { get; set; } = new();

        public string? Notes { get; set; }
    }
}
