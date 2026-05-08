
using KonSanad.Core.Entities;
using KonSanad.Core.Entity.Enums;
using Microsoft.AspNetCore.Http;
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

        public CampaignCategory CategoryId { get; set; }
        public int MaxBenificary { get; set; } = 50;


        // Optional image path or external url if you support images
        public IFormFile? ImageUrl { get; set; }
    }
}
