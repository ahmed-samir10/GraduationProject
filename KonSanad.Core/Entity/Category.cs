using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonSanad.Repository.Entities;

namespace KonSanad.Core.Entities
{
    [Table("Categories")]
    public class Category : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }


        // Common relationships ( Supply , Campaign )
        public ICollection<Supply> Supplies { get; set; } = new List<Supply>();
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
        
    }
}
