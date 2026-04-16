using KonSanad.Repository.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Core.Entity
{
    [Table("HelpOrderFiles")]
    public class HelpOrderFile : BaseEntity<int>
    {
        public int HelpOrderId { get; set; }

        [ForeignKey(nameof(HelpOrderId))]
        public HelpOrder HelpOrder { get; set; } = null!;

        public string FileUrl { get; set; } = string.Empty;
        public string? FileName { get; set; }
    }
}