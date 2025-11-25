// KonSanad.Repository\Entities\Receipt.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("Receipts")]
    public class Receipt : BaseEntity<int>
    {

        // Relationships Between Receipt And DonationOrder
        public int? DonationOrderId { get; set; }
        [ForeignKey(nameof(DonationOrderId))]
        public DonationOrder? DonationOrder { get; set; } // Navigation Property


        //  Relationships Between Receipt And Manager
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; } // Navigation Property


        // Relationships Between Receipt And Beneficiary
        public int? BeneficiaryId { get; set; }
        [ForeignKey(nameof(BeneficiaryId))]
        public Beneficiary? Beneficiary { get; set; } // Navigation Property


        // Relationships Between Receipt And Donor
        public int? DonorId { get; set; }
        [ForeignKey(nameof(DonorId))]
        public Donor? Donor { get; set; } // Navigation Property


        // Relationships Between Receipt And CashDonation
        public int? CashDonationId { get; set; }
        [ForeignKey(nameof(CashDonationId))]
        public CashDonation? CashDonation { get; set; }

        // Shoud Recape This AmountCash Because We Can Get It From CashDonation Table
        // public decimal? AmountCash { get; set; }


        // Shoud Recape This AmountInKind Because We Can Get CreatedAt
        // public DateTime dateTime { get; set; }
    }
}