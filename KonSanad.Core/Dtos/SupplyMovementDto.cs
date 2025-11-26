using System;

namespace KonSanad.Core.Dtos
{
    public class SupplyMovementDto
    {
        public int Id { get; set; }
        public string MovementType { get; set; } = null!;
        public int? MovedQuantity { get; set; }
        public DateTime? MovementDate { get; set; }

        public int? SupplyId { get; set; }
        public int? OrderItemId { get; set; }
        public int? BeneficiaryID { get; set; }
        public int? ManagerId { get; set; }
    }
}