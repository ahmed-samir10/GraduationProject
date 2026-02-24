// KonSanad.Repository\Entities\HelpOrder.cs
using KonSanad.Core.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("HelpOrders")]
    public class HelpOrder : BaseEntity<int>
    {
      

        // The status of the help order (Pending, Rejected, Approved)
        public HelpOrderStatus HelpOrderStatus { get; set; } = HelpOrderStatus.Pending;

        // This My Relations Between ( Beneficiary And HelpOrder )
        public int BeneficiaryId { get; set; }
       
        [ForeignKey(nameof(BeneficiaryId))]
        public Beneficiary? Beneficiary { get; set; } 


        // This My Relations Between ( Manager And HelpOrder )
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager? Manager { get; set; }


        // This My Relations Between ( HelpOrder And HelpOrderItems )
        public ICollection<HelpOrderItem> HelpOrderItems { get; set; } = new List<HelpOrderItem>();
    }
}