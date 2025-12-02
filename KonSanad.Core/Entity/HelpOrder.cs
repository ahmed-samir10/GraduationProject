// KonSanad.Repository\Entities\HelpOrder.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("HelpOrders")]
    public class HelpOrder : BaseEntity<int>
    {
        public string Description { get; set; } = null!;

        
        public int BeneficiaryId { get; set; }
       
        [ForeignKey(nameof(BeneficiaryId))]
        public Beneficiary? Beneficiary { get; set; } // My Navigation Property

        // This My Relations Between ( Manager And HelpOrder )
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; } // My Navigation Property

        // metadata
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<HelpOrderItem> HelpOrderItems { get; set; } = new List<HelpOrderItem>();
    }
}