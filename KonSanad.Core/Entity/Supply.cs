// KonSanad.Repository\Entities\Supply.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonSanad.Core.Entities;

namespace KonSanad.Repository.Entities
{
    [Table("Supplies")]
    public class Supply : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public int? StockQuantity { get; set; }
        public int? MinimumLevel { get; set; }

        // Link to Category
        public int? CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<SupplyMovement> SupplyMovements { get; set; } = new List<SupplyMovement>();
    }
}