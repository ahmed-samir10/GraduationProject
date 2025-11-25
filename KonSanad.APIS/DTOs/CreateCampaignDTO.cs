using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class CreateCampaignDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? GoalCash { get; set; }

        [Range(0, int.MaxValue)]
        public int? GoalSupplies { get; set; }

        public int? CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsUrgent { get; set; } = false;

        // ربط الحملة بمستفيد معين (optional)
        public int? BeneficiaryId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
