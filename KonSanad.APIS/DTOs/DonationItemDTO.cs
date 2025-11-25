using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class DonationItemDTO
    {
        [Required]
        public int SupplyId { get; set; }

        [Required]
        public string CustomSupplyName { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string Unit { get; set; } = null!;

        public string? Condition { get; set; } // "New", "Used - Good", "Used - Fair"
        public string? Description { get; set; }
    }
}
