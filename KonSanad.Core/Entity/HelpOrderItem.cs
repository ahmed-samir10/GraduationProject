// KonSanad.Repository\Entities\HelpOrderItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("HelpOrderItem")]
    public class HelpOrderItem : BaseEntity<int>
    {
        public int QuantityRequested { get; set; }

        // This My Relations Between ( HelpOrder And HelpOrderItem ) One Order Have Many Items
        public int HelpOrderId { get; set; }
        [ForeignKey(nameof(HelpOrderId))]
        public HelpOrder? HelpOrder { get; set; } // My Navigation Property

        // This My Relations Between ( Supply And HelpOrderItem ) One Supply Can Be In Many Help OrderItem
        [Required]
        public int SupplyId { get; set; }
        [ForeignKey(nameof(SupplyId))]
        public Supply? Supply { get; set; } // My Navigation Property
    }
}