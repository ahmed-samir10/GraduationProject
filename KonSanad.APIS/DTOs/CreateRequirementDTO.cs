using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class CreateRequirementDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal? EstimatedCost { get; set; }

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = null!; // "Urgent", "High", "Medium", "Low"

        [MaxLength(100)]
        public string? Category { get; set; } // "Medical", "Educational", "Housing", "Food", "Other"

        public List<RequiredItemDTO>? RequiredItems { get; set; }

        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
