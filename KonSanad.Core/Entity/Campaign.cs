// KonSanad.Repository\Entities\Campaign.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using KonSanad.Core.Entities;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Repository.Entities
{
    [Table("Campaigns")]
    public class Campaign : BaseEntity<int>
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? GoalCash { get; set; }
        public int? GoalSupplies { get; set; }

        public CampaignStatus CampaignStatus {  get; set; } = CampaignStatus.NotCompleted;
    

         public string? PictureUrl { get; set; }
        // Category FK
        public int? CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; } // Navigation property

       
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }  // Navigation property


        public int MaxBeneficiaries { get; set; } = 50;

        // Navigations
        public ICollection<DonationOrder> DonationOrders { get; set; } = new List<DonationOrder>();
        public ICollection<CampaignResult> CampaignResults { get; set; } = new List<CampaignResult>();
        public ICollection<Mission> Missions { get; set; } = new List<Mission>();
        public ICollection<CampaignVolunteer> CampaignVolunteers { get; set; } = new List<CampaignVolunteer>();


        public ICollection<BeneficiaryCampaign> BeneficiaryCampaigns { get; set; }

    }
}