// KonSanad.Repository\Entities\DonationOrder.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Repository.Entities
{
    [Table("DonationOrders")]
    public class DonationOrder : BaseEntity<int>
    {
        public string DonationCategoryStatus { get; set; } = null!;
        public decimal? CashAmount { get; set; }
        public RequestedStatus? DonationRequestStatus { get; set; } = RequestedStatus.Pending;
        public DateTime? OrderDate { get; set; }

    
        public int? DonorId { get; set; }
        [ForeignKey(nameof(DonorId))]
        public Donor? Donor { get; set; }

        // ✅ HelpOrderId بس من غير navigation property
        public int? HelpOrderId { get; set; }

        // ✅ CampaignId مع الـ ForeignKey صح
        public int? CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public Campaign? Campaign { get; set; }

        // Manager
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
        public CashDonation? CashDonation { get; set; }
    }
}