// KonSanad.Repository\Entities\CashDonation.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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


    }
}