using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class RequiredItemDTO
    {
        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = null!; // "Piece", "Box", "Kg", "Liter"

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal? EstimatedUnitCost { get; set; }
    }
}
