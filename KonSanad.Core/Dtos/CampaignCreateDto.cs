
using System;

namespace KonSanad.Core.Dtos
{
    public class CampaignCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? GoalCash { get; set; }
        public int? GoalSupplies { get; set; }
        public string PictureUrl { get; set; }
        public int? CategoryId { get; set; }
        public int? ManagerId { get; set; }

        // Optional image path or external url if you support images
        public string? ImageUrl { get; set; }
    }
}
