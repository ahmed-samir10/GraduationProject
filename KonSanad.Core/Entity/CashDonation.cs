// KonSanad.Repository\Entities\CashDonation.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Repository.Entities
{
    [Table("CashDonation")]
    public class CashDonation : BaseEntity<int>
    {
        // Relationships Between CashDonation And DonationOrder
        public int DonationOrderId { get; set; }
        [ForeignKey(nameof(DonationOrderId))]
        public DonationOrder? DonationOrder { get; set; } // Navigation Property

        [Required]
        public decimal Amount { get; set; }
        public int? CampaignId { get; set; }
        public Campaign? Campaign { get; set; } // Navigation Property
        public int? HelpOrderId { get; set; }
        public int? receiptId { get; set; }
        [ForeignKey(nameof(receiptId))]
        public Receipt? Receipt { get; set; } = null!; // Navigation Property


        //CashDonationStatus 
        public RequestedStatus cashDonationStatus {  get; set; } // = RequestedStatus.Pending; // مؤقتاً نعملها ولا اي؟





    }
}