using System.ComponentModel.DataAnnotations;

namespace KonSanad.APIS.DTOs
{
    public class CreateCampaignFromRequestDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal? GoalCash { get; set; }
        public int? GoalSupplies { get; set; }

        public int? CategoryId { get; set; }
    }
}
