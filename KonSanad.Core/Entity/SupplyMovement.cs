// KonSanad.Repository\Entities\SupplyMovement.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("SuppliesMovement")]
    public class SupplyMovement : BaseEntity<int>
    {
     

        public int? SupplyId { get; set; }
        [ForeignKey(nameof(SupplyId))]
        public Supply? Supply { get; set; }

        public int? OrderItemId { get; set; }
        [ForeignKey(nameof(OrderItemId))]
        public OrderItem? OrderItem { get; set; }

        public int? BeneficiaryID { get; set; }
        [ForeignKey(nameof(BeneficiaryID))]
        // Navigation Property
        public Beneficiary? Beneficiary { get; set; }

        // could be enum in future

        //public int? MovementSourceType { get; set; }

        //public int? MovementSourceId { get; set; }

        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }

        public string MovementType { get; set; } = null!;

        public double? MovedQuantity { get; set; }

        public DateTime? MovementDate { get; set; }

     
    }
}