// KonSanad.Repository\Entities\OrderItem.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("OrderItems")]
    public class OrderItem : BaseEntity<int>
    {
        public string CustomSupplyName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;

        public int DonationOrderId { get; set; }
        [ForeignKey(nameof(DonationOrderId))]
        public DonationOrder? DonationOrder { get; set; }

        public int SupplyId { get; set; }
        [ForeignKey(nameof(SupplyId))]
        public Supply? Supply { get; set; }



    }
}