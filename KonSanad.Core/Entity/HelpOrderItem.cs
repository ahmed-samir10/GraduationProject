using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("HelpOrderItems")]  // الأفضل جمع
    public class HelpOrderItem : BaseEntity<int>
    {
        [Required]
        public int QuantityRequested { get; set; }

        // علاقة: One HelpOrder -> Many HelpOrderItems
        [Required]
        public int HelpOrderId { get; set; }

        [ForeignKey(nameof(HelpOrderId))]
        public HelpOrder HelpOrder { get; set; } = null!;

        // علاقة: One Supply -> Many HelpOrderItems
        [Required]
        public int SupplyId { get; set; }
        public string reason { get; set; } = string.Empty; 

        [ForeignKey(nameof(SupplyId))]
        public Supply Supply { get; set; } = null!;

        // الصورة (اختياري)
        public string? ImageUrl { get; set; }
    }
}
